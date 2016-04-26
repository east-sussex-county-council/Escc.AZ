using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Escc.FormControls.WebForms;
using Escc.Web.Metadata;
using Exceptionless;
using Microsoft.ApplicationBlocks.Data;

namespace Escc.AZ.Admin
{
    /// <summary>
    /// Manage the data for one heading in the A-Z
    /// </summary>
    /// <remarks>
    /// <para>Sorting ESCC services works as follows:</para>
    /// <list type="bullet">
    /// <item>The services are sorted correctly by the stored procedure when they're read from the database</item>
    /// <item>The service ids are placed into the <c>sortEsccServices</c> hidden field in order</item>
    /// <item>The services related to the heading are sorted according to the contents of the <c>sortEsccServices</c> field, and displayed in that order</item>
    /// <item>If someone clicks an up or down button, the IDs are shuffled in the <c>sortEsccServices</c> field, and the services are sorted again 
    /// according to the contents of that field and displayed in that order</item>
    /// <item>When the Save button is clicked, the current order is saved to the database</item>
    /// </list>
    /// <para>Note that District and Borough services are not sorted and always appear after ESCC services. 
    /// This is because they're reimported each night with new IDs so the sort order would be reset.</para>
    /// </remarks>
    public partial class heading : Page
    {
        AZHeading headingToEdit;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            DataTable[] data;
            IList<AZHeading> headingData;
            List<AZService> serviceData;

            if (!IsPostBack)
            {
                // if not postback, check a querystring specified with a heading id
                try
                {
                    // get heading id from querystring - conversion ensures it's a number
                    if (Request.QueryString["heading"] != null) this.headingId.Value = Convert.ToInt32(Request.QueryString["heading"], CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                }
                catch (FormatException ex)
                {
                    // just redirect to default page if querystring is bad
                    ex.ToExceptionless().Submit();
                    Response.Redirect("default.aspx");
                    Response.End();
                }

                // work to do only when editing (ie not new heading)
                bool isNew = (this.headingId.Value.Length == 0);
                if (!isNew)
                {

                    // check whether it's a delete request
                    if (Request.QueryString["removerelated"] != null || Request.QueryString["removeservice"] != null)
                    {
                        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString);

                        try
                        {
                            // is it a remove heading request?
                            if (Request.QueryString["removerelated"] != null)
                            {
                                // set up two heading ids as parameters
                                SqlParameter[] sqlParams = new SqlParameter[2];
                                sqlParams[0] = new SqlParameter("@headingId", SqlDbType.Int, 4);
                                sqlParams[0].Value = this.headingId.Value;
                                sqlParams[1] = new SqlParameter("@relatedHeadingId", SqlDbType.Int, 4);
                                sqlParams[1].Value = Convert.ToInt32(Request.QueryString["removerelated"].ToString(), CultureInfo.InvariantCulture);

                                // update db
                                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "usp_DeleteRelatedHeading", sqlParams);
                            }

                                // is it a remove service request?
                            else if (Request.QueryString["removeservice"] != null)
                            {
                                // set up heading id and service id as parameters
                                SqlParameter[] sqlParams = new SqlParameter[2];
                                sqlParams[0] = new SqlParameter("@headingId", SqlDbType.Int, 4);
                                sqlParams[0].Value = this.headingId.Value;
                                sqlParams[1] = new SqlParameter("@serviceId", SqlDbType.Int, 4);
                                sqlParams[1].Value = Convert.ToInt32(Request.QueryString["removeservice"].ToString(), CultureInfo.InvariantCulture);

                                // update db
                                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "usp_DeleteHeadingFromService", sqlParams);
                            }
                        }
                        finally
                        {
                            // always close connection
                            conn.Close();
                        }
                    }

                }

                // get the initial data on the heading from the db
                data = this.GetData();
                headingData = BuildHeadingCollection(data[0]);
                serviceData = BuildServiceCollection(data[1]);
                this.headingToEdit = isNew ? new AZHeading() : BuildHeadingToEdit(data[2]);

                // bind the data to the form
                this.PopulateForm(headingData, serviceData, headingToEdit, false);
            }
            else
            {
                // When sort button is clicked, need to retrieve services in order to recreate buttons and 
                // hook up the click event
                data = this.GetData();
                headingData = BuildHeadingCollection(data[0]);
                serviceData = BuildServiceCollection(data[1]);
                this.headingToEdit = (this.headingId.Value.Length == 0) ? new AZHeading() : BuildHeadingToEdit(data[2]);
                this.PopulateForm(headingData, serviceData, headingToEdit, true);
            }
            // set page heading
            this.SetPageHeading();
        }

        /// <summary>
        /// Sets the page title and heading to say whether we're editing or creating a record
        /// </summary>
        private void SetPageHeading()
        {
            this.headContent.Title = (this.headingId.Value.Length > 0) ? "Edit A&#8211;Z heading" : "New A&#8211;Z heading";
            this.h1.InnerHtml = this.headContent.Title;
        }

        /// <summary>
        /// Get raw data from database based on heading id
        /// </summary>
        /// <returns>Raw data in DataTables</returns>
        private DataTable[] GetData()
        {
            // create container for data, and disconnected connection details
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString))
            {
                DataTable[] tables = new DataTable[3];

                // get from the db all headings 
                tables[0] = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "usp_SelectAllHeadings").Tables[0];
                tables[1] = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "usp_SelectAllServices").Tables[0];

                // get from the db any headings already related to this one
                if (!String.IsNullOrEmpty(this.headingId.Value))
                {
                    SqlParameter[] sqlParams = new SqlParameter[1];
                    sqlParams[0] = new SqlParameter("@headingId", SqlDbType.Int, 4);
                    sqlParams[0].Value = Convert.ToInt32(this.headingId.Value, CultureInfo.InvariantCulture);

                    tables[2] = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "usp_SelectHeadingForEdit", sqlParams).Tables[0];
                }

                return tables;
            }
        }

        /// <summary>
        /// Builds the heading collection from raw heading data
        /// </summary>
        /// <param name="headingData">The heading data.</param>
        /// <returns></returns>
        private static IList<AZHeading> BuildHeadingCollection(DataTable headingData)
        {
            IList<AZHeading> headings = new List<AZHeading>();
            foreach (DataRow row in headingData.Rows)
            {
                AZHeading heading = new AZHeading();
                heading.Id = Int32.Parse(row["HeadingId"].ToString(), CultureInfo.InvariantCulture);
                heading.Heading = row["Heading"].ToString();
                headings.Add(heading);
            }
            return headings;
        }

        /// <summary>
        /// Builds the service collection from raw service data
        /// </summary>
        /// <param name="serviceData">The service data.</param>
        /// <returns></returns>
        private static List<AZService> BuildServiceCollection(DataTable serviceData)
        {
            List<AZService> services = new List<AZService>();
            foreach (DataRow row in serviceData.Rows)
            {
                AZService service = new AZService();
                service.Id = Int32.Parse(row["ServiceId"].ToString(), CultureInfo.InvariantCulture);
                service.Service = row["Service"].ToString();
                service.Authority = ((Authority)Enum.Parse(typeof(Authority), row["Authority"].ToString()));
                services.Add(service);
            }
            return services;
        }

        /// <summary>
        /// Create a heading object from the raw data returned by the db
        /// </summary>
        /// <param name="headingData">Raw data from db. Table should contain data for the heading being edited.</param>
        /// <returns>Heading object</returns>
        private static AZHeading BuildHeadingToEdit(DataTable headingData)
        {
            if (headingData == null) throw new ArgumentNullException("headingData");

            // collection to remember what's already related (so we can stop them being selected again)
            List<int> alreadyRelated = new List<int>();
            List<int> servicesDone = new List<int>();
            List<int> ipsvDone = new List<int>();

            AZHeading heading = new AZHeading();

            // check we have heading data before binding it
            if (headingData.Rows.Count > 0)
            {
                // populate flatfile properties
                heading.Heading = headingData.Rows[0]["Heading"].ToString();
                if (headingData.Rows[0]["RedirectUrl"].ToString().Length > 0)
                {
                    heading.RedirectUrl = new Uri(headingData.Rows[0]["RedirectUrl"].ToString());
                }
                heading.RedirectTitle = headingData.Rows[0]["RedirectTitle"].ToString();

                // loop through related rows - remember ids of those done
                foreach (DataRow row in headingData.Rows)
                {
                    if (row["RelatedHeadingId"] != DBNull.Value && !alreadyRelated.Contains(Int32.Parse(row["RelatedHeadingId"].ToString(), CultureInfo.InvariantCulture)))
                    {
                        AZHeading relatedHeading = new AZHeading();
                        relatedHeading.Id = Int32.Parse(row["RelatedHeadingId"].ToString(), CultureInfo.InvariantCulture);
                        relatedHeading.Heading = row["RelatedHeading"].ToString();
                        heading.AddRelatedHeading(relatedHeading);

                        alreadyRelated.Add(Int32.Parse(row["RelatedHeadingId"].ToString(), CultureInfo.InvariantCulture));
                    }

                    if (row["ServiceId"] != DBNull.Value && !servicesDone.Contains(Int32.Parse(row["ServiceId"].ToString(), CultureInfo.InvariantCulture)))
                    {
                        AZService relatedService = new AZService();
                        relatedService.Id = Int32.Parse(row["ServiceId"].ToString(), CultureInfo.InvariantCulture);
                        relatedService.Service = row["Service"].ToString();
                        relatedService.Authority = ((Authority)Enum.Parse(typeof(Authority), row["Authority"].ToString()));
                        heading.AddService(relatedService);

                        servicesDone.Add(Int32.Parse(row["ServiceId"].ToString(), CultureInfo.InvariantCulture));
                    }

                    if (row["CommonName"] != DBNull.Value && !ipsvDone.Contains(Int32.Parse(row["CategoryId"].ToString(), CultureInfo.InvariantCulture)))
                    {
                        heading.IpsvPreferredTerms.Add(row["CommonName"].ToString());
                        ipsvDone.Add(Int32.Parse(row["CategoryId"].ToString(), CultureInfo.InvariantCulture));
                    }
                }
            }

            return heading;
        }

        /// <summary>
        /// Populates the service data table.
        /// </summary>
        private void PopulateRelatedServicesTable(ICollection relatedServices)
        {
            // Separate ESCC and other services
            Dictionary<int, AZService> esccServices = new Dictionary<int, AZService>();
            List<AZService> otherServices = new List<AZService>();

            foreach (AZService relatedService in relatedServices)
            {
                if (relatedService.Authority == Authority.EastSussex)
                {
                    esccServices.Add(relatedService.Id, relatedService);
                }
                else
                {
                    otherServices.Add(relatedService);
                }
            }

            // Get the sort order for ESCC services and use it to add them, last one first, 
            // onto the front of the "otherServices" collection
            string[] serviceSort = this.sortEsccServices.Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            Array.Reverse(serviceSort);
            foreach (string serviceId in serviceSort)
            {
                otherServices.Insert(0, esccServices[Int32.Parse(serviceId, CultureInfo.InvariantCulture)]);
            }

            this.services.Controls.Clear();
            int i = 0;
            foreach (AZService relatedService in otherServices)
            {
                using (HtmlTableRow tr = new HtmlTableRow())
                {

                    using (HtmlTableCell th = new HtmlTableCell("th"))
                    {
                        th.Attributes.Add("scope", "row");
                        using (HtmlAnchor serviceLink = new HtmlAnchor())
                        {
                            serviceLink.HRef = "service.aspx?service=" + relatedService.Id.ToString(CultureInfo.InvariantCulture);
                            serviceLink.InnerText = relatedService.Service + " (" + relatedService.Authority.ToString().Replace("EastSussex", "East Sussex") + ")";
                            th.Controls.Add(serviceLink);
                        }
                        tr.Controls.Add(th);
                    }

                    // Add sort option for ESCC services only
                    SortOption sort = SortOption.None;
                    if (i == 0 && esccServices.Count > 1) sort = SortOption.Down;
                    else if (i > 0 && i == (esccServices.Count - 1)) sort = SortOption.Up;
                    else if (i > 0 && relatedService.Authority == Authority.EastSussex) sort = SortOption.Both;
                    i++;

                    HtmlTableCell tdSort = new HtmlTableCell();
                    tr.Controls.Add(tdSort);

                    if (sort == SortOption.Up || sort == SortOption.Both)
                    {
                        using (EsccImageButton sortServiceUp = new EsccImageButton())
                        {
                            sortServiceUp.CssClass = "sortButton";
                            sortServiceUp.ImageUrl = "/wres/buttons/sortup.gif";
                            sortServiceUp.AlternateText = Properties.Resources.SortUp;
                            sortServiceUp.ID = String.Format(CultureInfo.InvariantCulture, "sortUp{0}", relatedService.Id);
                            sortServiceUp.CausesValidation = false;
                            sortServiceUp.Click += new ImageClickEventHandler(sortServiceUp_Click);
                            tdSort.Controls.Add(sortServiceUp);
                        }
                    }

                    if (sort == SortOption.Down || sort == SortOption.Both)
                    {
                        using (EsccImageButton sortServiceDown = new EsccImageButton())
                        {
                            sortServiceDown.CssClass = "sortButton";
                            sortServiceDown.ImageUrl = "/wres/buttons/sortdown.gif";
                            sortServiceDown.AlternateText = Properties.Resources.SortDown;
                            sortServiceDown.ID = String.Format(CultureInfo.InvariantCulture, "sortDown{0}", relatedService.Id);
                            sortServiceDown.CausesValidation = false;
                            sortServiceDown.Click += new ImageClickEventHandler(sortServiceDown_Click);
                            tdSort.Controls.Add(sortServiceDown);
                        }
                    }

                    using (HtmlTableCell td = new HtmlTableCell())
                    {
                        td.Attributes.Add("class", "action");
                        using (HtmlAnchor link = new HtmlAnchor())
                        {
                            link.InnerText = "Remove";
                            link.HRef = Request.Path + "?heading=" + this.headingId.Value + "&amp;removeservice=" + relatedService.Id;
                            td.Controls.Add(link);
                        }
                        tr.Controls.Add(td);
                    }

                    this.services.Controls.Add(tr);
                }
            }

            // hide exisiting services if there are none
            this.services.Visible = (this.services.Controls.Count > 0);
        }

        /// <summary>
        /// Bind data in dataset to the page
        /// </summary>
        /// <param name="headingData">Collection of all possible headings.</param>
        /// <param name="serviceData">Collection of all possible services.</param>
        /// <param name="heading">The A-Z heading being edited</param>
        /// <param name="repopulateFromPostData">if set to <c>true</c> repopulate form controls from post data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Web.UI.WebControls.ListItem.#ctor(System.String,System.String)")]
        private void PopulateForm(IList<AZHeading> headingData, List<AZService> serviceData, AZHeading heading, bool repopulateFromPostData)
        {
            PopulateRelatedHeadingsTable(heading, repopulateFromPostData);

            this.PopulateRelatedServicesTable(heading.Services);

            // collection to remember what's already related (so we can stop them being selected again)
            List<int> alreadyRelated = new List<int>();
            List<int> servicesDone = new List<int>();

            foreach (AZHeading relatedHeading in heading.RelatedHeadings) alreadyRelated.Add(relatedHeading.Id);
            foreach (AZService relatedService in heading.Services) servicesDone.Add(relatedService.Id);

            // add a blank option
            this.possibleRelatedHeadings.Items.Clear();
            this.possibleRelatedHeadings.Items.Add(new ListItem());

            // loop through all headings, and add all as options except current heading and those already related
            foreach (AZHeading relatedHeading in headingData)
            {
                if (relatedHeading.Id.ToString(CultureInfo.CurrentCulture) != this.headingId.Value && !alreadyRelated.Contains(relatedHeading.Id))
                {
                    ListItem item = new ListItem(relatedHeading.Heading, relatedHeading.Id.ToString(CultureInfo.CurrentCulture));
                    this.possibleRelatedHeadings.Items.Add(item);
                }
            }

            // add a blank option
            this.possibleServices.Items.Clear();
            this.possibleServices.Items.Add(new ListItem());

            // loop through all services, and add all as options except those already related
            foreach (AZService service in serviceData)
            {
                if (!servicesDone.Contains(service.Id))
                {
                    ListItem item = new ListItem(service.Service + " (" + service.Authority.ToString().Replace("EastSussex", "East Sussex") + ")", service.Id.ToString(CultureInfo.CurrentCulture));
                    this.possibleServices.Items.Add(item);
                }
            }

            // if validation error, repopulate details entered but not saved
            if (repopulateFromPostData)
            {
                if (Request.Form["ctl00$content$possibleRelatedHeadings"] != null)
                {
                    ListItem reselectHeading = this.possibleRelatedHeadings.Items.FindByValue(Request.Form["ctl00$content$possibleRelatedHeadings"].ToString());
                    if (reselectHeading != null) reselectHeading.Selected = true;
                }

                if (this.possibleServices.Enabled && Request.Form["ctl00$content$possibleServices"] != null)
                {
                    ListItem reselectService = this.possibleServices.Items.FindByValue(Request.Form["ctl00$content$possibleServices"].ToString());
                    if (reselectService != null) reselectService.Selected = true;
                }
            }
            else
            {
                if (this.possibleRelatedHeadings.SelectedItem != null) this.possibleRelatedHeadings.SelectedItem.Selected = false;
                if (this.possibleServices.SelectedItem != null) this.possibleServices.SelectedItem.Selected = false;
            }
        }

        private void PopulateRelatedHeadingsTable(AZHeading heading, bool repopulateFromPostData)
        {
            // check we have heading data before binding it
            if (heading.Heading.Length > 0)
            {
                // populate flatfile properties
                if (!repopulateFromPostData)
                {
                    this.headingTitle.Text = heading.Heading;
                    this.ipsvTerms.Text = heading.IpsvPreferredTerms.ToString();
                    if (heading.RedirectUrl != null) this.redirectUrl.Text = heading.RedirectUrl.ToString();
                    this.originalRedirectUrl.Value = this.redirectUrl.Text;
                    this.redirectTitle.Text = heading.RedirectTitle;
                }

                // Add related headings
                this.relatedHeadings.Controls.Clear();
                foreach (AZHeading relatedHeading in heading.RelatedHeadings)
                {
                    using (HtmlTableRow tr = new HtmlTableRow())
                    {

                        using (HtmlTableCell th = new HtmlTableCell("th"))
                        {
                            th.Attributes.Add("scope", "row");
                            using (HtmlAnchor headingLink = new HtmlAnchor())
                            {
                                headingLink.HRef = Request.Path + "?heading=" + relatedHeading.Id.ToString(CultureInfo.InvariantCulture);
                                headingLink.InnerText = relatedHeading.Heading;
                                th.Controls.Add(headingLink);
                            }
                            tr.Controls.Add(th);
                        }

                        using (HtmlTableCell td = new HtmlTableCell())
                        {
                            td.Attributes.Add("class", "action");
                            using (HtmlAnchor link = new HtmlAnchor())
                            {
                                link.InnerText = "Remove";
                                link.HRef = Request.Path + "?heading=" + this.headingId.Value + "&amp;removerelated=" + relatedHeading.Id.ToString(CultureInfo.InvariantCulture);
                                td.Controls.Add(link);
                            }
                            tr.Controls.Add(td);
                        }

                        this.relatedHeadings.Controls.Add(tr);
                    }
                }

                // Create field which controls sort order of ESCC related services
                if (!repopulateFromPostData)
                {
                    this.sortEsccServices.Value = String.Empty;
                    foreach (AZService relatedService in heading.Services)
                    {
                        if (relatedService.Authority == Authority.EastSussex)
                        {
                            if (this.sortEsccServices.Value.Length > 0) this.sortEsccServices.Value += ";";
                            this.sortEsccServices.Value += relatedService.Id.ToString(CultureInfo.CurrentCulture);
                        }
                    }
                }
            }

            // hide list of related headings if there are none
            this.relatedHeadings.Visible = (this.relatedHeadings.Controls.Count > 0);
        }

        /// <summary>
        /// Process submitted data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void submit_Click(object sender, EventArgs e)
        {
            if (this.IsValid)
            {
                // prepare auto-generated metadata
                EsdControlledList ipsv = EsdControlledList.GetControlledList("Ipsv");
                StringBuilder nonPrefs = new StringBuilder();

                foreach (EsdTerm term in this.ipsvValidator.MatchedTerms)
                {
                    EsdTermCollection nonPrefTerms = ipsv.GetNonPreferredTerms(term.Id);
                    foreach (EsdTerm nonPref in nonPrefTerms) term.NonPreferredTerms.Add(nonPref);
                    term.NonPreferredTerms.AppendText(nonPrefs);
                }

                // Is this heading an IPSV term?
                EsdTermCollection headingTerms = ipsv.GetTerms(this.headingTitle.Text.Trim(), true, EsdPreferredState.Preferred);
                bool headingIsIpsv = (headingTerms.Count == 1);

                // prepare parameters
                SqlParameter[] sqlParams = new SqlParameter[9];

                // heading id is optional because may be inserting new record
                if (this.headingId.Value.Length > 0)
                {
                    sqlParams[0] = new SqlParameter("@headingId", SqlDbType.Int, 4);
                    sqlParams[0].Value = this.headingId.Value;
                }

                sqlParams[1] = new SqlParameter("@heading", SqlDbType.VarChar, 255);
                sqlParams[1].Value = this.headingTitle.Text.Trim();

                sqlParams[2] = new SqlParameter("@redirectUrl", SqlDbType.VarChar, 255);
                sqlParams[2].Value = this.redirectUrl.Text.Trim();

                sqlParams[3] = new SqlParameter("@redirectTitle", SqlDbType.VarChar, 255);
                sqlParams[3].Value = this.redirectTitle.Text.Trim();

                if (Request.Form["ctl00$content$possibleRelatedHeadings"] != null && Request.Form["ctl00$content$possibleRelatedHeadings"].ToString().Length > 0)
                {
                    sqlParams[4] = new SqlParameter("@relatedHeadingId", SqlDbType.Int, 4);
                    sqlParams[4].Value = Convert.ToInt32(Request.Form["ctl00$content$possibleRelatedHeadings"].ToString(), CultureInfo.InvariantCulture);
                }

                if (Request.Form["ctl00$content$possibleServices"] != null && Request.Form["ctl00$content$possibleServices"].ToString().Length > 0)
                {
                    sqlParams[5] = new SqlParameter("@serviceId", SqlDbType.Int, 4);
                    sqlParams[5].Value = Convert.ToInt32(Request.Form["ctl00$content$possibleServices"].ToString(), CultureInfo.InvariantCulture);
                }

                // NOTE: IPSV preferred terms treated as relational data because they're used to control import/export.
                //		 Non-preferred terms treated as strings because that's the only way they're ever used,
                //		 and re-using the IPSV structure would generate overhead with extra rows in queries

                sqlParams[6] = new SqlParameter("@ipsvNonPreferredTerms", SqlDbType.VarChar, 1000);
                sqlParams[6].Value = nonPrefs.ToString();

                sqlParams[7] = new SqlParameter("@gclCategories", SqlDbType.VarChar, 255);
                sqlParams[7].Value = String.Empty; // Legacy parameter

                sqlParams[8] = new SqlParameter("@ipsv", SqlDbType.Bit);
                sqlParams[8].Value = headingIsIpsv;

                SqlParameter prmConceptId = new SqlParameter("@conceptId", SqlDbType.VarChar, 200);
                SqlParameter prmScheme = new SqlParameter("@scheme", SqlDbType.VarChar, 200);

                // Get ids of related ESCC services, which should be sorted into the desired order
                string[] esccServiceIds = this.sortEsccServices.Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                // prepare connection
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString))
                {
                    conn.Open();
                    SqlTransaction t = conn.BeginTransaction();

                    // update the db
                    try
                    {
                        // if heading id, update, else insert new record
                        if (this.headingId.Value.Length > 0)
                        {
                            SqlHelper.ExecuteNonQuery(t, CommandType.StoredProcedure, "usp_HeadingUpdate", sqlParams);

                            // clear existing ipsv terms
                            SqlHelper.ExecuteNonQuery(t, CommandType.StoredProcedure, "usp_DeleteCategoriesForHeading", sqlParams[0]);
                        }
                        else
                        {
                            // prepare output parameter for new id
                            sqlParams[0] = new SqlParameter("@headingId", SqlDbType.Int, 4);
                            sqlParams[0].Direction = ParameterDirection.Output;

                            // insert the record
                            Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteNonQuery(t, CommandType.StoredProcedure, "usp_HeadingInsert", sqlParams);

                            // get new id value and reset parameter direction
                            this.headingId.Value = sqlParams[0].Value.ToString();
                            sqlParams[0].Direction = ParameterDirection.Input;
                        }

                        // add ipsv terms, whether new or update
                        foreach (EsdTerm term in this.ipsvValidator.MatchedTerms)
                        {
                            prmConceptId.Value = term.ConceptId;
                            prmScheme.Value = term.ControlledList.AbbreviatedName;

                            SqlHelper.ExecuteNonQuery(t, CommandType.StoredProcedure, "usp_InsertCategoryForHeading", sqlParams[0], prmConceptId, prmScheme);
                        }

                        // update sort order of ESCC services
                        int sortPriority = esccServiceIds.Length;
                        int totalEsccServices = esccServiceIds.Length;
                        for (int i = 0; i < totalEsccServices; i++)
                        {
                            SqlParameter serviceParam = new SqlParameter("@serviceId", SqlDbType.Int);
                            serviceParam.Value = Int32.Parse(esccServiceIds[i], CultureInfo.InvariantCulture);

                            SqlParameter sortParam = new SqlParameter("@sortPriority", SqlDbType.Int);
                            sortParam.Value = sortPriority;

                            SqlHelper.ExecuteNonQuery(t, CommandType.StoredProcedure, "usp_ServiceSortWithinHeading", serviceParam, sqlParams[0], sortParam);

                            sortPriority--; // start with length of array and decrement sort value, meaning first service gets highest number 
                        }

                        t.Commit();
                    }
                    catch (SqlException)
                    {
                        t.Rollback();
                        throw;
                    }
                }
            }

            // get the initial data on the heading from the db
            DataTable[] data = this.GetData();
            IList<AZHeading> headingData = BuildHeadingCollection(data[0]);
            List<AZService> serviceData = BuildServiceCollection(data[1]);
            AZHeading heading = (this.headingId.Value.Length == 0) ? new AZHeading() : BuildHeadingToEdit(data[2]);

            // bind the data to the form
            this.SetPageHeading();
            this.PopulateForm(headingData, serviceData, heading, !this.IsValid);
        }

        /// <summary>
        /// Which sort button should be shown when building a table row for a service?
        /// </summary>
        private enum SortOption
        {
            None, Up, Down, Both
        }

        /// <summary>
        /// Handles the Click event of the sortServiceUp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.ImageClickEventArgs"/> instance containing the event data.</param>
        private void sortServiceUp_Click(object sender, ImageClickEventArgs e)
        {
            List<string> serviceSort = new List<string>(this.sortEsccServices.Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries));

            Control clicked = sender as Control;
            string serviceToMove = clicked.ID.Substring(6);
            int serviceToMovePos = serviceSort.IndexOf(serviceToMove);
            if (serviceToMovePos > 0)
            {
                serviceSort.RemoveAt(serviceToMovePos);
                serviceSort.Insert(serviceToMovePos - 1, serviceToMove);
            }

            this.sortEsccServices.Value = String.Join(";", serviceSort.ToArray());

            this.PopulateRelatedServicesTable(this.headingToEdit.Services);
        }

        /// <summary>
        /// Handles the Click event of the sortServiceDown control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.ImageClickEventArgs"/> instance containing the event data.</param>
        private void sortServiceDown_Click(object sender, ImageClickEventArgs e)
        {
            List<string> serviceSort = new List<string>(this.sortEsccServices.Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries));

            Control clicked = sender as Control;
            string serviceToMove = clicked.ID.Substring(8);
            int serviceToMovePos = serviceSort.IndexOf(serviceToMove);
            if (serviceToMovePos < (serviceSort.Count - 1))
            {
                serviceSort.RemoveAt(serviceToMovePos);
                serviceSort.Insert(serviceToMovePos + 1, serviceToMove);
            }

            this.sortEsccServices.Value = String.Join(";", serviceSort.ToArray());

            this.PopulateRelatedServicesTable(this.headingToEdit.Services);
        }
    }
}
