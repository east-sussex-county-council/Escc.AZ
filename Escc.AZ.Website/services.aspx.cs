using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Escc.Data.Ado;
using EsccWebTeam.Data.Web;
using EsccWebTeam.EastSussexGovUK;
using Exceptionless;

namespace Escc.AZ.Website
{
    /// <summary>
    /// Lists services under a single heading in the website A-Z
    /// </summary>
    public partial class Services : Page
    {

        private AZContext context;

        private int totalServices;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Http.CacheDaily(5, 0);

            this.context = AZContext.Current;

            try
            {
                // get heading data from db
                DataTable serviceData = this.GetServicesData();

                if (serviceData != null && serviceData.Rows.Count > 0)
                {
                    using (HtmlGenericControl tContent = new HtmlGenericControl("div"))
                    {
                        tContent.Attributes["class"] = "text";
                        this.content.Controls.Add(tContent);

                        // convert into AZHeading object - only one will have been retrieved
                        var headings = AZObjectBuilder.BuildHeadingsFromRawData(serviceData);
                        AZHeading heading = headings[0];

                        // bind to page
                        using (HtmlGenericControl h1 = new HtmlGenericControl("h1"))
                        {
                            h1.InnerText = heading.Heading;
                            tContent.Controls.Add(h1);
                        }

                        this.headContent.Title = heading.Heading + " - A-Z of services";
                        this.headContent.IpsvNonPreferredTerms = heading.IpsvNonPreferredTerms;
                        if (heading.IpsvPreferredTerms.Count > 0) this.headContent.IpsvPreferredTerms = heading.IpsvPreferredTerms.ToString();

                        // the "service" query string parameter is intended only for use by our links to a microformats parser, 
                        // so that we can offer a link to download vCard data for one service rather than every service under a heading
                        int onlyService = 0;
                        if (!String.IsNullOrEmpty(Request.QueryString["service"]))
                        {
                            try
                            {
                                onlyService = Int32.Parse(Request.QueryString["service"], CultureInfo.InvariantCulture);
                            }
                            catch (Exception ex)
                            {
                                // If this throws an exception stop immediately. This query string parameter should only ever be requested by our link to 
                                // the Technorati microformats parser, so the link should always be valid and if we return a blank page only the parser should see it.
                                ex.ToExceptionless().Submit();
                                Response.End();
                            }
                        }

                        // write out each service
                        // if there's only one service, pass in main heading - if it's the same as the service heading, the service heading will not be used
                        foreach (AZService service in heading.Services)
                        {
                            // If request is to read only one service, ignore others. This is only ever meant for use by the microformats parser.
                            if (onlyService > 0 && service.Id != onlyService) continue;

                            tContent.Controls.Add(BuildService(service, ((heading.Services.Count == 1 || onlyService > 0) ? heading.Heading : "")));

                            if (service.Keywords.Length > 0)
                            {
                                if (this.headContent.Keywords.Length > 0 && !this.headContent.Keywords.EndsWith(";", StringComparison.Ordinal)) this.headContent.Keywords += ";";
                                this.headContent.Keywords += service.Keywords;
                            }
                        }

                        // if some services from other authorities were hidden, show message
                        if (heading.Services.Count < this.totalServices)
                        {
                            int missingServices = this.totalServices - heading.Services.Count;

                            using (HtmlGenericControl p = new HtmlGenericControl("p"))
                            {
                                if (missingServices == 1)
                                {
                                    p.Controls.Add(new LiteralControl(GetNumber(missingServices) + " service from another council was not shown. "));
                                }
                                else
                                {
                                    p.Controls.Add(new LiteralControl(GetNumber(missingServices) + " services from other councils were not shown. "));
                                }
                                using (HtmlAnchor link = new HtmlAnchor())
                                {
                                    link.InnerText = "Show all services";
                                    link.HRef = "heading" + heading.Id + ".aspx";
                                    p.Controls.Add(link);
                                }
                                p.Controls.Add(new LiteralControl("."));

                                using (HtmlGenericControl div = new HtmlGenericControl("div"))
                                {
                                    div.Attributes["class"] = "infoBar";
                                    div.Controls.Add(p);
                                    tContent.Controls.Add(div);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // if there are no services, print a message saying so (shouldn't be able to get here in the first place)
                    EastSussexGovUKContext.HttpStatus404NotFound(this.content);
                }
            }
            catch (ArgumentException)
            {
                EastSussexGovUKContext.HttpStatus400BadRequest(this.content);
            }
        }

        /// <summary>
        /// Gets text representing the number formatted according to house style
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private static string GetNumber(int number)
        {
            switch (number)
            {
                case 0:
                    return "Zero";
                case 1:
                    return "One";
                case 2:
                    return "Two";
                case 3:
                    return "Three";
                case 4:
                    return "Four";
                case 5:
                    return "Five";
                case 6:
                    return "Six";
                case 7:
                    return "Seven";
                case 8:
                    return "Eight";
                case 9:
                    return "Nine";
                default:
                    return number.ToString(CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Get a list of headings from the database
        /// </summary>
        /// <returns></returns>
        private DataTable GetServicesData()
        {
            try
            {
                // ensure the querystring is passed correctly
                if (Request.QueryString["heading"] == null) throw new ArgumentException("Heading id not supplied");

                /// build parameter
                SqlParameter[] sqlParams = new SqlParameter[8];
                sqlParams[0] = new SqlParameter("@headingId", SqlDbType.Int, 4);
                sqlParams[0].Value = Convert.ToInt32(Request.QueryString["heading"], CultureInfo.InvariantCulture);

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

                sqlParams[7] = new SqlParameter("@totalServices", SqlDbType.Int, 4);
                sqlParams[7].Direction = ParameterDirection.Output;


                // connect and get data
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConnectionStringAZ"].ConnectionString);
                try
                {
                    var tables = EsccSqlHelper.ExecuteDatatable(conn, CommandType.StoredProcedure, "usp_SelectServicesForHeading", sqlParams);
                    this.totalServices = Convert.ToInt32(sqlParams[7].Value, CultureInfo.CurrentCulture);
                    return tables;
                }
                finally
                {
                    conn.Close();
                }
            }
            catch (FormatException)
            {
                // FormatException most likely an inappropriate querystring value 
                EastSussexGovUKContext.HttpStatus400BadRequest(this.content);
                return null;
            }
        }




        /// <summary>
        /// Build the XHTML for a service
        /// </summary>
        /// <param name="service">The AZService containing the data</param>
        /// <param name="mainHeading">Main heading of page, if we want to make sure one-and-only service doesn't duplicate it</param>
        /// <returns>a Control which can be added to the page</returns>
        private Control BuildService(AZService service, string mainHeading)
        {
            // create container control
            using (HtmlGenericControl div = new HtmlGenericControl("div"))
            {
                div.Attributes.Add("class", "azService");

                // get the name of the appropriate authority
                string authName = "";
                switch (service.Authority)
                {
                    case Authority.Eastbourne:
                        authName = "Eastbourne Borough Council";
                        break;

                    case Authority.EastSussex:
                        authName = "East Sussex County Council";
                        break;

                    case Authority.Hastings:
                        authName = "Hastings Borough Council";
                        break;

                    case Authority.Lewes:
                        authName = "Lewes District Council";
                        break;

                    case Authority.Rother:
                        authName = "Rother District Council";
                        break;

                    case Authority.Wealden:
                        authName = "Wealden District Council";
                        break;

                }

                // add heading for the service unless it duplicates the main heading, and is the only service on the page
                using (HtmlGenericControl h2 = new HtmlGenericControl("h2"))
                {
                    if (service.Service.ToUpperInvariant().Trim() != mainHeading.ToUpperInvariant().Trim())
                    {
                        h2.InnerText = service.Service;
                        if (this.context.PartnersEnabled)
                        {
                            h2.InnerText += " (" + authName + ")";
                        }
                    }
                    else if (this.context.PartnersEnabled)
                    {
                        h2.InnerText = authName;
                    }
                    div.Controls.Add(h2);
                }

                // add optional description
                if (!String.IsNullOrWhiteSpace(service.Description))
                {
                    using (HtmlGenericControl desc = new HtmlGenericControl("p"))
                    {
                        desc.InnerText = service.Description;
                        div.Controls.Add(desc);
                    }
                }

                // display contacts, if present
                if (service.Contacts.Count > 0)
                {
                    using (HtmlGenericControl contacth3 = new HtmlGenericControl("h3"))
                    {
                        contacth3.InnerText = "Contacts";
                        div.Controls.Add(contacth3);
                    }
                    div.Controls.Add(new AZContactGrid(service.Contacts));
                }

                // display urls, if present
                if (service.Urls.Count > 0)
                {
                    using (HtmlGenericControl urlh3 = new HtmlGenericControl("h3"))
                    {
                        urlh3.InnerText = "Further information";
                        div.Controls.Add(urlh3);
                    }

                    using (HtmlGenericControl list = new HtmlGenericControl("dl"))
                    {
                        list.Attributes["class"] = "az";
                        foreach (AZUrl link in service.Urls)
                        {
                            using (HtmlAnchor a = new HtmlAnchor())
                            {
                                a.HRef = link.Url.ToString().Replace("+", "%20"); // Wealden's site doesn't treat "+" and "%20" equally. 
                                a.InnerText = (link.Text.StartsWith("http:", StringComparison.Ordinal)) ? Iri.ShortenForDisplay(new Uri(link.Text)) : link.Text;

                                using (HtmlGenericControl dt = new HtmlGenericControl("dt"))
                                {
                                    dt.Controls.Add(a);
                                    list.Controls.Add(dt);
                                }
                            }

                            if (link.Description != null && link.Description.Length > 0)
                            {
                                using (HtmlGenericControl dd = new HtmlGenericControl("dd"))
                                {
                                    dd.InnerText = link.Description;
                                    list.Controls.Add(dd);
                                }
                            }
                        }
                        div.Controls.Add(list);
                    }
                }

                return div;
            }
        }

    }
}
