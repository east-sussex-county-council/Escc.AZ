using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Escc.EastSussexGovUK.Skins;
using Escc.EastSussexGovUK.Views;
using Escc.EastSussexGovUK.WebForms;
using Escc.Web.Metadata;
using Microsoft.ApplicationBlocks.Data;

namespace Escc.AZ.Admin
{
    /// <summary>
    /// Page to kick-off rarely-used admin functions
    /// </summary>
    public partial class admin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var skinnable = Master as BaseMasterPage;
            if (skinnable != null)
            {
                skinnable.Skin = new CustomerFocusSkin(ViewSelector.CurrentViewIs(MasterPageFile));
            }
        }

        protected void submit_Click(object sender, EventArgs e)
        {
            if (this.tasks.Items[0].Selected)
            {
                UpdateIpsvHeadings();
            }
            else if (this.tasks.Items[1].Selected)
            {
                UpdateIpsvServices();
            }
        }

        /// <summary>
        /// Updates data on which headings are IPSV terms
        /// </summary>
        private static void UpdateIpsvHeadings()
        {
            // Get all headings out of the database and into a collection
            SqlDataReader dr = SqlHelper.ExecuteReader(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString, "usp_SelectAllHeadings");
            IList<AZHeading> hc = new List<AZHeading>();

            try
            {
                while (dr.Read())
                {
                    AZHeading h = new AZHeading();
                    h.Id = Int32.Parse(dr["HeadingId"].ToString(), CultureInfo.InvariantCulture);
                    h.Heading = dr["Heading"].ToString();
                    hc.Add(h);
                }
            }
            finally
            {
                dr.Close();
            }

            // Check each heading to see whether it's an IPSV term
            EsdControlledList ipsv = EsdControlledList.GetControlledList("Ipsv");

            foreach (AZHeading h in hc)
            {
                EsdTermCollection matchedTerms = ipsv.GetTerms(h.Heading, true, EsdPreferredState.Preferred);
                h.Ipsv = (matchedTerms.Count == 1);
            }

            // Update each heading in the database
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString))
            {
                SqlParameter p1 = new SqlParameter("@headingId", SqlDbType.Int, 4);
                SqlParameter p2 = new SqlParameter("@ipsv", SqlDbType.Bit);

                foreach (AZHeading h in hc)
                {
                    p1.Value = h.Id;
                    p2.Value = h.Ipsv;

                    SqlHelper.ExecuteNonQuery(conn, "usp_HeadingUpdateIpsv", p1, p2);
                }
            }

        }


        /// <summary>
        /// Updates data on which services are IPSV terms
        /// </summary>
        private static void UpdateIpsvServices()
        {
            // Get all services out of the database and into a collection
            SqlDataReader dr = SqlHelper.ExecuteReader(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString, "usp_SelectAllServices");
            AZHeading services = new AZHeading();

            try
            {
                while (dr.Read())
                {
                    AZService sv = new AZService();
                    sv.Authority = (Authority)Enum.Parse(typeof(Authority), dr["Authority"].ToString(), true);

                    if (sv.Authority == Authority.EastSussex)
                    {
                        sv.Id = Int32.Parse(dr["ServiceId"].ToString(), CultureInfo.InvariantCulture);
                        sv.Service = dr["Service"].ToString();
                        services.AddService(sv);
                    }
                }
            }
            finally
            {
                dr.Close();
            }

            // Check each service to see whether it's an IPSV term
            EsdControlledList ipsv = EsdControlledList.GetControlledList("Ipsv");

            foreach (AZService sv in (ArrayList)services.Services)
            {
                EsdTermCollection matchedTerms = ipsv.GetTerms(sv.Service, true, EsdPreferredState.Preferred);
                sv.Ipsv = (matchedTerms.Count == 1);
            }

            // Update each service in the database
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString))
            {
                SqlParameter p1 = new SqlParameter("@serviceId", SqlDbType.Int, 4);
                SqlParameter p2 = new SqlParameter("@ipsv", SqlDbType.Bit);

                foreach (AZService sv in (ArrayList)services.Services)
                {
                    p1.Value = sv.Id;
                    p2.Value = sv.Ipsv;

                    SqlHelper.ExecuteNonQuery(conn, "usp_ServiceUpdateIpsv", p1, p2);
                }
            }

        }
    }
}
