﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 1.1.4322.2032
//
//     Changes to this file may cause incorrect behavior and will be lost if 
//     the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by wsdl, Version=1.1.4322.2032.
// 

using System.Diagnostics;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Services;

namespace Escc.AZ.Importer {
    /// <remarks/>
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [WebServiceBinding(Name="CouncilServicesSoap", Namespace="http://www.netescape.co.uk/webservices/")]
    public class CouncilServices : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        /// <remarks/>
        public CouncilServices() {
            this.Url = "https://www.accesseastsussex.org/webservices/councilservices.asmx";
        }
        
        /// <remarks/>
        [SoapDocumentMethod("http://www.netescape.co.uk/webservices/getServices", RequestNamespace="http://www.netescape.co.uk/webservices/", ResponseNamespace="http://www.netescape.co.uk/webservices/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode getServices() {
            object[] results = this.Invoke("getServices", new object[0]);
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetServices(System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getServices", new object[0], callback, asyncState);
        }
        
        /// <remarks/>
        public System.Xml.XmlNode EndgetServices(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        /// <remarks/>
        [SoapDocumentMethod("http://www.netescape.co.uk/webservices/getServiceByID", RequestNamespace="http://www.netescape.co.uk/webservices/", ResponseNamespace="http://www.netescape.co.uk/webservices/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode getServiceByID(int id) {
            object[] results = this.Invoke("getServiceByID", new object[] {
                        id});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetServiceByID(int id, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getServiceByID", new object[] {
                        id}, callback, asyncState);
        }
        
        /// <remarks/>
        public System.Xml.XmlNode EndgetServiceByID(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        /// <remarks/>
        [SoapDocumentMethod("http://www.netescape.co.uk/webservices/getServicesByArea", RequestNamespace="http://www.netescape.co.uk/webservices/", ResponseNamespace="http://www.netescape.co.uk/webservices/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode getServicesByArea(string area) {
            object[] results = this.Invoke("getServicesByArea", new object[] {
                        area});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetServicesByArea(string area, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getServicesByArea", new object[] {
                        area}, callback, asyncState);
        }
        
        /// <remarks/>
        public System.Xml.XmlNode EndgetServicesByArea(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        /// <remarks/>
        [SoapDocumentMethod("http://www.netescape.co.uk/webservices/getServicesByCategory", RequestNamespace="http://www.netescape.co.uk/webservices/", ResponseNamespace="http://www.netescape.co.uk/webservices/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode getServicesByCategory(string category) {
            object[] results = this.Invoke("getServicesByCategory", new object[] {
                        category});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetServicesByCategory(string category, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getServicesByCategory", new object[] {
                        category}, callback, asyncState);
        }
        
        /// <remarks/>
        public System.Xml.XmlNode EndgetServicesByCategory(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        /// <remarks/>
        [SoapDocumentMethod("http://www.netescape.co.uk/webservices/getServicesByCategoryAndArea", RequestNamespace="http://www.netescape.co.uk/webservices/", ResponseNamespace="http://www.netescape.co.uk/webservices/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode getServicesByCategoryAndArea(string category, string area) {
            object[] results = this.Invoke("getServicesByCategoryAndArea", new object[] {
                        category,
                        area});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetServicesByCategoryAndArea(string category, string area, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getServicesByCategoryAndArea", new object[] {
                        category,
                        area}, callback, asyncState);
        }
        
        /// <remarks/>
        public System.Xml.XmlNode EndgetServicesByCategoryAndArea(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        /// <remarks/>
        [SoapDocumentMethod("http://www.netescape.co.uk/webservices/getServicesByLetter", RequestNamespace="http://www.netescape.co.uk/webservices/", ResponseNamespace="http://www.netescape.co.uk/webservices/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode getServicesByLetter(string letter) {
            object[] results = this.Invoke("getServicesByLetter", new object[] {
                        letter});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetServicesByLetter(string letter, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getServicesByLetter", new object[] {
                        letter}, callback, asyncState);
        }
        
        /// <remarks/>
        public System.Xml.XmlNode EndgetServicesByLetter(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        /// <remarks/>
        [SoapDocumentMethod("http://www.netescape.co.uk/webservices/getServicesByLetterAndArea", RequestNamespace="http://www.netescape.co.uk/webservices/", ResponseNamespace="http://www.netescape.co.uk/webservices/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode getServicesByLetterAndArea(string letter, string area) {
            object[] results = this.Invoke("getServicesByLetterAndArea", new object[] {
                        letter,
                        area});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetServicesByLetterAndArea(string letter, string area, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getServicesByLetterAndArea", new object[] {
                        letter,
                        area}, callback, asyncState);
        }
        
        /// <remarks/>
        public System.Xml.XmlNode EndgetServicesByLetterAndArea(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        /// <remarks/>
        [SoapDocumentMethod("http://www.netescape.co.uk/webservices/getServices_DataSet", RequestNamespace="http://www.netescape.co.uk/webservices/", ResponseNamespace="http://www.netescape.co.uk/webservices/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Data.DataSet getServices_DataSet() {
            object[] results = this.Invoke("getServices_DataSet", new object[0]);
            return ((System.Data.DataSet)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetServices_DataSet(System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getServices_DataSet", new object[0], callback, asyncState);
        }
        
        /// <remarks/>
        public System.Data.DataSet EndgetServices_DataSet(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((System.Data.DataSet)(results[0]));
        }
        
        /// <remarks/>
        [SoapDocumentMethod("http://www.netescape.co.uk/webservices/getServiceByID_DataSet", RequestNamespace="http://www.netescape.co.uk/webservices/", ResponseNamespace="http://www.netescape.co.uk/webservices/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Data.DataSet getServiceByID_DataSet(int id) {
            object[] results = this.Invoke("getServiceByID_DataSet", new object[] {
                        id});
            return ((System.Data.DataSet)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetServiceByID_DataSet(int id, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getServiceByID_DataSet", new object[] {
                        id}, callback, asyncState);
        }
        
        /// <remarks/>
        public System.Data.DataSet EndgetServiceByID_DataSet(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((System.Data.DataSet)(results[0]));
        }
        
        /// <remarks/>
        [SoapDocumentMethod("http://www.netescape.co.uk/webservices/getServicesByArea_DataSet", RequestNamespace="http://www.netescape.co.uk/webservices/", ResponseNamespace="http://www.netescape.co.uk/webservices/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Data.DataSet getServicesByArea_DataSet(string area) {
            object[] results = this.Invoke("getServicesByArea_DataSet", new object[] {
                        area});
            return ((System.Data.DataSet)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetServicesByArea_DataSet(string area, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getServicesByArea_DataSet", new object[] {
                        area}, callback, asyncState);
        }
        
        /// <remarks/>
        public System.Data.DataSet EndgetServicesByArea_DataSet(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((System.Data.DataSet)(results[0]));
        }
        
        /// <remarks/>
        [SoapDocumentMethod("http://www.netescape.co.uk/webservices/getServicesByCategory_DataSet", RequestNamespace="http://www.netescape.co.uk/webservices/", ResponseNamespace="http://www.netescape.co.uk/webservices/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Data.DataSet getServicesByCategory_DataSet(string category) {
            object[] results = this.Invoke("getServicesByCategory_DataSet", new object[] {
                        category});
            return ((System.Data.DataSet)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetServicesByCategory_DataSet(string category, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getServicesByCategory_DataSet", new object[] {
                        category}, callback, asyncState);
        }
        
        /// <remarks/>
        public System.Data.DataSet EndgetServicesByCategory_DataSet(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((System.Data.DataSet)(results[0]));
        }
        
        /// <remarks/>
        [SoapDocumentMethod("http://www.netescape.co.uk/webservices/getServicesByCategoryAndArea_DataSet", RequestNamespace="http://www.netescape.co.uk/webservices/", ResponseNamespace="http://www.netescape.co.uk/webservices/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Data.DataSet getServicesByCategoryAndArea_DataSet(string category, string area) {
            object[] results = this.Invoke("getServicesByCategoryAndArea_DataSet", new object[] {
                        category,
                        area});
            return ((System.Data.DataSet)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetServicesByCategoryAndArea_DataSet(string category, string area, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getServicesByCategoryAndArea_DataSet", new object[] {
                        category,
                        area}, callback, asyncState);
        }
        
        /// <remarks/>
        public System.Data.DataSet EndgetServicesByCategoryAndArea_DataSet(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((System.Data.DataSet)(results[0]));
        }
        
        /// <remarks/>
        [SoapDocumentMethod("http://www.netescape.co.uk/webservices/getServicesByLetter_DataSet", RequestNamespace="http://www.netescape.co.uk/webservices/", ResponseNamespace="http://www.netescape.co.uk/webservices/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Data.DataSet getServicesByLetter_DataSet(string letter) {
            object[] results = this.Invoke("getServicesByLetter_DataSet", new object[] {
                        letter});
            return ((System.Data.DataSet)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetServicesByLetter_DataSet(string letter, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getServicesByLetter_DataSet", new object[] {
                        letter}, callback, asyncState);
        }
        
        /// <remarks/>
        public System.Data.DataSet EndgetServicesByLetter_DataSet(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((System.Data.DataSet)(results[0]));
        }
        
        /// <remarks/>
        [SoapDocumentMethod("http://www.netescape.co.uk/webservices/getServicesByLetterAndArea_DataSet", RequestNamespace="http://www.netescape.co.uk/webservices/", ResponseNamespace="http://www.netescape.co.uk/webservices/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Data.DataSet getServicesByLetterAndArea_DataSet(string letter, string area) {
            object[] results = this.Invoke("getServicesByLetterAndArea_DataSet", new object[] {
                        letter,
                        area});
            return ((System.Data.DataSet)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetServicesByLetterAndArea_DataSet(string letter, string area, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getServicesByLetterAndArea_DataSet", new object[] {
                        letter,
                        area}, callback, asyncState);
        }
        
        /// <remarks/>
        public System.Data.DataSet EndgetServicesByLetterAndArea_DataSet(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((System.Data.DataSet)(results[0]));
        }
    }
}
