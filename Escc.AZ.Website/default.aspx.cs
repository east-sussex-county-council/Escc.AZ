using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Escc.Data.Ado;
using Escc.EastSussexGovUK.Skins;
using Escc.EastSussexGovUK.Views;
using Escc.EastSussexGovUK.WebForms;
using Escc.Web;
using Humanizer;

namespace Escc.AZ.Website
{
    /// <summary>
    /// The main A-Z page, which lists headings.
    /// Cannot be a CMS template because without Javascript the form posts to a non-existent page (the template)
    /// </summary>
    public partial class Headings : Page
    {
        private AZContext context;
        private bool usedSearch = false;
        private DataTable headingData = null;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            var skinnable = Master as BaseMasterPage;
            if (skinnable != null)
            {
                skinnable.Skin = new CustomerFocusSkin(ViewSelector.CurrentViewIs(MasterPageFile));
            }

            new HttpCacheHeaders().CacheUntil(Response.Cache, DateTime.Now.AddDays(1));

            this.context = AZContext.Current;
            this.azScript.Visible = this.context.PartnersEnabled;

            // check for search request
            if (Request.QueryString["azq"] != null && Request.QueryString["azq"].ToString().Trim().Length > 0)
            {
                // search
                headingData = this.GetSearchData();
                usedSearch = true;

            }
            // otherwise display the selected letter
            else
            {
                // get heading data from db
                headingData = this.GetHeadingData();

            }

            DisplayDataOnPage(headingData, usedSearch);
        }

        private void DisplayDataOnPage(DataTable headingData, bool usedSearch)
        {
            if (headingData != null && headingData.Rows.Count > 0)
            {
                // convert into AZHeading objects
                AZHeading[] headings = this.GetHeadings(headingData);

                // bind to page
                foreach (AZHeading heading in headings)
                {
                    if (heading != null)
                    {
                        HtmlGenericControl control = this.BuildHeading(heading);
                        if (control != null) this.headingList.Controls.Add(control);
                    }
                }

                this.headingList.Visible = true;
                this.notFound.Visible = false;
                this.notFoundSearch.Visible = false;
            }
            else
            {
                // if no headings, add a message to say so
                this.headingList.Visible = false;
                this.notFound.Visible = !usedSearch;
                this.notFoundSearch.Visible = usedSearch;
                if (this.notFoundSearch.Visible)
                {
                    this.searchTerm.Text = Server.HtmlEncode(Request.QueryString["azq"].Trim());
                    this.searchLink.HRef = String.Format(CultureInfo.CurrentCulture, this.searchLink.HRef, this.searchTerm.Text);
                }
            }

            // say whether results are filtered by authority
            StringBuilder criteriaText = new StringBuilder();
            criteriaText.Append("all services");
            if (usedSearch)
            {
                criteriaText.Append(" matching \"").Append(Request.QueryString["azq"].ToString().Trim()).Append("\"");
            }
            else
            {
                criteriaText.Append(" beginning with ").Append(this.context.SelectedChar.ToUpper(CultureInfo.CurrentCulture));
            }

            int councilCount = this.context.SelectedCouncils;
            if (councilCount < 6)
            {
                criteriaText.Append(" (from ").Append(councilCount.ToString(CultureInfo.CurrentCulture)).Append(" council");
                if (councilCount > 1) criteriaText.Append("s");
                criteriaText.Append(")");
            }

            this.criteria.InnerText += criteriaText.ToString();
            this.headContent.Title += criteriaText.ToString().Transform(To.SentenceCase);
        }


        /// <summary>
        /// Get a list of headings from the database
        /// </summary>
        /// <returns></returns>
        private DataTable GetHeadingData()
        {
            // set up connection details
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConnectionStringAZ"].ConnectionString);

            try
            {
                // build parameters
                SqlParameter[] sqlParams = new SqlParameter[7];
                sqlParams[0] = new SqlParameter("@indexChar", SqlDbType.Char, 1);
                sqlParams[0].Value = this.context.SelectedChar;

                sqlParams[1] = new SqlParameter("@eastSussex", SqlDbType.Bit, 1);
                sqlParams[1].Value = this.context.EsccSelected;

                sqlParams[2] = new SqlParameter("@eastbourne", SqlDbType.Bit, 1);
                sqlParams[2].Value = this.context.EastbourneSelected;

                sqlParams[3] = new SqlParameter("@hastings", SqlDbType.Bit, 1);
                sqlParams[3].Value = this.context.HastingsSelected;

                sqlParams[4] = new SqlParameter("@lewes", SqlDbType.Bit, 1);
                sqlParams[4].Value = this.context.LewesSelected;

                sqlParams[5] = new SqlParameter("@rother", SqlDbType.Bit, 1);
                sqlParams[5].Value = this.context.RotherSelected;

                sqlParams[6] = new SqlParameter("@wealden", SqlDbType.Bit, 1);
                sqlParams[6].Value = this.context.WealdenSelected;

                // connect and get data
                return EsccSqlHelper.ExecuteDatatable(conn, CommandType.StoredProcedure, "usp_SelectHeadingsByIndex", sqlParams);
            }
            finally
            {
                // always close connection
                conn.Close();
            }
        }

        /// <summary>
        /// Get a list of headings from the database matching the search term
        /// </summary>
        /// <returns></returns>
        private DataTable GetSearchData()
        {
            // set up connection details
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConnectionStringAZ"].ConnectionString);

            try
            {
                // build parameters
                SqlParameter[] sqlParams = new SqlParameter[7];
                sqlParams[0] = new SqlParameter("@search", SqlDbType.VarChar, 50);
                sqlParams[0].Value = (Request.QueryString["azq"] != null) ? Request.QueryString["azq"].Trim().ToString() : "";

                sqlParams[1] = new SqlParameter("@eastSussex", SqlDbType.Bit, 1);
                sqlParams[1].Value = this.context.EsccSelected;

                sqlParams[2] = new SqlParameter("@eastbourne", SqlDbType.Bit, 1);
                sqlParams[2].Value = this.context.EastbourneSelected;

                sqlParams[3] = new SqlParameter("@hastings", SqlDbType.Bit, 1);
                sqlParams[3].Value = this.context.HastingsSelected;

                sqlParams[4] = new SqlParameter("@lewes", SqlDbType.Bit, 1);
                sqlParams[4].Value = this.context.LewesSelected;

                sqlParams[5] = new SqlParameter("@rother", SqlDbType.Bit, 1);
                sqlParams[5].Value = this.context.RotherSelected;

                sqlParams[6] = new SqlParameter("@wealden", SqlDbType.Bit, 1);
                sqlParams[6].Value = this.context.WealdenSelected;

                // connect and get data
                return EsccSqlHelper.ExecuteDatatable(conn, CommandType.StoredProcedure, "usp_SelectHeadingsBySearch", sqlParams);
            }
            finally
            {
                // always close connection
                conn.Close();
            }
        }

        /// <summary>
        /// Organise data into array of AZHeading business objects
        /// </summary>
        /// <param name="headingData"></param>
        /// <returns></returns>
        private AZHeading[] GetHeadings(DataTable headingData)
        {
            // create array to hold headings - may be too big, but upper entries can remain null
            AZHeading[] headings = new AZHeading[headingData.Rows.Count];

            // monitor loop
            int headingCount = -1;

            // monitor multiple-rows-per-heading
            int currentId = -1;

            foreach (DataRow row in headingData.Rows)
            {
                // monitor heading id to see if we're dealing with same heading in multiple rows
                int headingId = Convert.ToInt32(row["HeadingId"], CultureInfo.InvariantCulture);

                if (currentId != headingId)
                {
                    // increment loop vars
                    headingCount++;
                    currentId = headingId;

                    // create heading on first row for this heading
                    headings[headingCount] = new AZHeading();
                    headings[headingCount].Id = headingId;
                    headings[headingCount].Heading = row["Heading"].ToString();
                    headings[headingCount].ServiceCountEscc = Convert.ToInt32(row["ServiceCountEscc"], CultureInfo.CurrentCulture);
                    headings[headingCount].ServiceCountEastbourne = Convert.ToInt32(row["ServiceCountEastbourne"], CultureInfo.CurrentCulture);
                    headings[headingCount].ServiceCountHastings = Convert.ToInt32(row["ServiceCountHastings"], CultureInfo.CurrentCulture);
                    headings[headingCount].ServiceCountLewes = Convert.ToInt32(row["ServiceCountLewes"], CultureInfo.CurrentCulture);
                    headings[headingCount].ServiceCountRother = Convert.ToInt32(row["ServiceCountRother"], CultureInfo.CurrentCulture);
                    headings[headingCount].ServiceCountWealden = Convert.ToInt32(row["ServiceCountWealden"], CultureInfo.CurrentCulture);
                    if (row["RedirectUrl"].ToString().Length > 0) headings[headingCount].RedirectUrl = new System.Uri(row["RedirectUrl"].ToString());
                    headings[headingCount].RedirectTitle = row["RedirectTitle"].ToString();

                    // add related heading if present in row
                    if (row["RelatedHeadingId"] != DBNull.Value)
                    {
                        // add related heading if present in row
                        this.AddRelatedHeadingIfRelevant(headings[headingCount], row);
                    }

                }
                // can have multiple-rows-per-heading due to related headings
                else if (row["RelatedHeadingId"] != DBNull.Value)
                {
                    // add related heading if present in row
                    this.AddRelatedHeadingIfRelevant(headings[headingCount], row);
                }

            }

            return headings;
        }


        /// <summary>
        /// Stored proc does some filtering of headings but complexity means it can't eliminate all of them, so
        /// this determines whether a related heading returned by the proc is relevant before adding it
        /// </summary>
        /// <param name="mainHeading"></param>
        /// <param name="row"></param>
        private void AddRelatedHeadingIfRelevant(AZHeading mainHeading, DataRow row)
        {
            // get data about the related heading
            AZHeading relatedHeading = new AZHeading();
            relatedHeading.Id = Convert.ToInt32(row["RelatedHeadingId"], CultureInfo.InvariantCulture);
            relatedHeading.Heading = row["RelatedHeading"].ToString();
            relatedHeading.ServiceCountEscc = Convert.ToInt32(row["RelatedServiceCountEscc"], CultureInfo.CurrentCulture);
            relatedHeading.ServiceCountEastbourne = Convert.ToInt32(row["RelatedServiceCountEastbourne"], CultureInfo.CurrentCulture);
            relatedHeading.ServiceCountHastings = Convert.ToInt32(row["RelatedServiceCountHastings"], CultureInfo.CurrentCulture);
            relatedHeading.ServiceCountLewes = Convert.ToInt32(row["RelatedServiceCountLewes"], CultureInfo.CurrentCulture);
            relatedHeading.ServiceCountRother = Convert.ToInt32(row["RelatedServiceCountRother"], CultureInfo.CurrentCulture);
            relatedHeading.ServiceCountWealden = Convert.ToInt32(row["RelatedServiceCountWealden"], CultureInfo.CurrentCulture);
            if (row["RelatedRedirectUrl"].ToString().Length > 0) relatedHeading.RedirectUrl = new System.Uri(row["RelatedRedirectUrl"].ToString());
            relatedHeading.RedirectTitle = row["RelatedRedirectTitle"].ToString();

            // test whether the heading has any content for the selected councils, or is a redirect
            if ((this.context.EsccSelected && relatedHeading.ServiceCountEscc > 0) ||
                (this.context.EastbourneSelected && relatedHeading.ServiceCountEastbourne > 0) ||
                (this.context.HastingsSelected && relatedHeading.ServiceCountHastings > 0) ||
                (this.context.LewesSelected && relatedHeading.ServiceCountLewes > 0) ||
                (this.context.RotherSelected && relatedHeading.ServiceCountRother > 0) ||
                (this.context.WealdenSelected && relatedHeading.ServiceCountWealden > 0) ||
                (relatedHeading.RedirectUrl != null)
                )
            {
                mainHeading.AddRelatedHeading(relatedHeading);
            }
        }

        /// <summary>
        /// Format AZHeading object as XHTML and add to unordered list
        /// </summary>
        /// <param name="row"></param>
        private HtmlGenericControl BuildHeading(AZHeading heading)
        {
            // array of headings will deliberately have null entries due to multiple-rows-per-heading, so check
            if (heading != null)
            {
                //list item to contain heading
                using (HtmlGenericControl li = new HtmlGenericControl("li"))
                {

                    // service count for selected councils for main heading 
                    int mainHeadingServiceCount = heading.GetServiceCount(this.context.EsccSelected, this.context.EastbourneSelected, this.context.HastingsSelected, this.context.LewesSelected, this.context.RotherSelected, this.context.WealdenSelected);

                    // type 1: linked heading
                    if (heading.GetServiceCount(true, true, true, true, true, true) > 0 && (heading.RedirectUrl == null || heading.RedirectUrl.ToString().Length == 0) && heading.RelatedHeadings.Count == 0)
                    {
                        li.Controls.Add(this.BuildHeadingLink(heading, false));
                    }
                    // type 2: unlinked heading with "see page" redirect
                    else if (heading.RedirectUrl != null && heading.RedirectUrl.ToString().Length > 0)
                    {
                        li.Controls.Add(new LiteralControl(heading.Heading + " &#8211; see page: "));

                        using (HyperLink link = new HyperLink())
                        {
                            link.NavigateUrl = heading.RedirectUrl.ToString();
                            link.Text = (heading.RedirectTitle.Length > 0) ? Server.HtmlEncode(heading.RedirectTitle) : heading.RedirectUrl.ToString();
                            li.Controls.Add(link);
                        }

                    }
                    // type 3: "see also heading, heading and heading or see pages page and page" heading
                    // (ie: any heading with related headings)
                    else if (heading.RelatedHeadings.Count > 0)
                    {
                        // link the main heading if it has services
                        bool mainHeadingLinked = (mainHeadingServiceCount > 0);
                        if (mainHeadingLinked)
                        {
                            li.Controls.Add(this.BuildHeadingLink(heading, false));
                        }
                        else
                        {
                            li.Controls.Add(new LiteralControl(HttpUtility.HtmlEncode(heading.Heading)));
                        }

                        // add the related headings
                        this.BuildRelatedHeadingsLinks(heading.RelatedHeadings, li, mainHeadingLinked);
                    }
                    else
                    {
                        // if no matches, must be a heading with no services and no related info - don't bother displaying
                        return null;
                    }

                    return li;
                }
            }
            else return null;
        }

        /// <summary>
        /// Build a HyperLink object to an AZHeading
        /// </summary>
        /// <remarks> Helper method to reduce duplicated code</remarks>
        /// <param name="heading">AZHeading to link to</param>
        /// <returns>Completed HyperLink control</returns>
        private HyperLink BuildHeadingLink(AZHeading heading, bool useRedirectTitle)
        {
            StringBuilder url = new StringBuilder(ResolveUrl("~/")).Append("heading");
            url.Append(heading.Id.ToString(CultureInfo.InvariantCulture));
            url.Append(".aspx?forms="); // forms param obsolete but makes it easier to build up querystring
            if (this.context.EsccSelected) url.Append("&acc=1");
            if (this.context.EastbourneSelected) url.Append("&ae=1");
            if (this.context.HastingsSelected) url.Append("&ah=1");
            if (this.context.LewesSelected) url.Append("&al=1");
            if (this.context.RotherSelected) url.Append("&ar=1");
            if (this.context.WealdenSelected) url.Append("&aw=1");

            using (HyperLink link = new HyperLink())
            {
                link.Text = (useRedirectTitle) ? heading.RedirectTitle : heading.Heading;
                link.Text = Server.HtmlEncode(link.Text);
                link.NavigateUrl = url.ToString();

                return link;
            }
        }

        /// <summary>
        /// Joins ICollection of AZHeading objects into HyperLinks with correct punctuation
        /// </summary>
        /// <param name="relatedHeadings">AZHeading objects related to a main AZHeading</param>
        /// <param name="listItem">The HtmlGenericControl list item to append the text to</param>
        private void BuildRelatedHeadingsLinks(ICollection relatedHeadings, HtmlGenericControl listItem, bool mainHeadingLinked)
        {
            // split into two collections - one for Headings with Services, and one for Headings which redirect
            AZHeading[] normalHeadings = new AZHeading[relatedHeadings.Count];
            AZHeading[] redirectHeadings = new AZHeading[relatedHeadings.Count];
            int normalHeadingCount = 0;
            int redirectHeadingCount = 0;

            foreach (AZHeading heading in relatedHeadings)
            {
                if (heading.RedirectUrl != null)
                {
                    redirectHeadings[redirectHeadingCount] = heading;
                    redirectHeadingCount++;
                }
                else
                {
                    normalHeadings[normalHeadingCount] = heading;
                    normalHeadingCount++;
                }
            }

            // monitor loop separately for grammar, because ICollection doesn't support indexers
            int i;
            int total;

            // if there are any normal headings...
            if (normalHeadingCount > 0)
            {
                // add prompt
                if (mainHeadingLinked)
                {
                    listItem.Controls.Add(new LiteralControl(" &#8211; see also: "));
                }
                else
                {
                    listItem.Controls.Add(new LiteralControl(" &#8211; see: "));
                }

                i = 1;
                total = normalHeadingCount - 1;

                // add links to headings
                foreach (AZHeading heading in normalHeadings)
                {
                    if (heading == null) break;

                    // add link every time
                    listItem.Controls.Add(this.BuildHeadingLink(heading, false));

                    // add punctuation as necessary
                    if (i < total) listItem.Controls.Add(new LiteralControl(", "));
                    else if (i == total) listItem.Controls.Add(new LiteralControl(" and "));

                    // increment loop monitor
                    i++;
                }
            }


            // if there are any redirect headings...
            if (redirectHeadingCount > 0)
            {
                // add prompt, depending on how many headings and whether the main heading was linked
                if (normalHeadingCount > 0)
                {
                    if (redirectHeadingCount > 1)
                    {
                        listItem.Controls.Add(new LiteralControl(" or see pages: "));
                    }
                    else
                    {
                        listItem.Controls.Add(new LiteralControl(" or see page: "));
                    }
                }
                else
                {
                    if (redirectHeadingCount > 1)
                    {
                        if (mainHeadingLinked)
                        {
                            listItem.Controls.Add(new LiteralControl(" &#8211; see also pages: "));
                        }
                        else
                        {
                            listItem.Controls.Add(new LiteralControl(" &#8211; see pages: "));
                        }
                    }
                    else
                    {
                        if (mainHeadingLinked)
                        {
                            listItem.Controls.Add(new LiteralControl(" &#8211; see also page: "));
                        }
                        else
                        {
                            listItem.Controls.Add(new LiteralControl(" &#8211; see page: "));
                        }
                    }
                }

                i = 1;
                total = redirectHeadingCount - 1;

                // add headings
                foreach (AZHeading heading in redirectHeadings)
                {
                    if (heading == null) break;

                    // add link every time
                    //listItem.Controls.Add(this.BuildHeadingLink(heading, true));
                    using (HyperLink link = new HyperLink())
                    {
                        link.NavigateUrl = heading.RedirectUrl.ToString();
                        link.Text = (heading.RedirectTitle.Length > 0) ? heading.RedirectTitle : heading.RedirectUrl.ToString();
                        listItem.Controls.Add(link);
                    }

                    // add punctuation as necessary
                    if (i < total) listItem.Controls.Add(new LiteralControl(", "));
                    else if (i == total) listItem.Controls.Add(new LiteralControl(" and "));

                    // increment loop monitor
                    i++;
                }
            }
        }


    }
}
