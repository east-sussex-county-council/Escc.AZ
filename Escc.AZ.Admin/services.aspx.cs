using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.HtmlControls;
using Escc.NavigationControls.WebForms;
using EsccWebTeam.EastSussexGovUK.MasterPages;
using Microsoft.ApplicationBlocks.Data;

namespace Escc.AZ.Admin
{
    /// <summary>
    /// Navigation for editing services in the A-Z
    /// </summary>
    public partial class services : System.Web.UI.Page
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            var skinnable = Master as BaseMasterPage;
            if (skinnable != null)
            {
                skinnable.Skin = new CustomerFocusSkin(ViewSelector.CurrentViewIs(MasterPageFile));
            }

            // delete service if requested
            if (Request.QueryString["deleteservice"] != null)
            {
                // get connection details for db
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString);

                try
                {
                    SqlParameter[] sqlParams = new SqlParameter[1];
                    sqlParams[0] = new SqlParameter("@serviceId", SqlDbType.Int, 4);
                    sqlParams[0].Value = Convert.ToInt32(Request.QueryString["deleteservice"].ToString(), CultureInfo.InvariantCulture);

                    Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "usp_DeleteService", sqlParams);

                }
                catch (FormatException)
                {
                    // if querystring fiddled, just get rid of the querystring
                    Response.Redirect(Request.Path);
                }
                finally
                {
                    // always close connection
                    conn.Close();
                }

            }

            // add alphabet navigation
            using (AZNavigation azNav = new AZNavigation())
            {
                azNav.TargetFile = "services.aspx";
                azNav.SelectedChar = this.GetSelectedChar();
                azNav.ItemSeparator = " ";
                azBox.Controls.Add(azNav);
            }

            // get headings
            DataTable data = this.GetData();

            // add headings to page
            foreach (DataRow row in data.Rows)
            {
                // build row structure
                using (HtmlTableRow tableRow = new HtmlTableRow())
                {

                    using (HtmlTableCell serviceCell = new HtmlTableCell())
                    {
                        serviceCell.Attributes.Add("scope", "row");
                        tableRow.Controls.Add(serviceCell);

                        using (HtmlAnchor editLink = new HtmlAnchor())
                        {
                            editLink.InnerText = row["Service"].ToString();
                            editLink.HRef = "service.aspx?service=" + row["ServiceId"].ToString();
                            serviceCell.Controls.Add(editLink);
                        }
                    }

                    using (HtmlTableCell authorityCell = new HtmlTableCell())
                    {
                        tableRow.Controls.Add(authorityCell);

                        authorityCell.InnerText = ((Authority)Enum.Parse(typeof(Authority), row["Authority"].ToString())).ToString().Replace("EastSussex", "East Sussex");
                    }

                    using (HtmlTableCell actionCell = new HtmlTableCell())
                    {
                        actionCell.Attributes.Add("class", "action");
                        tableRow.Controls.Add(actionCell);

                        if (((Authority)Enum.Parse(typeof(Authority), row["Authority"].ToString())) == Authority.EastSussex)
                        {
                            using (HtmlAnchor deleteLink = new HtmlAnchor())
                            {
                                deleteLink.InnerText = "Delete";
                                deleteLink.HRef = "services.aspx?index=" + this.GetSelectedChar() + "&amp;deleteservice=" + row["ServiceId"].ToString();
                                actionCell.Controls.Add(deleteLink);
                            }
                        }
                    }

                    tbody.Controls.Add(tableRow);
                }
            }
        }

        /// <summary>
        /// Get the selected A-Z character from the querysting, defaulting to "a" if not selected
        /// </summary>
        /// <returns></returns>
        private string GetSelectedChar()
        {
            return (Request.QueryString["index"] != null && Request.QueryString["index"].ToString().Trim().Length == 1) ? Request.QueryString["index"].ToString().Trim().ToLower(CultureInfo.CurrentCulture) : "a";
        }

        /// <summary>
        /// Get a list of services from the database
        /// </summary>
        /// <returns></returns>
        private DataTable GetData()
        {
            // build parameter
            SqlParameter[] sqlParams = new SqlParameter[1];
            sqlParams[0] = new SqlParameter("@indexChar", SqlDbType.Char, 1);
            sqlParams[0].Value = this.GetSelectedChar();

            // connect and get data
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString))
            {
                return SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "usp_SelectServicesByIndexForEdit", sqlParams).Tables[0];
            }
        }
    }
}
