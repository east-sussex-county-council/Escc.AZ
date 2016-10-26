using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Escc.EastSussexGovUK.Features;
using Escc.EastSussexGovUK.Skins;
using Escc.EastSussexGovUK.Views;
using Escc.EastSussexGovUK.WebForms;
using Escc.FormControls.WebForms;
using Escc.FormControls.WebForms.Validators;
using Escc.Web.Metadata;
using Microsoft.ApplicationBlocks.Data;

namespace Escc.AZ.Admin
{
    /// <summary>
    /// Manage the data for a service in the A-Z
    /// </summary>
    public partial class service : Page
    {
        private AZContext azContext;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            var skinnable = Master as BaseMasterPage;
            if (skinnable != null)
            {
                skinnable.Skin = new CustomerFocusSkin(ViewSelector.CurrentViewIs(MasterPageFile));
            }

            this.azContext = AZContext.Current;

            if (!IsPostBack)
            {
                // if not postback, check a querystring specified with a service id
                try
                {
                    // get service id from querystring - conversion ensures it's a number
                    if (Request.QueryString["service"] != null) this.serviceId.Value = Convert.ToInt32(Request.QueryString["service"], CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    // just redirect to default page if querystring is bad
                    Response.Redirect("default.aspx");
                    Response.End();

                    throw;
                }


                // work to do only when editing (ie not new service)
                if (this.serviceId.Value.Length > 0)
                {

                    // check whether it's a delete request
                    if (Request.QueryString["removeheading"] != null || Request.QueryString["deletecontact"] != null)
                    {
                        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString))
                        {

                            // check whether it's a delete heading request
                            if (Request.QueryString["removeheading"] != null)
                            {
                                // set up two heading ids as parameters
                                SqlParameter[] sqlParams = new SqlParameter[2];
                                sqlParams[0] = new SqlParameter("@serviceId", SqlDbType.Int, 4);
                                sqlParams[0].Value = this.serviceId.Value;
                                sqlParams[1] = new SqlParameter("@headingId", SqlDbType.Int, 4);
                                sqlParams[1].Value = Convert.ToInt32(Request.QueryString["removeheading"].ToString(), CultureInfo.InvariantCulture);

                                // update db
                                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "usp_DeleteHeadingFromService", sqlParams);
                            }


                                // check whether it's a delete contact request
                            else if (Request.QueryString["deletecontact"] != null)
                            {
                                // set up two heading ids as parameters
                                SqlParameter[] sqlParams = new SqlParameter[1];
                                sqlParams[0] = new SqlParameter("@contactId", SqlDbType.Int, 4);
                                sqlParams[0].Value = Convert.ToInt32(Request.QueryString["deletecontact"].ToString(), CultureInfo.InvariantCulture);

                                // update db
                                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "usp_DeleteContact", sqlParams);
                            }
                        }
                    }

                }
                else
                {
                    // Work to do only for a new service
                    this.PopulateIpsvList(null);
                }
            }
            else
            {
                // check for deleted URLs, since click event stopped working
                foreach (string key in Request.Form.Keys)
                {
                    var pos = key.ToUpperInvariant().IndexOf("DELETEURL");
                    if (pos > -1)
                    {
                        var urlId = Int32.Parse(key.Substring(pos + 9), CultureInfo.InvariantCulture);
                        deleteUrl(urlId);
                    }
                }
            }

            // get the initial data on the heading from the db
            DataTable[] data = this.GetData();
            IList<AZHeading> allHeadings = GetHeadings(data);
            AZService service = GetService(data);

            // bind the data to the form
            // Even on postback because need to rebuild URL table to hook up buttons and event handlers.
            this.PopulateTables(service, allHeadings, IsPostBack);
            if (!IsPostBack) this.PopulateForm(service, false);

            // set page heading
            this.SetPageHeading();
        }

        /// <summary>
        /// Sets the page title and heading to say whether we're editing or creating a record
        /// </summary>
        private void SetPageHeading()
        {
            this.headContent.Title = (String.IsNullOrEmpty(this.serviceId.Value)) ? "New A–Z service" : "Edit A–Z service";
            this.h1.InnerHtml = this.headContent.Title;
        }

        #region Get data and build into A-Z objects
        /// <summary>
        /// Get raw data from database based on service id
        /// </summary>
        /// <returns>Raw data in DataTables</returns>
        private DataTable[] GetData()
        {
            // create container for data, and disconnected connection details
            DataTable[] tables = new DataTable[2];
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString);

            try
            {
                // get from the db all headings 
                tables[0] = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "usp_SelectAllHeadings").Tables[0];

                // get from the db details of the current service
                if (!String.IsNullOrEmpty(this.serviceId.Value))
                {
                    SqlParameter[] sqlParams = new SqlParameter[1];
                    sqlParams[0] = new SqlParameter("@serviceId", SqlDbType.Int, 4);
                    sqlParams[0].Value = Convert.ToInt32(this.serviceId.Value, CultureInfo.InvariantCulture);

                    tables[1] = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "usp_SelectServiceForEdit", sqlParams).Tables[0];
                }

                return tables;
            }
            finally
            {
                // always close connection
                conn.Close();
            }
        }


        /// <summary>
        /// Gets the service from the second table in the dataset
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static AZService GetService(DataTable[] data)
        {
            if (data[1] != null && data[1].Rows.Count > 0)
            {
                return AZObjectBuilder.BuildServiceFromRawData(data[1]);
            }
            else return null;
        }

        /// <summary>
        /// Get all headings from the first table in the dataset
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static IList<AZHeading> GetHeadings(DataTable[] data)
        {
            IList<AZHeading> allHeadings = new List<AZHeading>();

            // get headings from dataset
            if (data != null && data[0] != null)
            {
                foreach (DataRow row in data[0].Rows)
                {
                    AZHeading head = new AZHeading();
                    head.Id = Int32.Parse(row["HeadingId"].ToString(), CultureInfo.InvariantCulture);
                    head.Heading = row["Heading"].ToString();
                    allHeadings.Add(head);
                }

            }

            return allHeadings;
        }
        #endregion


        private void PopulateTables(AZService currentService, IList<AZHeading> allHeadings, bool hasFailedValidation)
        {
            PopulateRelatedHeadingsTable(currentService, allHeadings, hasFailedValidation);

            PopulateRelatedUrlsTable(currentService, hasFailedValidation);

            PopulateRelatedContactsTable(currentService);


        }

        private void PopulateRelatedHeadingsTable(AZService currentService, IList<AZHeading> allHeadings, bool hasFailedValidation)
        {
            var alreadyRelated = new List<int>();

            if (currentService != null)
            {
                if (currentService.Authority != Authority.EastSussex) this.azContext.Mode = AZMode.EditRestricted;

                // related headings
                foreach (AZHeading head in currentService.Headings) alreadyRelated.Add(head.Id);

                this.relatedHeadings.Controls.Clear();
                foreach (AZHeading head in currentService.Headings)
                {
                    HtmlTableRow tr = this.BuildHeadingRow(head);
                    if (tr != null) this.relatedHeadings.Controls.Add(tr);
                }

                // hide list of related headings if there are none
                this.relatedHeadings.Visible = (this.relatedHeadings.Controls.Count > 0);
            }

            // loop through all headings, and add all as options except those already related
            this.possibleRelatedHeadings.Items.Add(new ListItem()); // add a blank option
            foreach (AZHeading head in allHeadings)
            {
                if (!alreadyRelated.Contains(head.Id))
                {
                    ListItem item = new ListItem(head.Heading, head.Id.ToString(CultureInfo.CurrentCulture));
                    this.possibleRelatedHeadings.Items.Add(item);
                }
            }

            // if validation error, repopulate details entered but not saved
            if (hasFailedValidation)
            {
                ListItem reselectHeading = this.possibleRelatedHeadings.Items.FindByValue(Request.Form["ctl00$content$possibleRelatedHeadings"].ToString());
                if (reselectHeading != null) reselectHeading.Selected = true;
            }
        }

        private void PopulateRelatedContactsTable(AZService currentService)
        {
            if (currentService != null)
            {
                // write out contacts
                using (var contactGrid = new AZContactGrid(currentService.Contacts, this.azContext))
                {
                    this.contacts.Controls.Clear();
                    contactGrid.EmailAddressTransformer = new WebsiteFormEmailAddressTransformer(Request.Url);
                    this.contacts.Controls.Add(contactGrid);
                }

                // admin for contacts if East Sussex entry
                if (this.azContext.Mode == AZMode.Edit)
                {
                    this.editContacts.Visible = true;
                }

                // new contact link
                this.newContactLink.HRef = "contact.aspx?service=" + this.serviceId.Value;
                this.newContactLink.Visible = true;
            }
            else
            {
                this.newContactLink.Visible = false;
            }

            if (this.azContext.Mode == AZMode.Edit)
            {
                this.noContactHelp.Visible = !this.newContactLink.Visible; // explain why "new contact" link is invisible
            }
            else this.noContactHelp.Visible = false;
        }

        private void PopulateRelatedUrlsTable(AZService currentService, bool hasFailedValidation)
        {
            if (currentService != null)
            {
                // related URLs
                bool canDelete = (this.azContext.Mode == AZMode.Edit);
                this.currentUrls.Controls.Clear();
                for (int i = 0; i < currentService.Urls.Count; i++)
                {
                    SortOption opt = SortOption.None;
                    if (i == 0 && currentService.Urls.Count > 1) opt = SortOption.Down;
                    else if (i > 0 && i == (currentService.Urls.Count - 1)) opt = SortOption.Up;
                    else if (i > 0) opt = SortOption.Both;

                    this.BuildUrlRow(i + 1, currentService.Urls[i], canDelete, opt, this.currentUrls);
                }

                this.currentUrls.Visible = (this.currentUrls.Controls.Count > 0);

                // Store the URL ids in a hidden field to be used on postback
                StringBuilder sb = new StringBuilder();
                foreach (AZUrl azUrl in currentService.Urls)
                {
                    if (sb.Length > 0) sb.Append(";");
                    sb.Append(azUrl.Id.ToString(CultureInfo.CurrentCulture));
                }
                this.urlIds.Value = sb.ToString();
            }

            // if validation error, repopulate details entered but not saved
            if (hasFailedValidation)
            {
                if (Request.Form["ctl00$content$url"] != null) this.url.Text = Request.Form["ctl00$content$url"].ToString().Trim();
                if (Request.Form["ctl00$content$urlTitle"] != null) this.urlTitle.Text = Request.Form["ctl00$content$urlTitle"].ToString().Trim();
                if (Request.Form["ctl00$content$urlDescription"] != null) this.urlDescription.Text = Request.Form["ctl00$content$urlDescription"].ToString().Trim();
            }
        }

        /// <summary>
        /// Bind data in dataset to the page
        /// </summary>
        /// <param name="currentService">The service data retrieved from the db</param>
        /// <param name="hasFailedValidation">Pass in !this.IsValid when appropriate</param>
        private void PopulateForm(AZService currentService, bool hasFailedValidation)
        {
            // check we have service data before binding it
            if (currentService != null)
            {

                // populate flatfile properties
                if (!hasFailedValidation)
                {
                    this.serviceTitle.Text = currentService.Service;
                    this.description.Text = currentService.Description;
                    this.keywords.Text = currentService.Keywords;
                    this.PopulateIpsvList(currentService.IpsvPreferredTerms);
                }

                // clear entry-only fields
                this.url.Text = "";
                this.urlTitle.Text = "";
                this.urlDescription.Text = "";

                // disable fields for an other-authority heading
                if (this.azContext.Mode == AZMode.EditRestricted)
                {
                    this.serviceTitle.ReadOnly = true;
                    this.description.ReadOnly = true;
                    this.keywords.ReadOnly = true;

                    this.editContacts.Visible = false;
                    this.editUrls.Visible = false;
                    this.editHeadings.Visible = false;
                    this.submit.Visible = false;
                }
            }
        }

        /// <summary>
        /// Add info on a related heading to a table row
        /// </summary>
        /// <param name="head">The heading to add</param>
        /// <returns>A table row</returns>
        private HtmlTableRow BuildHeadingRow(AZHeading head)
        {
            using (HtmlTableRow tr = new HtmlTableRow())
            {

                using (HtmlTableCell th = new HtmlTableCell("th"))
                {
                    th.Attributes.Add("scope", "row");
                    using (HtmlAnchor headingLink = new HtmlAnchor())
                    {
                        headingLink.HRef = "heading.aspx?heading=" + head.Id.ToString(CultureInfo.CurrentCulture);
                        headingLink.InnerText = head.Heading;
                        th.Controls.Add(headingLink);
                    }
                    tr.Controls.Add(th);
                }

                if (this.azContext.Mode == AZMode.Edit)
                {
                    using (HtmlTableCell td = new HtmlTableCell())
                    {
                        td.Attributes.Add("class", "action");
                        using (HtmlAnchor link = new HtmlAnchor())
                        {
                            link.InnerText = "Remove";
                            link.HRef = Request.Path + "?service=" + this.serviceId.Value + "&amp;removeheading=" + head.Id.ToString(CultureInfo.CurrentCulture);
                            td.Controls.Add(link);
                        }
                        tr.Controls.Add(td);
                    }
                }

                return tr;
            }
        }

        /// <summary>
        /// Add info on a related URL to a table row
        /// </summary>
        /// <param name="index">Number of link (1st, 2nd, 3rd etc)</param>
        /// <param name="urlToEdit">The related URL</param>
        /// <param name="canEdit">Boolean indicating whether URL is editable</param>
        /// <param name="sort">Which sort button to show?</param>
        /// <param name="container">Control to add the new row to</param>
        private void BuildUrlRow(int index, AZUrl urlToEdit, bool canEdit, SortOption sort, Control container)
        {
            using (HtmlGenericControl fs = new HtmlGenericControl("fieldset"))
            {
                fs.Attributes["class"] = "azUrlEdit";
                using (HtmlGenericControl leg = new HtmlGenericControl("legend"))
                {
                    leg.InnerText = "Link " + index.ToString(CultureInfo.CurrentCulture);
                    fs.Controls.Add(leg);
                }
                container.Controls.Add(fs);

                string urlId = urlToEdit.Id.ToString(CultureInfo.CurrentCulture);

                using (HtmlGenericControl divData = new HtmlGenericControl("div"))
                {
                    divData.Attributes["class"] = "azUrlData";
                    fs.Controls.Add(divData);

                    // Editable URL
                    using (TextBox urlBox = new TextBox())
                    {
                        urlBox.ReadOnly = !canEdit;
                        urlBox.Text = urlToEdit.Url.ToString();
                        urlBox.MaxLength = 255;
                        urlBox.ID = String.Format(CultureInfo.InvariantCulture, "url{0}url", urlId);

                        FormPart urlPart = new FormPart("Link", urlBox);
                        divData.Controls.Add(urlPart);


                        // Editable title
                        using (TextBox titleBox = new TextBox())
                        {
                            titleBox.ReadOnly = !canEdit;
                            titleBox.Text = urlToEdit.Text.ToString();
                            titleBox.MaxLength = 75;
                            titleBox.ID = String.Format(CultureInfo.InvariantCulture, "url{0}title", urlId);

                            FormPart titlePart = new FormPart("Link text", titleBox);
                            divData.Controls.Add(titlePart);

                            // Editable description if used
                            using (TextBox descBox = new TextBox())
                            {
                                descBox.ReadOnly = !canEdit;
                                descBox.Text = urlToEdit.Description;
                                descBox.TextMode = TextBoxMode.MultiLine;
                                descBox.ID = String.Format(CultureInfo.InvariantCulture, "url{0}desc", urlId);

                                FormPart descPart = new FormPart("Description", descBox);
                                divData.Controls.Add(descPart);

                                if (canEdit)
                                {
                                    // Add validators
                                    string linkNumber = index.ToString(CultureInfo.CurrentCulture);
                                    fs.Controls.Add(new EsccRequiredFieldValidator(urlBox.ID, "You can't save link " + linkNumber + " with no URL. If you want to delete a link, click on 'Delete'."));
                                    fs.Controls.Add(new EsccRegularExpressionValidator(urlBox.ID, "Link " + linkNumber + " must be a complete web address beginning with http://&#8230;", @"^(http|https|ftp)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&amp;%\$\-]+)*@)*((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|localhost|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{2}))(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\?\'\\\+&amp;%\$#\=~_\-]+))*$"));
                                    fs.Controls.Add(new EsccRegularExpressionValidator(urlBox.ID, "The URL for link " + linkNumber + " is too long: it must be 255 characters or fewer", ".{1,255}"));
                                    fs.Controls.Add(new EsccRegularExpressionValidator(titleBox.ID, "Your text for link " + linkNumber + " is too long: it must be 75 characters or fewer", ".{1,75}"));
                                    fs.Controls.Add(new EsccRegularExpressionValidator(descBox.ID, "Your description for link " + linkNumber + " is too long: it must be 300 characters or fewer", ".{1,300}"));

                                    // Add sort option
                                    using (HtmlGenericControl divSort = new HtmlGenericControl("div"))
                                    {
                                        divSort.Attributes["class"] = "azUrlSort";
                                        if (sort != SortOption.None)
                                        {
                                            fs.Controls.Add(divSort);
                                        }

                                        if (sort == SortOption.Up || sort == SortOption.Both)
                                        {
                                            using (EsccImageButton sortUrlUp = new EsccImageButton())
                                            {
                                                sortUrlUp.CssClass = "sortButton";
                                                sortUrlUp.ImageUrl = "/wres/buttons/sortup.gif";
                                                sortUrlUp.AlternateText = Properties.Resources.SortUp;
                                                sortUrlUp.ID = String.Format(CultureInfo.InvariantCulture, "sortUp{0}", urlId);
                                                sortUrlUp.CausesValidation = false;
                                                sortUrlUp.Click += new ImageClickEventHandler(sortUrlUp_Click);
                                                divSort.Controls.Add(sortUrlUp);
                                            }
                                        }

                                        if (sort == SortOption.Down || sort == SortOption.Both)
                                        {
                                            using (EsccImageButton sortUrlDown = new EsccImageButton())
                                            {
                                                sortUrlDown.CssClass = "sortButton";
                                                sortUrlDown.ImageUrl = "/wres/buttons/sortdown.gif";
                                                sortUrlDown.AlternateText = Properties.Resources.SortDown;
                                                sortUrlDown.ID = String.Format(CultureInfo.InvariantCulture, "sortDown{0}", urlId);
                                                sortUrlDown.CausesValidation = false;
                                                sortUrlDown.Click += new ImageClickEventHandler(sortUrlDown_Click);
                                                divSort.Controls.Add(sortUrlDown);
                                            }
                                        }
                                    }

                                    // Delete link
                                    using (HtmlGenericControl divDelete = new HtmlGenericControl("div"))
                                    {
                                        divDelete.Attributes["class"] = "azUrlDelete";
                                        fs.Controls.Add(divDelete);

                                        using (EsccButton deleteUrl = new EsccButton())
                                        {
                                            deleteUrl.CssClass = "button";
                                            deleteUrl.Text = Properties.Resources.DeleteButton;
                                            deleteUrl.ID = String.Format(CultureInfo.InvariantCulture, "deleteUrl{0}", urlId);
                                            deleteUrl.CausesValidation = false;
                                            divDelete.Controls.Add(deleteUrl);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
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
                // Is this service an IPSV term?
                EsdControlledList ipsv = EsdControlledList.GetControlledList("Ipsv");
                EsdTermCollection serviceTerms = ipsv.GetTerms(this.serviceTitle.Text.Trim(), true, EsdPreferredState.Preferred);
                bool serviceIsIpsv = (serviceTerms.Count == 1);

                // prepare parameters
                SqlParameter[] sqlParams = null;

                sqlParams = new SqlParameter[11];

                // service id optional because may be inserting new record
                if (sqlParams != null && this.serviceId.Value.Length > 0)
                {
                    sqlParams[0] = new SqlParameter("@serviceId", SqlDbType.Int, 4);
                    sqlParams[0].Value = this.serviceId.Value;
                }

                sqlParams[1] = new SqlParameter("@service", SqlDbType.VarChar, 255);
                sqlParams[1].Value = this.serviceTitle.Text.Trim();

                sqlParams[2] = new SqlParameter("@description", SqlDbType.VarChar, 2550);
                sqlParams[2].Value = this.description.Text.Trim();

                sqlParams[3] = new SqlParameter("@keywords", SqlDbType.VarChar, 250);
                sqlParams[3].Value = this.keywords.Text.Trim();

                sqlParams[4] = new SqlParameter("@authorityId", SqlDbType.TinyInt, 1);
                sqlParams[4].Value = (int)Authority.EastSussex;

                sqlParams[5] = new SqlParameter("@sortPriority", SqlDbType.TinyInt, 1);
                sqlParams[5].Value = 1;

                sqlParams[6] = new SqlParameter("@headingId", SqlDbType.Int, 4);
                if (Request.Form["ctl00$content$possibleRelatedHeadings"] != null && Request.Form["ctl00$content$possibleRelatedHeadings"].Length > 0)
                {
                    sqlParams[6].Value = Convert.ToInt32(Request.Form["ctl00$content$possibleRelatedHeadings"].ToString(), CultureInfo.InvariantCulture);
                }

                string urlText = this.url.Text.Trim();
                if (urlText.Length > 0)
                {
                    sqlParams[7] = new SqlParameter("@url", SqlDbType.VarChar, 255);
                    sqlParams[7].Value = urlText;

                    sqlParams[8] = new SqlParameter("@urlTitle", SqlDbType.VarChar, 75);
                    sqlParams[8].Value = this.urlTitle.Text.Trim();

                    sqlParams[9] = new SqlParameter("@urlDescription", SqlDbType.VarChar, 300);
                    sqlParams[9].Value = this.urlDescription.Text.Trim();
                }

                sqlParams[10] = new SqlParameter("@ipsv", SqlDbType.Bit);
                sqlParams[10].Value = serviceIsIpsv;

                string[] urlsToUpdate = this.urlIds.Value.Split(';');
                SqlParameter[] urlParams = new SqlParameter[4];

                urlParams[0] = new SqlParameter("@urlId", SqlDbType.Int, 4);
                urlParams[1] = new SqlParameter("@url", SqlDbType.VarChar, 255);
                urlParams[2] = new SqlParameter("@urlTitle", SqlDbType.VarChar, 75);
                urlParams[3] = new SqlParameter("@urlDescription", SqlDbType.VarChar, 300);

                // prepare connection
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString);

                // update the db
                try
                {
                    // if service id, update, else insert new record
                    if (this.serviceId.Value.Length > 0)
                    {
                        SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "usp_ServiceUpdate", sqlParams);

                        // Update each link in the URL table
                        foreach (string urlId in urlsToUpdate)
                        {
                            if (urlId.Length > 0)
                            {
                                urlParams[0].Value = urlId;
                                urlParams[1].Value = Request.Form[String.Format(CultureInfo.InvariantCulture, "ctl00$content$url{0}url", urlId)];
                                urlParams[2].Value = Request.Form[String.Format(CultureInfo.InvariantCulture, "ctl00$content$url{0}title", urlId)];
                                urlParams[3].Value = Request.Form[String.Format(CultureInfo.InvariantCulture, "ctl00$content$url{0}desc", urlId)];

                                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "usp_UrlUpdate", urlParams);
                            }
                        }
                    }
                    else
                    {
                        // prepare output parameter for new id
                        sqlParams[0] = new SqlParameter("@serviceId", SqlDbType.Int, 4);
                        sqlParams[0].Direction = ParameterDirection.Output;

                        // insert the record
                        SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "usp_ServiceInsert", sqlParams);

                        // get new id value
                        this.serviceId.Value = sqlParams[0].Value.ToString();
                    }
                }
                finally
                {
                    // always close connection
                    conn.Close();
                }

                // get the initial data on the heading from the db
                DataTable[] data = this.GetData();
                IList<AZHeading> allHeadings = GetHeadings(data);
                AZService service = GetService(data);

                // bind the data to the form
                this.SetPageHeading();
                this.PopulateTables(service, allHeadings, !this.IsValid);
                this.PopulateForm(service, !this.IsValid);
            }
            else
            {
                this.PopulateIpsvList(null);
            }
        }

        /// <summary>
        /// Validate description box to be no more than 60 words
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CheckDescriptionWords(object sender, ServerValidateEventArgs e)
        {
            if (e == null) throw new ArgumentNullException("e");

            // split text into words
            string text = this.description.Text.Trim();
            text = Regex.Replace(text, "\n", " ");
            while (text.IndexOf("  ", StringComparison.Ordinal) > -1) text = text.Replace("  ", " ");
            string[] words = text.Split(' ');

            e.IsValid = (words.Length) <= 60;

        }

        /// <summary>
        /// Show IPSV terms used to assign the service to headings during import
        /// </summary>
        /// <param name="terms">IPSV terms used ti import the service</param>
        private void PopulateIpsvList(EsdTermCollection terms)
        {
            if (terms != null && terms.Count > 0)
            {
                foreach (EsdTerm term in terms)
                {
                    using (HtmlGenericControl li = new HtmlGenericControl("li"))
                    {
                        li.InnerText = term.Text;
                        this.ipsvTerms.Controls.Add(li);
                    }
                }
                ipsvInfo.Visible = true;
            }
            else
            {
                ipsvInfo.Visible = false;
            }
        }

        #region Manage URLs

        /// <summary>
        /// Which sort button should be shown when building a table row for a URL?
        /// </summary>
        private enum SortOption
        {
            None, Up, Down, Both
        }

        private void sortUrlUp_Click(object sender, ImageClickEventArgs e)
        {
            // set up url id and sort direction as parameters
            SqlParameter[] sqlParams = new SqlParameter[2];
            sqlParams[0] = new SqlParameter("@urlId", SqlDbType.Int, 4);
            sqlParams[0].Value = Convert.ToInt32(((ImageButton)sender).ID.Substring(6), CultureInfo.InvariantCulture);
            sqlParams[1] = new SqlParameter("@sortUp", SqlDbType.Bit);
            sqlParams[1].Value = 1;

            // update db
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString))
            {
                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "usp_UrlSort", sqlParams);
            }

            DataTable[] data = this.GetData();
            IList<AZHeading> allHeadings = GetHeadings(data);
            AZService service = GetService(data);
            this.PopulateTables(service, allHeadings, true);
        }

        private void sortUrlDown_Click(object sender, ImageClickEventArgs e)
        {
            // set up url id and sort direction as parameters
            SqlParameter[] sqlParams = new SqlParameter[2];
            sqlParams[0] = new SqlParameter("@urlId", SqlDbType.Int, 4);
            sqlParams[0].Value = Convert.ToInt32(((ImageButton)sender).ID.Substring(8), CultureInfo.InvariantCulture);
            sqlParams[1] = new SqlParameter("@sortUp", SqlDbType.Bit);
            sqlParams[1].Value = 0;

            // update db
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString))
            {
                Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "usp_UrlSort", sqlParams);
            }

            DataTable[] data = this.GetData();
            IList<AZHeading> allHeadings = GetHeadings(data);
            AZService service = GetService(data);
            this.PopulateTables(service, allHeadings, true);
        }

        private void deleteUrl(int urlId)
        {
            // set up url id as parameter
            SqlParameter[] sqlParams = new SqlParameter[1];
            sqlParams[0] = new SqlParameter("@urlId", SqlDbType.Int, 4);
            sqlParams[0].Value = urlId;

            // update db
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString))
            {
                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "usp_DeleteUrl", sqlParams);
            }

            DataTable[] data = this.GetData();
            IList<AZHeading> allHeadings = GetHeadings(data);
            AZService service = GetService(data);
            this.PopulateTables(service, allHeadings, true);
        }

        #endregion Manage URLs

    }
}
