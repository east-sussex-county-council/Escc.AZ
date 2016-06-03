using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.HtmlControls;
using Escc.NavigationControls.WebForms;
using Exceptionless;
using Microsoft.ApplicationBlocks.Data;

namespace Escc.AZ.Admin
{
    /// <summary>
    /// Navigation for editing headings in the A-Z
    /// </summary>
    public partial class headings : System.Web.UI.Page
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            // delete heading if requested
            if (Request.QueryString["deleteheading"] != null)
            {
                // get connection details for db
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString))
                {

                    try
                    {
                        SqlParameter[] sqlParams = new SqlParameter[1];
                        sqlParams[0] = new SqlParameter("@headingId", SqlDbType.Int, 4);
                        sqlParams[0].Value = Convert.ToInt32(Request.QueryString["deleteheading"].ToString(), CultureInfo.InvariantCulture);

                        Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "usp_DeleteHeading", sqlParams);

                    }
                    catch (FormatException ex)
                    {
                        // if querystring fiddled, just get rid of the querystring
                        ex.ToExceptionless().Submit();
                        Response.Redirect(Request.Path);
                    }
                }

            }

            // add alphabet navigation
            using (AZNavigation azNav = new AZNavigation())
            {
                azNav.TargetFile = "headings.aspx";
                azNav.SelectedChar = this.GetSelectedChar();
                azNav.ItemSeparator = " ";
                azBox.Controls.Add(azNav);
            }

            // get headings
            DataTable data = this.GetHeadingData();

            // add headings to page
            if (data != null && data.Rows.Count > 0)
            {
                foreach (DataRow row in data.Rows)
                {
                    // build row structure
                    using (HtmlTableRow tableRow = new HtmlTableRow())
                    {
                        using (HtmlTableCell headingCell = new HtmlTableCell())
                        {
                            headingCell.Attributes.Add("scope", "row");
                            tableRow.Controls.Add(headingCell);

                            // add data
                            using (HtmlAnchor editLink = new HtmlAnchor())
                            {
                                editLink.InnerText = row["Heading"].ToString();
                                editLink.HRef = "heading.aspx?heading=" + row["HeadingId"].ToString();
                                headingCell.Controls.Add(editLink);
                            }
                        }

                        using (HtmlTableCell servicesCell = new HtmlTableCell())
                        {
                            tableRow.Controls.Add(servicesCell);

                            servicesCell.InnerText = row["ServiceCount"].ToString();
                        }

                        using (HtmlTableCell actionCell = new HtmlTableCell())
                        {
                            actionCell.Attributes.Add("class", "action");
                            tableRow.Controls.Add(actionCell);

                            if ((int)row["ServiceCount"] <= 0)
                            {
                                using (HtmlAnchor deleteLink = new HtmlAnchor())
                                {
                                    deleteLink.InnerText = "Delete";
                                    deleteLink.HRef = "headings.aspx?index=" + this.GetSelectedChar() + "&amp;deleteheading=" + row["HeadingId"].ToString();
                                    actionCell.Controls.Add(deleteLink);
                                }
                            }
                        }

                        tbody.Controls.Add(tableRow);
                    }
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
        /// Get a list of headings from the database
        /// </summary>
        /// <returns></returns>
        private DataTable GetHeadingData()
        {
            // prepare connection details
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString))
            {
                // build parameter
                SqlParameter[] sqlParams = new SqlParameter[1];
                sqlParams[0] = new SqlParameter("@indexChar", SqlDbType.Char, 1);
                sqlParams[0].Value = this.GetSelectedChar();

                // connect and get data
                return SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "usp_SelectHeadingsByIndexForEdit", sqlParams).Tables[0];
            }
        }
    }
}
