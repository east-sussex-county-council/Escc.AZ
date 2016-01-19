using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using EsccWebTeam.Data.Web;
using Microsoft.ApplicationBlocks.Data;

namespace Escc.AZ.Admin
{
    /// <summary>
    /// Manage the data for a service in the A-Z
    /// </summary>
    public partial class contact : System.Web.UI.Page
    {
        protected System.Web.UI.WebControls.TextBox address2;
        protected System.Web.UI.WebControls.TextBox address3;
        protected System.Web.UI.WebControls.CustomValidator requireTownAndCountyValidator;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Web.UI.WebControls.TextBox.set_Text(System.String)")]
        protected void Page_Load(object sender, System.EventArgs e)
        {
            if (!IsPostBack)
            {
                // if not postback, check a querystring specified with a service or contact id
                try
                {
                    // get heading id from querystring - conversion ensures it's a number
                    if (Request.QueryString["service"] != null) this.serviceId.Value = Convert.ToInt32(Request.QueryString["service"], CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                    if (Request.QueryString["contact"] != null) this.contactId.Value = Convert.ToInt32(Request.QueryString["contact"], CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    // just redirect to default page if querystring is bad
                    Response.Redirect("default.aspx");
                    Response.End();
                }


                // get the initial data on the heading from the db
                DataTable data = this.GetData();

                // bind the data to the form
                this.PopulateForm(data);
            }
            else
            {
                // uppercase before validation
                this.postcode.Text = this.postcode.Text.ToUpper(CultureInfo.CurrentCulture);

                // replace invalid characters before validation
                this.paon.Text = this.paon.Text.Replace("–", "-");
                this.saon.Text = this.saon.Text.Replace("–", "-");
            }

            // set page heading
            this.SetPageHeading();
        }

        /// <summary>
        /// Sets the page title and heading to say whether we're editing or creating a record
        /// </summary>
        private void SetPageHeading()
        {
            this.headContent.Title = (String.IsNullOrEmpty(this.contactId.Value)) ? "New A&#8211;Z contact" : "Edit A&#8211;Z contact";
            this.h1.InnerHtml = this.headContent.Title;
        }


        /// <summary>
        /// Get raw data from database based on contact id
        /// </summary>
        /// <returns>Raw data in a DataTable</returns>
        private DataTable GetData()
        {
            if (!String.IsNullOrEmpty(this.contactId.Value))
            {
                // create disconnected connection details
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString);

                try
                {
                    // get from the db details of the current contact
                    SqlParameter[] sqlParams = new SqlParameter[1];
                    sqlParams[0] = new SqlParameter("@contactId", SqlDbType.Int, 4);
                    sqlParams[0].Value = Convert.ToInt32(this.contactId.Value, CultureInfo.InvariantCulture);

                    return SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "usp_SelectContactForEdit", sqlParams).Tables[0];
                }
                finally
                {
                    // always close connection
                    conn.Close();
                }

            }

            return null;
        }


        /// <summary>
        /// Bind data in dataset to the page
        /// </summary>
        /// <param name="data">Raw data from db. Table at index 0 should contain all headings in the db. Table at index 1 should contain data for the heading being edited.</param>
        private void PopulateForm(DataTable data)
        {
            // expect one row of data
            if (data != null && data.Rows.Count == 1)
            {
                DataRow row = data.Rows[0];

                // populate fields
                this.serviceId.Value = row["ServiceId"].ToString();
                this.firstName.Text = row["FirstName"].ToString();
                this.lastName.Text = row["LastName"].ToString();
                this.description.Text = row["ContactDescription"].ToString();
                this.phoneArea.Text = row["PhoneArea"].ToString().Trim();
                this.phone.Text = row["Phone"].ToString();
                this.faxArea.Text = row["FaxArea"].ToString().Trim();
                this.fax.Text = row["Fax"].ToString();
                this.email.Text = row["Email"].ToString();
                this.emailText.Text = row["EmailText"].ToString();
                this.paon.Text = row["PAON"].ToString();
                this.saon.Text = row["SAON"].ToString();
                this.street.Text = row["StreetDescription"].ToString();
                this.locality.Text = row["Locality"].ToString();
                this.town.Text = row["Town"].ToString();
                this.postcode.Text = row["Postcode"].ToString().Trim();
                this.addressUrl.Value = (row["AddressUrl"] != DBNull.Value) ? row["AddressUrl"].ToString().Trim() : "";
                this.addressUrlText.Value = (row["AddressUrlText"] != DBNull.Value) ? row["AddressUrlText"].ToString().Trim() : "";

                ListItem countyItem = this.county.Items.FindByValue(row["County"].ToString());
                if (countyItem != null) countyItem.Selected = true;
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
                // prepare parameters
                SqlParameter[] sqlParams = new SqlParameter[22];

                // contact id parameter differs for insert & update
                sqlParams[0] = new SqlParameter("@contactId", SqlDbType.Int, 4);
                if (this.contactId.Value.Length > 0)
                {
                    sqlParams[0].Value = this.contactId.Value;
                }
                else
                {
                    sqlParams[0].Direction = ParameterDirection.Output;
                }

                sqlParams[1] = new SqlParameter("@serviceId", SqlDbType.Int, 4);
                sqlParams[1].Value = Convert.ToInt32(this.serviceId.Value, CultureInfo.InvariantCulture);

                sqlParams[2] = new SqlParameter("@firstName", SqlDbType.VarChar, 35);
                sqlParams[2].Value = this.firstName.Text.Trim();

                sqlParams[3] = new SqlParameter("@lastName", SqlDbType.VarChar, 35);
                sqlParams[3].Value = this.lastName.Text.Trim();

                sqlParams[4] = new SqlParameter("@description", SqlDbType.VarChar, 255);
                sqlParams[4].Value = this.description.Text.Trim();

                sqlParams[5] = new SqlParameter("@phoneArea", SqlDbType.Char, 5);
                sqlParams[5].Value = this.phoneArea.Text.Trim();

                sqlParams[6] = new SqlParameter("@phone", SqlDbType.VarChar, 9);
                sqlParams[6].Value = this.phone.Text.Trim();

                sqlParams[7] = new SqlParameter("@phoneExtension", SqlDbType.VarChar, 8);
                sqlParams[7].Value = this.phoneExtension.Text.Trim();

                sqlParams[8] = new SqlParameter("@faxArea", SqlDbType.Char, 5);
                sqlParams[8].Value = this.faxArea.Text.Trim();

                sqlParams[9] = new SqlParameter("@fax", SqlDbType.VarChar, 8);
                sqlParams[9].Value = this.fax.Text.Trim();

                sqlParams[10] = new SqlParameter("@faxExtension", SqlDbType.VarChar, 8);
                sqlParams[10].Value = this.faxExtension.Text.Trim();

                sqlParams[11] = new SqlParameter("@email", SqlDbType.VarChar, 255);
                sqlParams[11].Value = this.email.Text.Trim();

                sqlParams[12] = new SqlParameter("@emailText", SqlDbType.VarChar, 75);
                sqlParams[12].Value = this.emailText.Text.Trim();

                sqlParams[13] = new SqlParameter("@paon", SqlDbType.VarChar, 100);
                sqlParams[13].Value = this.paon.Text.Trim();

                sqlParams[14] = new SqlParameter("@saon", SqlDbType.VarChar, 100);
                sqlParams[14].Value = this.saon.Text.Trim();

                sqlParams[15] = new SqlParameter("@streetDescription", SqlDbType.VarChar, 100);
                sqlParams[15].Value = this.street.Text.Trim();

                sqlParams[16] = new SqlParameter("@locality", SqlDbType.VarChar, 35);
                sqlParams[16].Value = this.locality.Text.Trim();

                sqlParams[17] = new SqlParameter("@town", SqlDbType.VarChar, 30);
                sqlParams[17].Value = this.town.Text.Trim();

                sqlParams[18] = new SqlParameter("@county", SqlDbType.VarChar, 30);
                sqlParams[18].Value = Request.Form["ctl00$content$county"].ToString().Trim();

                sqlParams[19] = new SqlParameter("@postcode", SqlDbType.Char, 8);
                sqlParams[19].Value = this.postcode.Text.Trim();

                sqlParams[20] = new SqlParameter("@addressUrl", SqlDbType.VarChar, 255);
                sqlParams[20].Value = this.addressUrl.Value.Trim();

                sqlParams[21] = new SqlParameter("@addressUrlText", SqlDbType.VarChar, 75);
                sqlParams[21].Value = this.addressUrlText.Value.Trim();

                // prepare connection
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionStringAZ"].ConnectionString))
                {

                    // update the db
                    // if contact id, update, else insert new record
                    if (this.contactId.Value.Length > 0)
                    {
                        Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "usp_UpdateContact", sqlParams);
                    }
                    else
                    {
                        // insert the record
                        Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "usp_InsertContact", sqlParams);

                        // get new id value
                        this.contactId.Value = sqlParams[0].Value.ToString();

                    }
                }

                // go back to service (which should show changed data)
                Http.Status303SeeOther(new Uri("service.aspx?service=" + this.serviceId.Value, UriKind.Relative));
            }

        }

        /// <summary>
        /// Custom validator to ensure first name has last name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CheckFirstNameHasLastName(object sender, ServerValidateEventArgs e)
        {
            if (e == null) throw new ArgumentNullException("e");
            e.IsValid = (this.lastName.Text.Trim().Length > 0);
        }

        /// <summary>
        /// Custom validator to ensure last name has first name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CheckLastNameHasFirstName(object sender, ServerValidateEventArgs e)
        {
            if (e == null) throw new ArgumentNullException("e");
            e.IsValid = (this.firstName.Text.Trim().Length > 0);
        }

        /// <summary>
        /// Custom validator to make sure phone number and area code are entered together
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CheckPhoneAreaHasPhone(object sender, ServerValidateEventArgs e)
        {
            if (e == null) throw new ArgumentNullException("e");
            e.IsValid = (this.phone.Text.Trim().Length > 0);
        }

        /// <summary>
        /// Custom validator to make sure phone number and area code are entered together
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CheckPhoneHasArea(object sender, ServerValidateEventArgs e)
        {
            if (e == null) throw new ArgumentNullException("e");
            e.IsValid = (this.phoneArea.Text.Trim().Length > 0);
        }

        /// <summary>
        /// Custom validator to make sure fax number and area code are entered together
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CheckFaxAreaHasFax(object sender, ServerValidateEventArgs e)
        {
            if (e == null) throw new ArgumentNullException("e");
            e.IsValid = (this.fax.Text.Trim().Length > 0);
        }

        /// <summary>
        /// Custom validator to make sure fax number and area code are entered together
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CheckFaxHasArea(object sender, ServerValidateEventArgs e)
        {
            if (e == null) throw new ArgumentNullException("e");
            e.IsValid = (this.faxArea.Text.Trim().Length > 0);
        }

        /// <summary>
        /// Custom validator to ensure an email recipient has an email address
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CheckForEmailAddress(object sender, ServerValidateEventArgs e)
        {
            if (e == null) throw new ArgumentNullException("e");
            e.IsValid = (this.email.Text.Trim().Length > 0);
        }

        /// <summary>
        /// Custom validator to make sure addresses have town and county
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CheckTownAndCounty(object sender, ServerValidateEventArgs e)
        {
            if (e == null) throw new ArgumentNullException("e");
            e.IsValid = (this.town.Text.Trim().Length > 0 && this.county.SelectedIndex > 0);
        }

        /// <summary>
        /// Custom validator to ensure all addresses have an street address filled in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CheckForStreetAddress(object sender, ServerValidateEventArgs e)
        {
            if (e == null) throw new ArgumentNullException("e");
            e.IsValid = (this.paon.Text.Trim().Length > 0 || this.saon.Text.Trim().Length > 0 || this.street.Text.Trim().Length > 0 || this.locality.Text.Trim().Length > 0);
        }

        /// <summary>
        /// Custom validator to ensure phone extension has phone number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CheckPhoneExtensionHasPhone(object sender, ServerValidateEventArgs e)
        {
            if (e == null) throw new ArgumentNullException("e");
            e.IsValid = (this.phoneArea.Text.Trim().Length > 0 && this.phone.Text.Trim().Length > 0);
        }

        /// <summary>
        /// Custom validator to ensure fax extension has fax number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CheckFaxExtensionHasFax(object sender, ServerValidateEventArgs e)
        {
            if (e == null) throw new ArgumentNullException("e");
            e.IsValid = (this.faxArea.Text.Trim().Length > 0 && this.fax.Text.Trim().Length > 0);
        }
    }
}
