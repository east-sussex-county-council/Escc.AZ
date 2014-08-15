using System.Collections;
using System.Web.UI.HtmlControls;
using Escc.AddressAndPersonalDetails.Controls;

namespace Escc.AZ
{
    /// <summary>
    /// Display of contact information in the website A-Z
    /// </summary>
    public class AZContactGrid : HtmlGenericControl
    {
        private IList contacts;
        private AZContext azContext;

        /// <summary>
        /// Create a new AZContactGrid control
        /// </summary>
        /// <param name="contacts">An IList of AZContact objects</param>
        public AZContactGrid(IList contacts)
        {
            // store contacts
            this.contacts = contacts;
            this.azContext = AZContext.Current;
        }

        /// <summary>
        /// Create a new AZContactGrid control
        /// </summary>
        /// <param name="contacts">An IList of AZContact objects</param>
        /// <param name="con">Context which influences the display of the contacts grid</param>
        public AZContactGrid(IList contacts, AZContext con)
        {
            // store contacts
            this.contacts = contacts;
            this.azContext = con;
        }

        /// <summary>
        /// Override create child controls to build the display of the contacts, then write it to the page
        /// </summary>
        protected override void CreateChildControls()
        {
            // tbody tag used if we're in edit mode
            using (var tbody = new HtmlGenericControl("tbody"))
            {
                foreach (AZContact contact in this.contacts)
                {
                    if (contact != null)
                    {
                        using (var info = new ContactInfoControl())
                        {
                            info.Name = contact.Name;
                            info.Description = contact.Description;
                            info.BS7666Address = contact.BS7666Address;
                            info.Telephone = contact.PhoneText;
                            info.Fax = contact.FaxText;
                            info.EmailAddress = contact.Email;
                            info.EmailText = contact.EmailText;
                            info.UseEmailForm = true;

                            if (contact.AddressUrl != null)
                            {
                                info.AddressUrl = contact.AddressUrl.Url;
                                info.AddressUrlText = contact.AddressUrl.Text;
                            }

                            // display differently in published and edit mode
                            if (this.azContext.Mode == AZMode.Published)
                            {
                                this.Controls.Add(info);
                            }
                            else
                            {
                                // new table row for contact
                                using (var row = new HtmlTableRow())
                                {
                                    tbody.Controls.Add(row);

                                    // add the address
                                    using (var addressCell = new HtmlTableCell("th"))
                                    {
                                        addressCell.Controls.Add(info);
                                        row.Controls.Add(addressCell);
                                    }

                                    if (this.azContext.Mode == AZMode.Edit)
                                    {
                                        // add edit link
                                        using (var editLink = new HtmlAnchor())
                                        {
                                            editLink.InnerText = "Edit";
                                            editLink.HRef = "contact.aspx?contact=" + contact.Id;
                                            using (var editCell = new HtmlTableCell())
                                            {
                                                editCell.Attributes.Add("class", "action");
                                                editCell.Controls.Add(editLink);
                                                row.Controls.Add(editCell);
                                            }
                                        }

                                        // add delete link
                                        using (var deleteLink = new HtmlAnchor())
                                        {
                                            deleteLink.InnerText = "Delete";
                                            deleteLink.HRef = "service.aspx?service=" + contact.ServiceId + "&amp;deletecontact=" + contact.Id;
                                            using (var delCell = new HtmlTableCell())
                                            {
                                                delCell.Attributes.Add("class", "action");
                                                delCell.Controls.Add(deleteLink);
                                                row.Controls.Add(delCell);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }

                // display differently in published and edit mode
                if (this.azContext.Mode == AZMode.Published)
                {
                    this.TagName = "div";

                    /* Commented out because Technorati service currently down. Labelled as beta, so not sure if it's a temporary glitch.
                     * 
                    if (this.contacts.Count > 0)
                    {
                        HtmlAnchor microformatLink = new HtmlAnchor();
                        microformatLink.Attributes["class"] = "hCard";
                        microformatLink.HRef = "http://feeds.technorati.com/contact/" + this.Page.Request.Url.ToString() + "&amp;service=" + serviceId;
                        if (this.contacts.Count > 1)
                        {
                            microformatLink.InnerText = "Add these contacts to your address book";
                        }
                        else
                        {
                            microformatLink.InnerText = "Add this contact to your address book";
                        }
                        this.Controls.Add(microformatLink);

                    }*/
                }
                else
                {
                    // display differently if no contacts
                    if (this.contacts.Count > 0)
                    {
                        // build up a table for all contacts
                        this.TagName = "table";
                        this.Attributes.Add("class", "itemManager");

                        using (var caption = new HtmlGenericControl("caption"))
                        {
                            caption.InnerText = "Current contacts";
                            this.Controls.Add(caption);
                        }

                        this.Controls.Add(tbody);
                    }
                    else
                    {
                        this.TagName = "p";
                        this.InnerText = "This service has no contacts";
                    }
                }
            }
        }
    }
}
