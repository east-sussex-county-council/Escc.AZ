using System;
using EsccWebTeam.EastSussexGovUK.MasterPages;

namespace Escc.AZ.Admin
{
    /// <summary>
    /// Summary description for IpsvData.
    /// </summary>
    public partial class IpsvData : System.Web.UI.Page
    {
        //		private string sql;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            var skinnable = Master as BaseMasterPage;
            if (skinnable != null)
            {
                skinnable.Skin = new CustomerFocusSkin(ViewSelector.CurrentViewIs(MasterPageFile));
            }
            /*			NameValueCollection config = (NameValueCollection)ConfigurationSettings.GetConfig("egmsXml");

                        XmlDocument ipsv = new XmlDocument();
                        ipsv.Load(config["Ipsv"]);

                        XmlDocument lgcl = new XmlDocument();
                        lgcl.Load(config["Lgcl"]);
			
                        XmlDocument mapping = new XmlDocument();
                        mapping.Load(config["LgclIpsvMapping"]);

                        XmlDocument lgclGclMapping = new XmlDocument();
                        lgclGclMapping.Load(config["LgclGclMapping"]);

                        XmlDocument gcl = new XmlDocument();
                        gcl.Load(config["Gcl"]);
            */
            /***** Populate database with all IPSV terms *****/
            /*			XmlNamespaceManager nsManager = new XmlNamespaceManager(ipsv.NameTable);
                        nsManager.AddNamespace("ns", ipsv.DocumentElement.NamespaceURI);
                        XmlNodeList found = ipsv.DocumentElement.SelectNodes("/ns:ControlledList/ns:Item[@Preferred='true']", nsManager);

                        SqlConnection conn = new SqlConnection(ConfigurationSettings.ConnectionStrings["DBConnectionStringAZ"].ConnectionString);
                        conn.Open();
                        foreach (XmlNode node in found)
                        {
                            EsdTerm term = EsdSearches.GetEsdTermFromXmlNode(node);

                            SqlCommand cmd = new SqlCommand("INSERT INTO Category (Identifier, Scheme, CommonName) VALUES ('" + term.Id.ToString() + "', '" + term.ControlledList.AbbreviatedName + "', '" + term.Text.Replace("'", "''") + "')", conn);
                            cmd.CommandType = CommandType.Text;

                            cmd.ExecuteNonQuery();
                        }

            */
            /***** Look up current terms against LGCL, GCL and IPSV ****/
            /*			DataSet headings = SqlHelper.ExecuteDataset(conn, CommandType.Text, "SELECT HeadingId, Heading FROM Heading");
                        try
                        {
                            foreach (DataRow row in headings.Tables[0].Rows)
                            {
                                string heading = row["Heading"].ToString();
                                EsdTermCollection terms = EsdSearches.GetTerms(lgcl, heading, true, EsdPreferredState.Preferred);
                                if (terms.Count > 0)
                                {
                                    EsdTermCollection gclTerms = EsdSearches.GetMappedTerms(lgclGclMapping, gcl, terms[0].Id);
                                    if (gclTerms.Count > 0)
                                    {
                                        this.sql = "UPDATE Heading SET GclCategories = '" + gclTerms.ToString().Replace("'", "''") + "' WHERE HeadingId = " + row["HeadingId"].ToString();
                                        SqlHelper.ExecuteNonQuery(conn, CommandType.Text, this.sql);
                                    }

                                    EsdTermCollection mapped = EsdSearches.GetMappedTerms(mapping, ipsv, terms[0].Id, true);
                                    if (mapped.Count > 0)
                                    {
                                        StringBuilder nonPrefs = new StringBuilder();

                                        foreach (EsdTerm map in mapped)
                                        {
                                            this.sql = "SELECT CategoryId FROM Category WHERE Identifier = '" + map.Id + "'";
                                            object ipsvId = SqlHelper.ExecuteScalar(conn, CommandType.Text, this.sql);
								
                                            this.sql = "INSERT INTO Heading_Category (HeadingId, CategoryId) VALUES (" + row["HeadingId"].ToString() + ", " + ipsvId.ToString() +  ")";
                                            SqlHelper.ExecuteNonQuery(conn, CommandType.Text, this.sql);

                                            map.NonPreferredTerms = EsdSearches.GetNonPreferredTerms(ipsv, map.Id);
                                            map.NonPreferredTerms.AppendText(nonPrefs);
                                        }

                                        if (nonPrefs.Length > 0)
                                        {
                                            this.sql = "UPDATE Heading SET IpsvNonPreferredTerms = '" + nonPrefs.ToString().Replace("'", "''") + "' WHERE HeadingId = " + row["HeadingId"].ToString();
                                            SqlHelper.ExecuteNonQuery(conn, CommandType.Text, this.sql);
                                        }
                                    }
                                }
                                else
                                {
                                    terms = EsdSearches.GetTerms(ipsv, heading, true, EsdPreferredState.Preferred);
                                    if (terms.Count > 0)
                                    {
                                        this.sql = "SELECT CategoryId FROM Category WHERE Identifier = '" + terms[0].Id + "'";
                                        object ipsvId = SqlHelper.ExecuteScalar(conn, CommandType.Text, this.sql);

                                        this.sql = "INSERT INTO Heading_Category (HeadingId, CategoryId) VALUES (" + row["HeadingId"].ToString() + ", " + ipsvId.ToString() +  ")";
                                        SqlHelper.ExecuteNonQuery(conn, CommandType.Text, this.sql);

                                        terms[0].NonPreferredTerms = EsdSearches.GetNonPreferredTerms(ipsv, terms[0].Id);
                                        if (terms[0].NonPreferredTerms.Count > 0) 
                                        {
                                            this.sql = "UPDATE Heading SET IpsvNonPreferredTerms = '" + terms[0].NonPreferredTerms.ToString().Replace("'", "''") + "' WHERE HeadingId = " + row["HeadingId"].ToString();
                                            SqlHelper.ExecuteNonQuery(conn, CommandType.Text, this.sql);
                                        }

                                        EsdTermCollection lgclTerms = EsdSearches.GetMappedTerms(mapping, lgcl, terms[0].Id, false);
                                        EsdTermCollection gclTerms = EsdSearches.GetMappedTerms(lgclGclMapping, gcl, lgclTerms);
                                        if (gclTerms.Count > 0)
                                        {
                                            this.sql = "UPDATE Heading SET GclCategories = '" + gclTerms.ToString().Replace("'", "''") + "' WHERE HeadingId = " + row["HeadingId"].ToString();
                                            SqlHelper.ExecuteNonQuery(conn, CommandType.Text, this.sql);
                                        }

                                    }

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string test="";
                        }
                        finally
                        {
                            conn.Close();
                        }
            */
        }
    }
}
