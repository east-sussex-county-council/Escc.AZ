using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.ApplicationBlocks.Data;

namespace Escc.AZ.Admin
{
    /// <summary>
    /// Allow editing of which forms appear on the "most popular" list
    /// </summary>
    public class popular : System.Web.UI.Page
    {
        protected System.Web.UI.WebControls.Repeater formList;

        private void Page_Load(object sender, System.EventArgs e)
        {
            if (!this.IsPostBack)
            {
                this.GetData();
            }
        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(this.Page_Load);

        }
        #endregion

        /// <summary>
        /// Save the popularity wherever it's been changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void popularity_TextChanged(object sender, EventArgs e)
        {
            SqlParameter[] prms = new SqlParameter[2];
            prms[0] = new SqlParameter("@serviceUrlId", SqlDbType.Int, 4);
            prms[1] = new SqlParameter("@popularity", SqlDbType.TinyInt);

            TextBox box = sender as TextBox;
            HtmlInputHidden idBox = this.FindControl(box.UniqueID.Replace("popularity", "urlid")) as HtmlInputHidden;
            prms[0].Value = idBox.Value;
            prms[1].Value = box.Text;

            SqlHelper.ExecuteNonQuery(ConfigurationManager.AppSettings["DbConnectionStringAZ"], CommandType.StoredProcedure, "usp_UrlUpdatePopularity", prms);

        }

        /// <summary>
        /// Reload the data after Save was clicked and any changes were saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void save_Click(object sender, EventArgs e)
        {
            this.GetData();
        }

        /// <summary>
        /// Load the data from the database
        /// </summary>
        private void GetData()
        {
            // get data
            DataTable data = SqlHelper.ExecuteDataset(ConfigurationManager.AppSettings["DbConnectionStringAZ"], CommandType.StoredProcedure, "usp_UrlSelectFormsForEdit").Tables[0];

            this.formList.DataSource = data;
            this.formList.DataBind();
        }
    }
}
