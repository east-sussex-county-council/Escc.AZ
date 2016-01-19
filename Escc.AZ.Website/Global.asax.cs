using System;
using eastsussexgovuk.webservices.TextXhtml.HouseStyle;
using System.Web;

namespace Escc.AZ.Website
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {

        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            UriFormatter.ConvertToHiddenQueryString(HttpContext.Current.Request.ApplicationPath + "heading", HttpContext.Current.Request.ApplicationPath + "services.aspx?heading=");
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}