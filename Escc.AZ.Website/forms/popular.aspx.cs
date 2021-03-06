using System.Configuration;
using System.Data;
using System.Web.UI;
using Escc.Data.Ado;
using Escc.EastSussexGovUK.Skins;
using Escc.EastSussexGovUK.Views;
using Escc.EastSussexGovUK.WebForms;

namespace Escc.AZ.Website.forms
{
    /// <summary>
    /// The most popular forms page in the A-Z of forms, which lists links.
    /// </summary>
    public partial class Popular : Page
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            var skinnable = Master as BaseMasterPage;
            if (skinnable != null)
            {
                skinnable.Skin = new CustomerFocusSkin(ViewSelector.CurrentViewIs(MasterPageFile));
            }

            // get data
            DataTable data = EsccSqlHelper.ExecuteDatatable(ConfigurationManager.ConnectionStrings["DbConnectionStringAZ"].ConnectionString, CommandType.StoredProcedure, "usp_UrlSelectPopularForms");

            this.formList.DataSource = data.DefaultView;
            this.formList.DataBind();
        }
    }
}
