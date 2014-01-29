using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;

namespace Escc.AZ
{
    /// <summary>
    /// Metadata about the current request in the A-Z of services
    /// </summary>
    public sealed class AZContext
    {
        AZMode mode;
        private bool noneSelected;
        private bool esccSelected;
        private bool eastbourneSelected;
        private bool hastingsSelected;
        private bool lewesSelected;
        private bool rotherSelected;
        private bool wealdenSelected;
        private int councilCount;
        private string selectedChar;

        /// <summary>
        /// Metadata about the current request in the A-Z of services
        /// </summary>
        public AZContext()
        {
            // determine the current context based on the url
            this.mode = (HttpContext.Current.Request.Path.IndexOf("managewebsite", StringComparison.OrdinalIgnoreCase) > -1) ? AZMode.Edit : AZMode.Published;

            // check which councils (if any) have been selected in the search form
            CheckWhichCouncilsAreSelected();

            // count how many councils selected
            CountSelectedCouncils();

            // Get the selected A-Z character from the querystring, defaulting to "a" if not selected
            NameValueCollection qs = HttpContext.Current.Request.QueryString;
            this.selectedChar = (qs["index"] != null && qs["index"].Trim().Length == 1) ? qs["index"].Trim().ToLower(CultureInfo.CurrentCulture) : "a";
        }

        private void CountSelectedCouncils()
        {
            this.councilCount = 0;
            if (this.EsccSelected) this.councilCount++;
            if (this.EastbourneSelected) this.councilCount++;
            if (this.LewesSelected) this.councilCount++;
            if (this.WealdenSelected) this.councilCount++;
            if (this.HastingsSelected) this.councilCount++;
            if (this.RotherSelected) this.councilCount++;
            if (this.councilCount == 0) this.councilCount = 6;
        }

        private void CheckWhichCouncilsAreSelected()
        {
            NameValueCollection qs = HttpContext.Current.Request.QueryString;

            this.noneSelected = (qs["acc"] == null && qs["ae"] == null && qs["ah"] == null && qs["al"] == null && qs["ar"] == null && qs["aw"] == null);
            this.esccSelected = ((qs["acc"] != null && qs["acc"] == "1") || this.noneSelected);
            this.eastbourneSelected = ((qs["ae"] != null && qs["ae"] == "1") || this.noneSelected);
            this.hastingsSelected = ((qs["ah"] != null && qs["ah"] == "1") || this.noneSelected);
            this.lewesSelected = ((qs["al"] != null && qs["al"] == "1") || this.noneSelected);
            this.rotherSelected = ((qs["ar"] != null && qs["ar"] == "1") || this.noneSelected);
            this.wealdenSelected = ((qs["aw"] != null && qs["aw"] == "1") || this.noneSelected);
        }

        /// <summary>
        /// Get the current A-Z context
        /// </summary>
        public static AZContext Current
        {
            get
            {
                return new AZContext();
            }
        }

        /// <summary>
        /// Is the A-Z currently being edited?
        /// </summary>
        public AZMode Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                this.mode = value;
            }
        }

        /// <summary>
        /// Are data about ESCC's services required?
        /// </summary>
        public bool EsccSelected
        {
            get { return this.esccSelected; }
        }

        /// <summary>
        /// Are data about Eastbourne Borough Council's services required?
        /// </summary>
        public bool EastbourneSelected
        {
            get { return this.eastbourneSelected; }
        }

        /// <summary>
        /// Are data about Hastings Borough Council's services required?
        /// </summary>
        public bool HastingsSelected
        {
            get { return this.hastingsSelected; }
        }

        /// <summary>
        /// Are data about Lewes District Council's services required?
        /// </summary>
        public bool LewesSelected
        {
            get { return this.lewesSelected; }
        }

        /// <summary>
        /// Are data about Rother District Council's services required?
        /// </summary>
        public bool RotherSelected
        {
            get { return this.rotherSelected; }
        }

        /// <summary>
        /// Are data about Wealden District Council's services required?
        /// </summary>
        public bool WealdenSelected
        {
            get { return this.wealdenSelected; }
        }

        /// <summary>
        /// Have no specific councils' services been requested? (meaning we should assume all councils)
        /// </summary>
        public bool NoneSelected
        {
            get { return this.noneSelected; }
        }

        /// <summary>
        /// How many councils in total have been selected?
        /// </summary>
        public int SelectedCouncils
        {
            get { return this.councilCount; }
        }

        /// <summary>
        /// Get the selected A-Z character from the querystring
        /// </summary>
        public string SelectedChar
        {
            get { return this.selectedChar; }
        }
    }
}
