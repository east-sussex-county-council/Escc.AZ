using System;
using System.Collections;
using Escc.Web.Metadata;

namespace Escc.AZ
{
    /// <summary>
    /// Business object representing a heading in the A-Z
    /// </summary>
    public class AZHeading
    {
        #region Fields
        /// <summary>
        /// Store autonumber from db - used as unique identifier in code
        /// </summary>
        private int id;

        /// <summary>
        /// Store the title of the heading
        /// </summary>
        private string heading = "";

        /// <summary>
        /// Store how many ESCC services are assigned to the heading
        /// </summary>
        private int serviceCountEscc;

        /// <summary>
        /// Store how many Eastbourne Borough Council services are assigned to the heading
        /// </summary>
        private int serviceCountEastbourne;

        /// <summary>
        /// Store how many Hastings Borough Council services are assigned to the heading
        /// </summary>
        private int serviceCountHastings;

        /// <summary>
        /// Store how many Lewes District Council services are assigned to the heading
        /// </summary>
        private int serviceCountLewes;

        /// <summary>
        /// Store how many Rother District Council services are assigned to the heading
        /// </summary>
        private int serviceCountRother;

        /// <summary>
        /// Store how many Wealden District Council services are assigned to the heading
        /// </summary>
        private int serviceCountWealden;

        /// <summary>
        /// Store (optional) url to redirect heading to
        /// </summary>
        private Uri redirectUrl;

        /// <summary>
        /// Store link title if RedirectUrl property is used
        /// </summary>
        private string redirectTitle;

        /// <summary>
        /// Store AZHeading objects related to this AZHeading object. ArrayList used to allow dynamic size.
        /// </summary>
        private ArrayList relatedHeadings = new ArrayList();

        /// <summary>
        /// Store AZService objects related to this AZHeading object. ArrayList used to allow dynamic size.
        /// </summary>
        private ArrayList services = new ArrayList();
        private EsdTermCollection ipsvPreferredTerms = new EsdTermCollection();
        private string ipsvNonPreferredTerms;
        private bool ipsv;

        #endregion

        #region Constructors
        /// <summary>
        /// Constructor - no arguments required
        /// </summary>
        public AZHeading() { }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AZHeading"/> is an IPSV term.
        /// </summary>
        /// <value><c>true</c> if IPSV; otherwise, <c>false</c>.</value>
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

        /// <summary>
        /// Gets or sets a semi-colon separated list of IPSV non-preferred terms
        /// </summary>
        /// <remarks>
        /// Unlike the IPSV preferred terms, which are used for importing/exporting data, 
        /// this property is only ever used as a string of metadata, which is why it is
        /// represented as a lightweight string rather than an EsdTermCollection
        /// </remarks>
        public string IpsvNonPreferredTerms
        {
            get
            {
                return this.ipsvNonPreferredTerms;
            }
            set
            {
                this.ipsvNonPreferredTerms = value;
            }
        }

        /// <summary>
        /// Gets or sets the IPSV preferred terms for the heading
        /// </summary>
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
        /// Property to store autonumber from db - used as unique identifier in code
        /// </summary>
        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        /// <summary>
        /// Property to store the title of the heading
        /// </summary>
        public string Heading
        {
            get { return this.heading; }
            set { this.heading = value; }
        }

        /// <summary>
        /// Gets or sets how many ESCC services are assigned to the heading
        /// </summary>
        public int ServiceCountEscc
        {
            get { return this.serviceCountEscc; }
            set { this.serviceCountEscc = value; }
        }

        /// <summary>
        /// Gets or sets how many Eastbourne Borough Council services are assigned to the heading
        /// </summary>
        public int ServiceCountEastbourne
        {
            get { return this.serviceCountEastbourne; }
            set { this.serviceCountEastbourne = value; }
        }

        /// <summary>
        /// Gets or sets how many Hastings Borough Council services are assigned to the heading
        /// </summary>
        public int ServiceCountHastings
        {
            get { return this.serviceCountHastings; }
            set { this.serviceCountHastings = value; }
        }

        /// <summary>
        /// Gets or sets how many Lewes District Council services are assigned to the heading
        /// </summary>
        public int ServiceCountLewes
        {
            get { return this.serviceCountLewes; }
            set { this.serviceCountLewes = value; }
        }

        /// <summary>
        /// Gets or sets how many Rother District Council services are assigned to the heading
        /// </summary>
        public int ServiceCountRother
        {
            get { return this.serviceCountRother; }
            set { this.serviceCountRother = value; }
        }

        /// <summary>
        /// Gets or sets how many Wealden District Council services are assigned to the heading
        /// </summary>
        public int ServiceCountWealden
        {
            get { return this.serviceCountWealden; }
            set { this.serviceCountWealden = value; }
        }

        /// <summary>
        /// Gets or sets (optional) url to redirect heading to
        /// </summary>
        public Uri RedirectUrl
        {
            get { return this.redirectUrl; }
            set { this.redirectUrl = value; }
        }

        /// <summary>
        /// Gets or sets link title if RedirectUrl property is used
        /// </summary>
        public string RedirectTitle
        {
            get { return this.redirectTitle; }
            set { this.redirectTitle = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets ServiceCount for selected councils
        /// </summary>
        /// <param name="escc">Is East Sussex County Council selected?</param>
        /// <param name="eastbourne">Is Eastbourne Borough Council selected?</param>
        /// <param name="hastings">Is Hastings Borough Council selected?</param>
        /// <param name="lewes">Is Lewes District Council selected?</param>
        /// <param name="rother">Is Rother District Council selected?</param>
        /// <param name="wealden">Is Weladen District Council selected?</param>
        /// <returns>Total services under this heading for the selected councils</returns>
        public int GetServiceCount(bool escc, bool eastbourne, bool hastings, bool lewes, bool rother, bool wealden)
        {
            int count = 0;

            if (escc) count += this.serviceCountEscc;
            if (eastbourne) count += this.serviceCountEastbourne;
            if (hastings) count += this.serviceCountHastings;
            if (lewes) count += this.serviceCountLewes;
            if (rother) count += this.serviceCountRother;
            if (wealden) count += this.serviceCountWealden;

            return count;
        }

        /// <summary>
        /// Add AZHeading object related to this AZHeading object. Encapsulates internal format of collection.
        /// </summary>
        /// <param name="relatedHeading">A related AZHeading object</param>
        public void AddRelatedHeading(AZHeading relatedHeading)
        {
            this.relatedHeadings.Add(relatedHeading);
        }

        /// <summary>
        /// Read only access to collection of AZHeading objects related to this AZHeading object.
        /// </summary>
        public IList RelatedHeadings
        {
            get { return this.relatedHeadings as IList; }
        }

        /// <summary>
        /// Add AZService object related to this AZHeading object. Encapsulates internal format of collection.
        /// </summary>
        /// <param name="service">A related AZService object</param>
        public void AddService(AZService service)
        {
            if (service == null) throw new ArgumentNullException("service");

            // Add this heading's preferred terms to the service
            if (this.ipsvPreferredTerms.Count > 0)
            {
                foreach (EsdTerm term in this.ipsvPreferredTerms)
                {
                    if (!service.IpsvPreferredTerms.Contains(term))
                    {
                        service.IpsvPreferredTerms.Add(term);
                    }
                }
            }

            // Add the service
            this.services.Add(service);
        }

        /// <summary>
        /// Read only access to collection of AZService objects related to this AZHeading object. 
        /// </summary>
        public IList Services
        {
            get { return this.services as IList; }
        }

        #endregion
    }
}
