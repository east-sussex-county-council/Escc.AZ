using System.Collections;
using System.Collections.Generic;
using Escc.Web.Metadata;

namespace Escc.AZ
{
    /// <summary>
    /// Business object representing a service in the A-Z
    /// </summary>
    public class AZService
    {
        #region Private fields
        private int id;
        private string service = "";
        private string description = "";
        private ArrayList contacts = new ArrayList();
        private AZUrlCollection urls = new AZUrlCollection();
        private Authority authority;
        private string keywords = "";
        private EsdTermCollection ipsvPreferredTerms = new EsdTermCollection();
        private IList<AZHeading> headings = new List<AZHeading>();
        private bool ipsv;

        /// <summary>
        /// Gets or sets a value indicating whether the name of this <see cref="AZService"/> is an IPSV preferred term.
        /// </summary>
        /// <value><c>true</c> if service name is an IPSV preferred term; otherwise, <c>false</c>.</value>
        public bool Ipsv
        {
            get
            {
                return this.ipsv;
            }
            set
            {
                this.ipsv = value;
            }
        }


        #endregion // Private fields

        #region Public Properties

        /// <summary>
        /// Gets or sets the headings this service is categorised under
        /// </summary>
        public IList<AZHeading> Headings
        {
            get
            {
                return this.headings;
            }
            set
            {
                this.headings = value;
            }
        }

        /// <summary>
        /// Property to store autonumber from db - used as unique identifier for service
        /// </summary>
        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        /// <summary>
        /// Gets or sets the IPSV preferred terms for the service
        /// </summary>
        /// <remarks>IPSV preferred terms belong to a heading in the A-Z database, 
        /// but they apply to each service which is associated with that heading so 
        /// they're represented in the AZService object too.</remarks>
        public EsdTermCollection IpsvPreferredTerms
        {
            get
            {
                return this.ipsvPreferredTerms;
            }
            set
            {
                this.ipsvPreferredTerms = value;
            }
        }

        /// <summary>
        /// Gets or sets a semi-colon separated list of free-text keywords
        /// </summary>
        public string Keywords
        {
            get
            {
                return this.keywords;
            }
            set
            {
                this.keywords = value;
            }
        }

        /// <summary>
        /// Property to store the title of the service
        /// </summary>
        public string Service
        {
            get { return this.service; }
            set { this.service = value; }
        }

        /// <summary>
        /// Property to store a description of the service (max 60 words)
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        /// <summary>
        /// Add an AZContact to the service. Method encapsulates internal format.
        /// </summary>
        /// <param name="contact">Completed AZContact object</param>
        public void AddContact(AZContact contact)
        {
            this.contacts.Add(contact);
        }

        /// <summary>
        /// Read only collection to store AZContacts related to this AZService. 
        /// </summary>
        public IList Contacts
        {
            get { return this.contacts as IList; }
        }


        #endregion // Public Properties

        /// <summary>
        /// Read only access to collection of links.
        /// </summary>
        public AZUrlCollection Urls
        {
            get { return this.urls; }
        }

        /// <summary>
        /// Enum property to store which council is responsible for this AZService
        /// </summary>
        public Authority Authority
        {
            get { return this.authority; }
            set { this.authority = value; }
        }
    }
}
