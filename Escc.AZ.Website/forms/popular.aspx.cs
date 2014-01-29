using System.Configuration;
using System.Data;
using System.Web.UI;
using Microsoft.ApplicationBlocks.Data;

namespace Escc.AZ.Website.forms
{
    /// <summary>
    /// The most popular forms page in the A-Z of forms, which lists links.
    /// </summary>
    public partial class Popular : Page
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            // get data
            DataTable data = SqlHelper.ExecuteDataset(ConfigurationManager.AppSettings["DbConnectionStringAZ"], CommandType.StoredProcedure, "usp_UrlSelectPopularForms").Tables[0];

            this.formList.DataSource = data.DefaultView;
            this.formList.DataBind();
        }
    }
}
