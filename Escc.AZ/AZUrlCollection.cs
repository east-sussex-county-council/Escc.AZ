using System;
using System.Collections.ObjectModel;
using System.Web;
using EsccWebTeam.Data.Web;

namespace Escc.AZ
{
    /// <summary>
    /// A collection of URLs in an A-Z service
    /// </summary>
    public class AZUrlCollection : Collection<AZUrl>
    {
        /// <summary>
        /// Add a URL to the collection
        /// </summary>
        /// <param name="url">The complete URL of the link's target resource</param>
        /// <param name="title">Optional link text - the URL will be used if null or empty string is passed</param>
        /// <param name="description">Optional description of the link</param>
        public void Add(Uri url, string title, string description)
        {
            if (url == null) throw new ArgumentNullException("url");

            // Tidy up URL
            string validUrl = url.ToString();
            if (HttpContext.Current != null)
            {
                validUrl = HttpContext.Current.Server.HtmlEncode(validUrl);
            }
            validUrl = validUrl.Replace(" ", "+");

            // Create AZUrl object
            AZUrl link = new AZUrl();
            link.Url = new Uri(validUrl);

            if (title != null && title.Length > 0)
            {
                link.Text = title;
            }
            else
            {
                link.Text = Iri.ShortenForDisplay(link.Url);
            }

            link.Description = description;

            this.Add(link);
        }
    }
}
