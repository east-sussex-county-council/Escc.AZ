using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.HtmlControls;
using Microsoft.ApplicationBlocks.Data;
using System.Globalization;

namespace Escc.AZ.Admin
{
    /// <summary>
    /// List services with no heading in the A-Z
    /// </summary>
    public partial class orphans : System.Web.UI.Page
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
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

            // get services
            DataTable data = GetData();

            if (data != null)
            {
                // add services to page
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

                        using (HtmlTableCell ipsvCell = new HtmlTableCell())
                        {
                            tableRow.Controls.Add(ipsvCell);

                            if (row["IpsvImported"] != DBNull.Value) ipsvCell.InnerHtml = row["IpsvImported"].ToString().Replace(";", "<br />");
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
                                    deleteLink.HRef = "orphans.aspx?deleteservice=" + row["ServiceId"].ToString();
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
        /// Get a list of orphaned services from the database
        /// </summary>
        /// <returns></returns>
        private static DataTable GetData()
        {
            // connect and get data
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString))
            {
                return SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "usp_SelectOrphanedServicesForEdit").Tables[0];
            }
        }
    }
}
