using System;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using EsccWebTeam.NavigationControls;

namespace Escc.AZ
{
    /// <summary>
    /// Navigation for A-Z of services, including search form and a-z listing
    /// </summary>
    [DefaultProperty("Text"),
        ToolboxData("<{0}:AZSearchForm runat=server></{0}:AZSearchForm>")]
    public class AZSearchForm : PlaceHolder
    {
        private AZContext azContext;

        /// <summary>
        /// Navigation for A-Z of services, including search form and a-z listing
        /// </summary>
        public AZSearchForm()
            : base()
        {
            // shorthand
            HttpRequest request = HttpContext.Current.Request;
            HttpResponse response = HttpContext.Current.Response;
            this.azContext = new AZContext();

            // check whether a search has been performed, and put it in the querystring
            if (request.Form["ctl00$content$azq"] != null)
            {
                try
                {
                    StringBuilder url = new StringBuilder("default.aspx?azq=");
                    url.Append(this.Context.Server.UrlEncode(request.Form["ctl00$content$azq"].Trim().ToString()));
                    if (this.azContext.PartnersEnabled)
                    {
                        if (request.Form["autoPostback"] != null) url.Append("&autoPostback=").Append(this.Context.Server.UrlEncode(request.Form["autoPostback"].Trim()));
                        if (request.Form["ctl00$content$acc"] != null) url.Append("&acc=").Append(this.Context.Server.UrlEncode(request.Form["ctl00$content$acc"].Trim()));
                        if (request.Form["ctl00$content$ae"] != null) url.Append("&ae=").Append(this.Context.Server.UrlEncode(request.Form["ctl00$content$ae"].Trim()));
                        if (request.Form["ctl00$content$ah"] != null) url.Append("&ah=").Append(this.Context.Server.UrlEncode(request.Form["ctl00$content$ah"].Trim()));
                        if (request.Form["ctl00$content$al"] != null) url.Append("&al=").Append(this.Context.Server.UrlEncode(request.Form["ctl00$content$al"].Trim()));
                        if (request.Form["ctl00$content$ar"] != null) url.Append("&ar=").Append(this.Context.Server.UrlEncode(request.Form["ctl00$content$ar"].Trim()));
                        if (request.Form["ctl00$content$aw"] != null) url.Append("&aw=").Append(this.Context.Server.UrlEncode(request.Form["ctl00$content$aw"].Trim()));
                    }
                    if (request.Form["ctl00$content$index"] != null) url.Append("&index=").Append(this.Context.Server.UrlEncode(request.Form["ctl00$content$index"].Trim()));

                    response.Redirect(url.ToString());
                    response.End();
                }
                catch (NullReferenceException)
                {
                    // redirect and publish exception if fields fiddled
                    response.Redirect("default.aspx");
                    throw;
                }
            }


        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            // shorthand
            HttpRequest request = HttpContext.Current.Request;

            // format this box
            this.Controls.Add(new LiteralControl("<div id=\"azSearchForm\">"));

            // create a hidden field to select an index letter
            using (var index = new HtmlInputHidden())
            {
                index.ID = "index";
                if (request.QueryString["index"] != null) index.Value = request.QueryString["index"];
                this.Controls.Add(index);
            }

            // create the search box
            using (var searchBox = new TextBox())
            {
                searchBox.MaxLength = 50;
                searchBox.ID = "azq";
                if (request.QueryString["azq"] != null) searchBox.Text = request.QueryString["azq"].Trim();


                // and a div to contain the whole row
                using (var rowDiv = new HtmlGenericControl("div"))
                {
                    rowDiv.Attributes.Add("class", "formPart");
                    this.Controls.Add(rowDiv);

                    // create a label for the search box
                    using (var label = new HtmlGenericControl("label"))
                    {
                        label.Attributes.Add("class", "azSearchLabel");
                        label.Attributes.Add("for", this.Parent.UniqueID.Replace("$", "_") + "_" + searchBox.ClientID);
                        label.InnerHtml = "Search services";

                        using (var labelDiv = new HtmlGenericControl("div"))
                        {
                            labelDiv.Attributes.Add("class", "formLabel");
                            labelDiv.Controls.Add(label);
                            rowDiv.Controls.Add(labelDiv);
                        }
                    }

                    // create a go button
                    using (var go = new HtmlInputSubmit())
                    {
                        go.Value = "Search";
                        go.Attributes["class"] = "button";

                        // and a div to contain them
                        using (var searchDiv = new HtmlGenericControl("div"))
                        {
                            searchDiv.Attributes.Add("class", "formControl");
                            searchDiv.Controls.Add(searchBox);
                            searchDiv.Controls.Add(go);
                            rowDiv.Controls.Add(searchDiv);
                        }
                    }
                }
            }

            if (this.azContext.PartnersEnabled)
            {
                //// create checkboxes for each authority ////
                CreateCheckboxes();

                // add noscript text
                using (var noscript = new HtmlGenericControl("noscript"))
                {
                    noscript.Attributes.Add("class", "noscript");
                    using (var noPara = new HtmlGenericControl("p"))
                    {
                        noPara.InnerText = Properties.Resources.SearchNoScript;
                        noscript.Controls.Add(noPara);
                    }
                    this.Controls.Add(noscript);
                }
            }

            // add containing boxes for grid
            this.Controls.Add(new LiteralControl("<div id=\"azGrid\"><div><div>"));
            using (var azGrid = new HtmlGenericControl("div"))
            {

                // add A-Z grid nav
                using (var azNav = new AZNavigation())
                {
                    if (this.azContext.PartnersEnabled)
                    {
                        if (request.QueryString["autoPostback"] != null) azNav.AddQueryStringParameter("autoPostback", request.QueryString["autoPostback"]);
                        else if (request.QueryString["azq"] == null) azNav.AddQueryStringParameter("autoPostback", "on"); // default to on if form not submitted (ie: not a postback) 
                        if (request.QueryString["acc"] != null) azNav.AddQueryStringParameter("acc", request.QueryString["acc"]);
                        if (request.QueryString["ae"] != null) azNav.AddQueryStringParameter("ae", request.QueryString["ae"]);
                        if (request.QueryString["ah"] != null) azNav.AddQueryStringParameter("ah", request.QueryString["ah"]);
                        if (request.QueryString["al"] != null) azNav.AddQueryStringParameter("al", request.QueryString["al"]);
                        if (request.QueryString["ar"] != null) azNav.AddQueryStringParameter("ar", request.QueryString["ar"]);
                        if (request.QueryString["aw"] != null) azNav.AddQueryStringParameter("aw", request.QueryString["aw"]);
                    }

                    azNav.TargetFile = request.ServerVariables["SCRIPT_NAME"].ToString();
                    if (String.IsNullOrEmpty(request.QueryString["azq"]))
                    {
                        if (!String.IsNullOrEmpty(request.QueryString["index"]))
                        {
                            azNav.SelectedChar = request.QueryString["index"];
                        }
                        else if (String.IsNullOrEmpty(request.QueryString.ToString()))
                        {
                            azNav.SelectedChar = "a";
                        }
                    }

                    azNav.ItemSeparator = " ";
                    azNav.SkipChars = "QX";
                    azGrid.Controls.Add(azNav);
                }
                this.Controls.Add(azGrid);
            }

            this.Controls.Add(new LiteralControl("</div></div></div>"));

            // add bottom corners
            this.Controls.Add(new LiteralControl("</div>"));
        }

        private void CreateCheckboxes()
        {
            using (var councilBoxes = new HtmlGenericControl("fieldset"))
            {
                councilBoxes.ID = "councilList";
                councilBoxes.Attributes.Add("class", "formPart");
                this.Controls.Add(councilBoxes);

                using (var boxIntro = new HtmlGenericControl("legend"))
                {
                    boxIntro.InnerText = Properties.Resources.SearchCouncilsPrompt;
                    councilBoxes.Controls.Add(boxIntro);
                }

                // ESCC
                using (var ccBox = new HtmlInputCheckBox())
                {
                    ccBox.Value = "1";
                    ccBox.ID = "acc";
                    ccBox.Checked = this.azContext.EsccSelected;

                    using (var ccLabel = new HtmlGenericControl("label"))
                    {
                        ccLabel.Attributes.Add("class", "checkboxAlt1");
                        ccLabel.Controls.Add(ccBox);
                        ccLabel.Attributes.Add("for", this.Parent.UniqueID.Replace("$", "_") + "_" + ccBox.ID);
                        ccLabel.Controls.Add(new LiteralControl(Properties.Resources.CouncilNameCounty));
                        councilBoxes.Controls.Add(ccLabel);
                    }
                }

                // Eastbourne
                using (var eBox = new HtmlInputCheckBox())
                {
                    eBox.Value = "1";
                    eBox.ID = "ae";
                    eBox.Checked = this.azContext.EastbourneSelected;

                    using (var eLabel = new HtmlGenericControl("label"))
                    {
                        eLabel.Attributes.Add("class", "checkboxAlt2");
                        eLabel.Controls.Add(eBox);
                        eLabel.Attributes.Add("for", this.Parent.UniqueID.Replace("$", "_") + "_" + eBox.ID);
                        eLabel.Controls.Add(new LiteralControl(Properties.Resources.CouncilNameEastbourne));
                        councilBoxes.Controls.Add(eLabel);
                    }
                }

                // Hastings
                using (var hBox = new HtmlInputCheckBox())
                {
                    hBox.Value = "1";
                    hBox.ID = "ah";
                    hBox.Checked = this.azContext.HastingsSelected;

                    using (var hLabel = new HtmlGenericControl("label"))
                    {
                        hLabel.Attributes.Add("class", "checkboxAlt1");
                        hLabel.Controls.Add(hBox);
                        hLabel.Attributes.Add("for", this.Parent.UniqueID.Replace("$", "_") + "_" + hBox.ID);
                        hLabel.Controls.Add(new LiteralControl(Properties.Resources.CouncilNameHastings));
                        councilBoxes.Controls.Add(hLabel);
                    }
                }

                // Lewes
                using (var lBox = new HtmlInputCheckBox())
                {
                    lBox.Value = "1";
                    lBox.ID = "al";
                    lBox.Checked = this.azContext.LewesSelected;

                    using (var lLabel = new HtmlGenericControl("label"))
                    {
                        lLabel.Attributes.Add("class", "checkboxAlt2");
                        lLabel.Controls.Add(lBox);
                        lLabel.Attributes.Add("for", this.Parent.UniqueID.Replace("$", "_") + "_" + lBox.ID);
                        lLabel.Controls.Add(new LiteralControl(Properties.Resources.CouncilNameLewes));
                        councilBoxes.Controls.Add(lLabel);
                    }
                }

                // Rother
                using (var rBox = new HtmlInputCheckBox())
                {
                    rBox.Value = "1";
                    rBox.ID = "ar";
                    rBox.Checked = this.azContext.RotherSelected;

                    using (var rLabel = new HtmlGenericControl("label"))
                    {
                        rLabel.Attributes.Add("class", "checkboxAlt1");
                        rLabel.Controls.Add(rBox);
                        rLabel.Attributes.Add("for", this.Parent.UniqueID.Replace("$", "_") + "_" + rBox.ID);
                        rLabel.Controls.Add(new LiteralControl(Properties.Resources.CouncilNameRother));
                        councilBoxes.Controls.Add(rLabel);
                    }
                }

                // Wealden
                using (var wBox = new HtmlInputCheckBox())
                {
                    wBox.Value = "1";
                    wBox.ID = "aw";
                    wBox.Checked = this.azContext.WealdenSelected;

                    using (var wLabel = new HtmlGenericControl("label"))
                    {
                        wLabel.Attributes.Add("class", "checkboxAlt2");
                        wLabel.Controls.Add(wBox);
                        wLabel.Attributes.Add("for", this.Parent.UniqueID.Replace("$", "_") + "_" + wBox.ID);
                        wLabel.Controls.Add(new LiteralControl(Properties.Resources.CouncilNameWealden));
                        councilBoxes.Controls.Add(wLabel);
                    }
                }
            }
        }
    }
}
