using System;
using Escc.AddressAndPersonalDetails;

namespace Escc.AZ
{
    /// <summary>
    /// Business object representing a contact in the A-Z
    /// </summary>
    public class AZContact
    {
        #region Private fields
        /// <summary>
        /// Property to store autonumber from db - used as unique identifier in code
        /// </summary>
        private int id;

        /// <summary>
        /// Property to store service id
        /// </summary>
        private int serviceId;

        /// <summary>
        /// Property to store (optional) first name of contact
        /// </summary>
        private string firstName = "";

        /// <summary>
        /// Property to store (optional) last name of contact
        /// </summary>
        private string lastName = "";

        private string description = "";

        /// <summary>
        /// Property to store (optional) phone area code
        /// </summary>
        private string phoneArea = "";

        /// <summary>
        /// Property to store (optional) telephone number
        /// </summary>
        private string phone = "";

        /// <summary>
        /// Property to store (optional) telephone number extension
        /// </summary>
        private string phoneExtension = "";

        /// <summary>
        /// Property to store (optional) email address
        /// </summary>
        private string email = "";
        private string emailText = "";

        /// <summary>
        /// Property to store (optional) fax area code
        /// </summary>
        private string faxArea = "";

        /// <summary>
        /// Property to store (optional) fax number
        /// </summary>
        private string fax = "";

        /// <summary>
        /// Property to store (optional) fax number extension
        /// </summary>
        private string faxExtension = "";

        private BS7666Address bS7666address;

        private AZUrl addressUrl;

        #endregion // Private fields

        /// <summary>
        /// All properties are optional, so no required arguments for instantiation
        /// </summary>
        public AZContact() { }

        #region Address properties
        /// <summary>
        /// Gets or sets the URL to link to instead of displaying an address
        /// </summary>
        public AZUrl AddressUrl
        {
            get
            {
                return this.addressUrl;
            }
            set
            {
                this.addressUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets an address in BS7666 format
        /// </summary>
        public BS7666Address BS7666Address
        {
            get
            {
                return this.bS7666address;
            }
            set
            {
                this.bS7666address = value;
            }
        }
        #endregion // Address properties

        /// <summary>
        /// Property to store autonumber from db - used as unique identifier in code
        /// </summary>
        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        /// <summary>
        /// Property to store id of service to which this contact belongs
        /// </summary>
        public int ServiceId
        {
            get { return this.serviceId; }
            set { this.serviceId = value; }
        }

        /// <summary>
        /// Property to store (optional) first name of contact
        /// </summary>
        public string FirstName
        {
            get { return this.firstName; }
            set { this.firstName = value; }
        }

        /// <summary>
        /// Property to store (optional) last name of contact
        /// </summary>
        public string LastName
        {
            get { return this.lastName; }
            set { this.lastName = value; }
        }

        /// <summary>
        /// Get joined-up first name and last name
        /// </summary>
        public string Name
        {
            get
            {
                string name = this.firstName + " " + this.lastName;
                return name.Trim();
            }
        }

        /// <summary>
        /// Gets or sets a description of the contact
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        /// <summary>
        /// Get a title for the contact depending on what's available
        /// </summary>
        public string ContactText
        {
            get
            {
                string text = this.Name;
                if (text.Length == 0) text = this.emailText;
                if (text.Length == 0)
                {
                    int linePos = this.Description.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                    text = (linePos > -1) ? this.description.Substring(0, linePos) : this.description;
                }

                return text;
            }
        }


        /// <summary>
        /// Property to store (optional) telephone number
        /// </summary>
        public string Phone
        {
            get { return this.phone; }
            set { this.phone = value; }
        }

        /// <summary>
        /// Property to store (optional) telephone area code
        /// </summary>
        public string PhoneArea
        {
            get { return this.phoneArea; }
            set { this.phoneArea = value; }
        }

        /// <summary>
        /// Property to store (optional) telephone number extension
        /// </summary>
        public string PhoneExtension
        {
            get { return this.phoneExtension; }
            set { this.phoneExtension = value; }
        }

        /// <summary>
        /// Property to get combined parts of phone number
        /// </summary>
        public string PhoneText
        {
            get
            {
                string phoneText = (this.phoneArea + " " + this.phone);
                phoneText = phoneText.Trim();
                if (this.phoneExtension != null && this.phoneExtension.Length > 0) phoneText += " ext " + this.phoneExtension;
                return phoneText;
            }
        }

        /// <summary>
        /// Property to store (optional) email address
        /// </summary>
        public string Email
        {
            get { return this.email; }
            set { this.email = value; }
        }

        /// <summary>
        /// Gets or sets the clickable text for a linked email address
        /// </summary>
        public string EmailText
        {
            get
            {
                if (this.emailText.Length > 0) return this.emailText;
                else return this.Name;
            }
            set { this.emailText = value; }
        }

        /// <summary>
        /// Get account part of email address
        /// </summary>
        public string EmailAccount
        {
            get
            {
                int atPos = this.Email.IndexOf("@", StringComparison.Ordinal);
                return (this.Email != null && atPos > 0) ? this.Email.Substring(0, atPos) : null;
            }

        }

        /// <summary>
        /// Get domain part of email address
        /// </summary>
        public string EmailDomain
        {
            get
            {
                return (this.Email != null) ? this.Email.Substring(this.Email.IndexOf("@", StringComparison.Ordinal) + 1) : null;
            }

        }

        /// <summary>
        /// Property to store (optional) fax area code
        /// </summary>
        public string FaxArea
        {
            get { return this.faxArea; }
            set { this.faxArea = value; }
        }

        /// <summary>
        /// Property to store (optional) fax number
        /// </summary>
        public string Fax
        {
            get { return this.fax; }
            set { this.fax = value; }
        }

        /// <summary>
        /// Property to store (optional) fax number extension
        /// </summary>
        public string FaxExtension
        {
            get { return this.faxExtension; }
            set { this.faxExtension = value; }
        }

        /// <summary>
        /// Property to get combined parts of fax number
        /// </summary>
        public string FaxText
        {
            get
            {
                string faxText = (this.faxArea + " " + this.fax);
                faxText = faxText.Trim();
                if (this.faxExtension != null && this.faxExtension.Length > 0) faxText += " ext " + this.faxExtension;
                return faxText;
            }
        }
    }
}
