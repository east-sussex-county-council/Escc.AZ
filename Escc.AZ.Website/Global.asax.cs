using System;
using System.Globalization;
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
            ConvertToHiddenQueryString(HttpContext.Current.Request.ApplicationPath + "/heading", HttpContext.Current.Request.ApplicationPath + "/services.aspx?heading=");
        }

        /// <summary>
        ///  Convert a path of the form page1.aspx into a path of the form  page.aspx?id=1
        /// </summary>
        /// <param name="lookFor">Unique section of the path to replace, up to but not including the id</param>
        /// <param name="replaceWith">Path to replace with, up to = sign of query string parameter</param>
        /// <example>
        /// To convert /councillors/find/councillor10.aspx into /councillors/find/councillor.aspx?councillor=10
        /// <code>
        /// ConvertToHiddenQuerystring("/councillors/find/councillor", "/councillors/find/councillor.aspx?councillor=")
        /// </code>
        /// </example>
        private static void ConvertToHiddenQueryString(string lookFor, string replaceWith)
        {
            HttpContext context = HttpContext.Current;
            string oldpath = context.Request.Path.ToLower(CultureInfo.CurrentCulture);

            int i = oldpath.IndexOf(lookFor);
            int len = lookFor.Length;

            if (i != -1)
            {
                int j = oldpath.IndexOf(".aspx");
                if (j != -1 && j > (i + len))
                {
                    string id = oldpath.Substring(i + len, j - (i + len));

                    string newpath = oldpath.Replace(lookFor + id + ".aspx", replaceWith + id);
                    string qs = context.Request.QueryString.ToString();
                    if (qs.Length > 0) newpath = newpath + "&" + qs;
                    context.RewritePath(newpath);
                }
            }
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