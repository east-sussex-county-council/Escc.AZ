using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.UI;
using Escc.Data.Ado;
using EsccWebTeam.EastSussexGovUK.MasterPages;

namespace Escc.AZ.Website.forms
{
    /// <summary>
    /// The main A-Z of forms page, which lists links.
    /// </summary>
    public partial class Forms : Page
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            var skinnable = Master as BaseMasterPage;
            if (skinnable != null)
            {
                skinnable.Skin = new CustomerFocusSkin(ViewSelector.CurrentViewIs(MasterPageFile));
            }

            // get data
            SqlParameter indexChars = new SqlParameter("@indexChars", SqlDbType.VarChar, 5);
            indexChars.Value = "abc"; // default
            this.navigation.SelectedChar = "a";

            if (Request.QueryString["index"] != null)
            {
                // only A-Z allowed, max five characters
                if (Regex.IsMatch(Request.QueryString["index"], "^[a-z]{1,5}$"))
                {
                    indexChars.Value = Request.QueryString["index"];
                    this.navigation.SelectedChar = Request.QueryString["index"];
                }
            }
            this.headContent.Title = String.Format(CultureInfo.CurrentCulture, this.headContent.Title, indexChars.Value.ToString().ToUpper(CultureInfo.CurrentCulture));

            DataTable data = EsccSqlHelper.ExecuteDatatable(ConfigurationManager.ConnectionStrings["DbConnectionStringAZ"].ConnectionString, CommandType.StoredProcedure, "usp_UrlSelectForms", indexChars);

            if (data.Rows.Count > 0)
            {
                this.formList.DataSource = data.DefaultView;
                this.formList.DataBind();
                this.formList.Visible = true;
                this.noForms.Visible = false;
            }
            else
            {
                this.formList.Visible = false;
                this.noForms.Visible = true;
            }
        }
    }
}
