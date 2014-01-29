using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Schema;
using eastsussexgovuk.webservices.EgmsWebMetadata;
using EsccWebTeam.Gdsc;
using Microsoft.ApplicationBlocks.Data;
using Microsoft.ApplicationBlocks.ExceptionManagement;

namespace Escc.AZ.Importer
{
    /// <summary>
    /// Import A-Z data from an XML file which validates against the accesseastsussex.org A-Z schema
    /// </summary>
    /// <remarks>
    /// The "savexml" switch is a quick-fix implementation to see the XML being returned by the web service. 
    /// It displays the help text for some reason I haven't tried to solve, but it seems to work.
    /// </remarks>
    class Importer
    {
        private static Authority authority;
        private static int importCount = 0;
        private static EsdControlledList ipsvList;
        private static EsdMapping lgclIpsvMapping;
        private static bool xmlHasWhiteSpace;
        private static List<string> servicesDone;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {

#if DEBUG
            // Set a breakpoint here to debug

            args = new string[2];
            args[0] = "/source:Hastings";
            //args[1] = @"/url:http://hostname/services_escc_schema.xml";
            args[1] = "/webservice";
#endif
            // check command line arguments
            if (args == null || args.Length < 2)
            {
                Importer.WriteHelp();
                return;
            }

            StringDictionary switches = new StringDictionary();
            args[0] = args[0].Substring(1);
            args[1] = args[1].Substring(1);
            if (args.Length > 2) args[2] = args[2].Substring(1);

            foreach (string arg in args)
            {
                int argIndex = arg.IndexOf(":");
                if (argIndex > -1)
                {
                    switches.Add(arg.Substring(0, argIndex), arg.Substring(argIndex + 1));
                }
                else
                {
                    switches.Add(arg, "");
                }
            }


            // check authority was specified and is one of the districts or boroughs
            if (switches["source"] != null)
            {
                try
                {
                    Importer.authority = (Authority)Enum.Parse(typeof(Authority), switches["source"], true);
                }
                catch (ArgumentException)
                {
                    Importer.WriteHelp();
                    return;
                }

                if (Importer.authority == Authority.EastSussex)
                {
                    Importer.WriteHelp();
                    return;
                }
            }
            else
            {
                Importer.WriteHelp();
                return;
            }



            // set up proxy server
            WebProxy proxy = null;
            if (ConfigurationManager.AppSettings["ProxyServer"] != null)
            {
                proxy = new WebProxy(String.Format("http://{0}", ConfigurationManager.AppSettings["ProxyServer"]), true);
                proxy.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["ProxyUser"], ConfigurationManager.AppSettings["ProxyPassword"], ConfigurationManager.AppSettings["ProxyDomain"]);
            }


            // check filename or URL was specified for XML to import...
            WebResponse xmlResponse = null;
            XmlNode xmlFragment = null;

            if (switches["filename"] != null)
            {
                if (!File.Exists(switches["filename"]))
                {
                    Console.WriteLine("The XML file was not found");
                    return;
                }
            }

            else if (switches["url"] != null)
            {
                try
                {
                    WebRequest xmlRequest = WebRequest.Create(switches["url"]);

                    if (ConfigurationManager.AppSettings["ProxyServer"] != null)
                    {
                        xmlRequest.Proxy = proxy;
                    }

                    xmlResponse = xmlRequest.GetResponse();
                }
                catch (WebException ex)
                {
                    Console.WriteLine(ex.Message);
                    ExceptionManager.Publish(ex);
                    return;
                }
            }

            else if (switches["webservice"] != null)
            {
                try
                {
                    CouncilServices srvs = new CouncilServices();
                    srvs.Proxy = proxy;
                    srvs.PreAuthenticate = true;
                    srvs.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["WebServiceUser"], ConfigurationManager.AppSettings["WebServicePassword"]);
                    xmlFragment = srvs.getServicesByArea(Importer.authority.ToString());

                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.Message);
                    ExceptionManager.Publish(ex);
                    return;
                }
            }
            else
            {
                Importer.WriteHelp();
                return;
            }




            try
            {
                // Process option to save XML to a file instead of importing into database - useful for checking when there's a problem
                if (switches["savexml"] != null)
                {
                    TextReader textReader = null;
                    if (switches["filename"] != null)
                    {
                        textReader = new StreamReader(switches["filename"]);
                    }
                    else if (xmlResponse != null)
                    {
                        textReader = new StreamReader(xmlResponse.GetResponseStream());
                    }
                    else if (xmlFragment != null)
                    {
                        textReader = new StringReader(xmlFragment.OuterXml);
                    }

                    if (textReader != null)
                    {
                        StreamWriter textWriter = new StreamWriter(switches["savexml"]);
                        textWriter.Write(textReader.ReadToEnd());
                        textWriter.Flush();
                        textWriter.Close();
                    }

                    Console.WriteLine("XML saved to " + switches["savexml"]);
                    return;
                }

                // get the document from the file or the web
                XmlTextReader reader;
                if (switches["filename"] != null)
                {
                    reader = new XmlTextReader(switches["filename"]);
                }
                else if (xmlResponse != null)
                {
                    reader = new XmlTextReader(xmlResponse.GetResponseStream());
                }
                else if (xmlFragment != null)
                {
                    reader = new XmlTextReader(xmlFragment.OuterXml, XmlNodeType.Document, new XmlParserContext(null, null, "", XmlSpace.Default));
                }
                else
                {
                    // should never get here because already validated to 
                    // ensure one of the above is true, but this allows class to build
                    return;
                }

                // set up namespace
                string nsAtoZ = "http://www.accesseastsussex.org/services";

                // use a validating reader to abort import if XML invalid or not well formed
                XmlValidatingReader vreader = new XmlValidatingReader(reader);
                XmlSchemaCollection xsc = new XmlSchemaCollection();
                xsc.Add(nsAtoZ, new XmlTextReader(ConfigurationManager.AppSettings["SchemaPath"]));
                vreader.Schemas.Add(xsc);

                // Load metadata XML in case it's needed to convert from LGCL to IPSV
                ipsvList = EsdControlledList.GetControlledList("Ipsv");
                lgclIpsvMapping = EsdMapping.GetMapping("LgclIpsvMapping");

                // connect to db
                SqlConnection conn = new SqlConnection(ConfigurationManager.AppSettings["DbConnectionStringAZ"]);
                SqlTransaction t = null;

                try
                {
                    conn.Open();
                    t = conn.BeginTransaction(IsolationLevel.Serializable); // don't want *any* other changes to db while we're doing this!

                    // delete existing services
                    Console.WriteLine();
                    Console.WriteLine(String.Format("Deleting existing {0} services", Importer.authority.ToString()));

                    SqlParameter prm = new SqlParameter("@authorityId", SqlDbType.TinyInt, 1);
                    prm.Value = (int)Importer.authority;

                    SqlHelper.ExecuteNonQuery(t, CommandType.StoredProcedure, "usp_ServicesDeleteForAuthority", prm);

                    // import new services
                    Console.WriteLine();
                    Console.WriteLine(String.Format("Importing new {0} services", Importer.authority.ToString()));
                    Importer.servicesDone = new List<string>();

                    while (vreader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (reader.LocalName == "Service") Importer.ImportService(vreader, t);
                        }
                    }

                    // update derived data
                    SqlHelper.ExecuteNonQuery(t, CommandType.StoredProcedure, "usp_UpdateDerivedServiceCountAllHeadings");

                    // all OK, so save
                    t.Commit();
                }
                catch (XmlSchemaException ex)
                {
                    // XML is not valid - roll back transaction, publish error and abort
                    if (t != null) t.Rollback();

                    Console.WriteLine();
                    Console.WriteLine(ex.Message);
                    Console.WriteLine();
                    Console.WriteLine("XML was not valid. Re-run with the -savexml switch to view XML. Aborting import.");

                    NameValueCollection additional = new NameValueCollection();
                    additional.Add("Authority", Importer.authority.ToString());

                    ExceptionManager.Publish(ex, additional);

                    return;
                }
                catch (XmlException ex)
                {
                    // XML is not well formed - roll back transaction, publish error and abort
                    if (t != null) t.Rollback();

                    Console.WriteLine();
                    Console.WriteLine(ex.Message);
                    Console.WriteLine();
                    Console.WriteLine("XML was not well formed. Re-run with the -savexml switch to view XML. Aborting import.");

                    NameValueCollection additional = new NameValueCollection();
                    additional.Add("Authority", Importer.authority.ToString());

                    ExceptionManager.Publish(ex, additional);

                    return;
                }
                catch (Exception ex)
                {
                    // unexpected .net exception - roll back transaction, publish error and abort
                    if (t != null) t.Rollback();

                    Console.WriteLine();
                    Console.WriteLine(ex.Message);

                    NameValueCollection additional = new NameValueCollection();
                    additional.Add("Authority", Importer.authority.ToString());

                    ExceptionManager.Publish(ex, additional);

                    return;
                }
                finally
                {
                    conn.Close();
                    vreader.Close();
                    reader.Close();
                }

                // confirm complete
                Console.WriteLine();
                Console.WriteLine(String.Format("Imported {0} services", Importer.importCount.ToString(CultureInfo.CurrentCulture)));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                NameValueCollection additional = new NameValueCollection();
                additional.Add("Authority", Importer.authority.ToString());

                ExceptionManager.Publish(ex, additional);
            }
        }

        /// <summary>
        /// Imports a service element into the database
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="t"></param>
        private static void ImportService(XmlReader reader, SqlTransaction t)
        {
            int serviceId = -1;

            // prepare basic info about a service as SQL parameters
            SqlParameter[] prms = new SqlParameter[7];

            prms[0] = new SqlParameter("@serviceId", SqlDbType.Int, 4);
            prms[0].Direction = ParameterDirection.Output;

            prms[1] = new SqlParameter("@service", SqlDbType.VarChar, 255);

            prms[2] = new SqlParameter("@description", SqlDbType.VarChar, 2550);
            prms[2].Value = ""; // description element is optional, so give it a default

            prms[3] = new SqlParameter("@keywords", SqlDbType.VarChar, 250);
            prms[3].Value = ""; // keywords element is optional, so give it a default

            prms[4] = new SqlParameter("@authorityId", SqlDbType.TinyInt, 1);
            prms[4].Value = (int)Importer.authority;

            prms[5] = new SqlParameter("@sortPriority", SqlDbType.TinyInt, 1);
            prms[5].Value = 0; // SortPriority always 0 for districts and boroughs

            prms[6] = new SqlParameter("@ipsv", SqlDbType.Bit);
            prms[6].Value = false;

            // read data from sub-nodes into parameters
            while (reader.Read() && (reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.Comment))
            {
                if (reader.NodeType == XmlNodeType.Comment) continue;

                xmlHasWhiteSpace = (xmlHasWhiteSpace || reader.NodeType == XmlNodeType.Whitespace);

                // Case statements are in the order they appear in the XML schema, which means
                // they appear in the order they'll be hit.
                switch (reader.LocalName)
                {
                    case "ServiceName":

                        prms[1].Value = reader.ReadString().Trim();
                        if (prms[1].Value.ToString().Length == 0)
                        {
                            Console.WriteLine("Ignoring unnamed service");
                            return; // Don't import services with no name
                        }

                        // track whether this service is a duplicate
                        if (Importer.servicesDone.Contains(prms[1].Value.ToString()))
                        {
                            // Actually this could be another authority's service which shares the same name.
                            // In that case the reported message is wrong, but the behaviour is correct (ie: don't import it).
                            // Would be more complicated to report the correct message because we don't yet have access to the
                            // authority - would have to store the duplicate status for subsequent loops and process it later.
                            Console.WriteLine(String.Format("Ignoring duplicate service: '{0}'", prms[1].Value.ToString()));
                            return;
                        }
                        Importer.servicesDone.Add(prms[1].Value.ToString());

                        EsdTermCollection ipsvMatches = Importer.ipsvList.GetTerms(prms[1].Value.ToString(), true, EsdPreferredState.Preferred);
                        prms[6].Value = (ipsvMatches.Count == 1);
                        break;

                    case "ServiceDescription":

                        prms[2].Value = reader.ReadString();
                        break;

                    case "Area":

                        // Guaranteed to hit this because schema requires an Area element,
                        // and it's after ServiceName and ServiceDescription elements so 
                        // we have the basic data for a service. That's why it's written here.

                        if (reader.ReadString() == Importer.authority.ToString())
                        {
                            SqlHelper.ExecuteNonQuery(t, CommandType.StoredProcedure, "usp_ServiceInsert", prms);

                            // get new id value
                            serviceId = (int)prms[0].Value;

                            // increment counter
                            Importer.importCount++;

                            // status report
                            Console.WriteLine("Importing service: '{0}'", prms[1].Value.ToString());
                        }

                            // If service is not from the authority we're importing for, ignore it completely
                        else
                        {
                            Console.WriteLine(String.Format("Ignoring service from other authority: '{0}'", prms[1].Value.ToString()));
                            return;
                        }

                        break;

                    case "Keywords":

                        // The service has now been written, and must be updated with any subsequent details. 
                        // Re-use existing parameters, just altering the id to an input parameter.
                        prms[0].Direction = ParameterDirection.Input;

                        // Tidy up # symbols used by Eastbourne
                        string keys = reader.ReadString().Trim();
                        keys = keys.Replace("#", ";").Replace("; ;", ";");
                        if (keys.StartsWith(";")) keys = keys.Substring(1);
                        if (keys.EndsWith(";")) keys = keys.Substring(0, keys.Length - 2);
                        prms[3].Value = keys.Trim();

                        SqlHelper.ExecuteNonQuery(t, CommandType.StoredProcedure, "usp_ServiceUpdate", prms);
                        break;

                    case "Category":
                        Importer.ImportCategory(reader, t, serviceId);
                        break;

                    case "Address":
                        Importer.ImportAddress(reader, t, serviceId);
                        break;

                    case "ServiceUrl":
                        Importer.ImportServiceUrl(reader, t, serviceId);
                        break;

                }
            }

        }


        /// <summary>
        /// Imports an Address element into the database
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="t"></param>
        /// <param name="serviceId"></param>
        private static void ImportAddress(XmlReader reader, SqlTransaction t, int serviceId)
        {
            if (serviceId > -1) // which it will be, but that was the default value...
            {
                var contacts = new List<AZContact>();
                contacts.Add(new AZContact());
                int currentContact = contacts.Count - 1;
                contacts[currentContact].BS7666Address = new BS7666Address();

                int thisContactElementUsed = -1;
                bool insertingFax;

                // read data from sub-nodes into parameters
                while (reader.Read() && (reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.Comment))
                {
                    if (reader.NodeType == XmlNodeType.Comment) continue;

                    xmlHasWhiteSpace = (xmlHasWhiteSpace || reader.NodeType == XmlNodeType.Whitespace);

                    // Case statements are in the order they appear in the XML schema, which means
                    // they appear in the order they'll be hit.
                    switch (reader.LocalName)
                    {
                        case "AddressUrl":
                            contacts[0].AddressUrl = new AZUrl();
                            contacts[0].AddressUrl.Url = new Uri(reader.ReadString());
                            break;

                        case "AddressUrlText":
                            if (contacts[0].AddressUrl == null) contacts[0].AddressUrl = new AZUrl();
                            contacts[0].AddressUrl.Text = reader.ReadString();
                            break;

                        case "Validation":
                            if (xmlHasWhiteSpace)
                            {
                                reader.Skip();
                            }
                            else
                            {
                                reader.ReadString();
                            }
                            break;

                        case "Easting":
                            if (xmlHasWhiteSpace)
                            {
                                reader.Skip();
                            }
                            else
                            {
                                reader.ReadString();
                            }
                            break;

                        case "Northing":
                            if (xmlHasWhiteSpace)
                            {
                                reader.Skip();
                            }
                            else
                            {
                                reader.ReadString();
                            }
                            break;

                        case "UPRN":
                            if (xmlHasWhiteSpace)
                            {
                                reader.Skip();
                            }
                            else
                            {
                                reader.ReadString();
                            }
                            break;

                        case "USRN":
                            if (xmlHasWhiteSpace)
                            {
                                reader.Skip();
                            }
                            else
                            {
                                reader.ReadString();
                            }
                            break;

                        case "PAON":
                            contacts[0].BS7666Address.Paon = reader.ReadString().Trim(); // Trim is because our db isn't holding PAON correctly yet
                            break;

                        case "SAON":
                            contacts[0].BS7666Address.Saon = reader.ReadString().Trim(); // Trim is because our db isn't holding SAON correctly yet
                            break;

                        case "Street":
                            contacts[0].BS7666Address.StreetName = reader.ReadString();
                            break;

                        case "Locality":
                            contacts[0].BS7666Address.Locality = reader.ReadString();
                            break;

                        case "Town":
                            contacts[0].BS7666Address.Town = reader.ReadString();
                            break;

                        case "AdministrativeArea":
                            contacts[0].BS7666Address.AdministrativeArea = reader.ReadString();
                            break;

                        case "Postcode":
                            contacts[0].BS7666Address.Postcode = reader.ReadString();
                            break;


                        // Our db schema doesn't match the XML schema so we can't store complex contact 
                        // details in exactly the way they're intended; only as closely as our schema allows.

                        case "Contact":

                            thisContactElementUsed = -1;
                            insertingFax = false;

                            // read data from sub-nodes into parameters
                            while (reader.Read() && (reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.Comment))
                            {
                                if (reader.NodeType == XmlNodeType.Comment) continue;

                                switch (reader.LocalName)
                                {
                                    case "FirstName":

                                        // if we can keep filling main contact, do so
                                        if (currentContact == 0)
                                        {
                                            contacts[currentContact].FirstName = reader.ReadString();
                                        }

                                        // otherwise we definitely need a new contact, because FirstName is the first element in a contact
                                        else
                                        {
                                            AZContact newContact = new AZContact();
                                            newContact.FirstName = reader.ReadString();
                                            contacts.Add(newContact);
                                            currentContact = contacts.Count - 1;
                                        }
                                        thisContactElementUsed = currentContact;

                                        break;

                                    case "LastName":

                                        // if there was a first name
                                        if (thisContactElementUsed > -1)
                                        {
                                            contacts[thisContactElementUsed].LastName = reader.ReadString();
                                        }

                                        // if there wasn't a first name
                                        else
                                        {
                                            if (currentContact == 0)
                                            {
                                                contacts[currentContact].LastName = reader.ReadString();
                                            }
                                            else
                                            {
                                                AZContact newContact = new AZContact();
                                                newContact.LastName = reader.ReadString();
                                                contacts.Add(newContact);
                                                currentContact = contacts.Count - 1;
                                            }
                                            thisContactElementUsed = currentContact;
                                        }

                                        break;

                                    case "ContactDescription":

                                        if (thisContactElementUsed > -1)
                                        {
                                            // if already building up an AZContact from this Contact element, keep doing so
                                            contacts[thisContactElementUsed].Description = reader.ReadString();
                                        }
                                        else
                                        {
                                            if (currentContact == 0)
                                            {
                                                contacts[currentContact].Description = reader.ReadString();
                                            }
                                            else
                                            {
                                                AZContact newContact = new AZContact();
                                                newContact.Description = reader.ReadString();
                                                contacts.Add(newContact);
                                                currentContact = contacts.Count - 1;
                                            }
                                            thisContactElementUsed = currentContact;
                                        }

                                        break;

                                    case "EmailText":

                                        if (thisContactElementUsed > -1)
                                        {
                                            // if already building up an AZContact from this Contact element, keep doing so
                                            contacts[thisContactElementUsed].EmailText = reader.ReadString();
                                        }
                                        else
                                        {
                                            if (currentContact == 0)
                                            {
                                                contacts[currentContact].EmailText = reader.ReadString();
                                            }
                                            else
                                            {
                                                AZContact newContact = new AZContact();
                                                newContact.EmailText = reader.ReadString();
                                                contacts.Add(newContact);
                                                currentContact = contacts.Count - 1;
                                            }
                                            thisContactElementUsed = currentContact;
                                        }

                                        break;

                                    case "EmailAddress":

                                        if (thisContactElementUsed > -1)
                                        {
                                            // if already building up an AZContact from this Contact element, keep doing so
                                            contacts[thisContactElementUsed].Email = reader.ReadString();
                                        }
                                        else
                                        {
                                            if (currentContact == 0)
                                            {
                                                contacts[currentContact].Email = reader.ReadString();
                                            }
                                            else
                                            {
                                                AZContact newContact = new AZContact();
                                                newContact.Email = reader.ReadString();
                                                contacts.Add(newContact);
                                                currentContact = contacts.Count - 1;
                                            }
                                            thisContactElementUsed = currentContact;
                                        }

                                        break;

                                    case "PhoneText":

                                        if (reader.ReadString().ToLower().StartsWith("fax")) insertingFax = true;
                                        break;

                                    case "PhoneArea":

                                        if (thisContactElementUsed > -1)
                                        {
                                            // if already building up an AZContact from this Contact element, keep doing so
                                            if (insertingFax)
                                            {
                                                contacts[thisContactElementUsed].FaxArea = reader.ReadString();
                                            }
                                            else
                                            {
                                                contacts[thisContactElementUsed].PhoneArea = reader.ReadString();
                                            }
                                        }
                                        else
                                        {
                                            if (currentContact == 0)
                                            {
                                                if (insertingFax)
                                                {
                                                    contacts[currentContact].FaxArea = reader.ReadString();
                                                }
                                                else
                                                {
                                                    contacts[currentContact].PhoneArea = reader.ReadString();
                                                }
                                            }
                                            else
                                            {
                                                AZContact newContact = new AZContact();
                                                if (insertingFax)
                                                {
                                                    newContact.FaxArea = reader.ReadString();
                                                }
                                                else
                                                {
                                                    newContact.PhoneArea = reader.ReadString();
                                                }
                                                contacts.Add(newContact);
                                                currentContact = contacts.Count - 1;
                                            }
                                            thisContactElementUsed = currentContact;
                                        }

                                        break;

                                    case "Phone":

                                        if (thisContactElementUsed > -1)
                                        {
                                            // if already building up an AZContact from this Contact element, keep doing so
                                            if (insertingFax)
                                            {
                                                contacts[thisContactElementUsed].Fax = reader.ReadString();
                                            }
                                            else
                                            {
                                                contacts[thisContactElementUsed].Phone = reader.ReadString();
                                            }
                                        }
                                        else
                                        {
                                            if (currentContact == 0)
                                            {
                                                if (insertingFax)
                                                {
                                                    contacts[currentContact].Fax = reader.ReadString();
                                                }
                                                else
                                                {
                                                    contacts[currentContact].Phone = reader.ReadString();
                                                }
                                            }
                                            else
                                            {
                                                AZContact newContact = new AZContact();
                                                if (insertingFax)
                                                {
                                                    newContact.Fax = reader.ReadString();
                                                }
                                                else
                                                {
                                                    newContact.Phone = reader.ReadString();
                                                }
                                                contacts.Add(newContact);
                                                currentContact = contacts.Count - 1;
                                            }
                                            thisContactElementUsed = currentContact;
                                        }

                                        break;

                                    case "PhoneExtension":

                                        if (thisContactElementUsed > -1)
                                        {
                                            // if already building up an AZContact from this Contact element, keep doing so
                                            if (insertingFax)
                                            {
                                                contacts[thisContactElementUsed].FaxExtension = reader.ReadString();
                                            }
                                            else
                                            {
                                                contacts[thisContactElementUsed].PhoneExtension = reader.ReadString();
                                            }
                                        }
                                        else
                                        {
                                            if (currentContact == 0)
                                            {
                                                if (insertingFax)
                                                {
                                                    contacts[currentContact].FaxExtension = reader.ReadString();
                                                }
                                                else
                                                {
                                                    contacts[currentContact].PhoneExtension = reader.ReadString();
                                                }
                                            }
                                            else
                                            {
                                                AZContact newContact = new AZContact();
                                                if (insertingFax)
                                                {
                                                    newContact.FaxExtension = reader.ReadString();
                                                }
                                                else
                                                {
                                                    newContact.PhoneExtension = reader.ReadString();
                                                }
                                                contacts.Add(newContact);
                                                currentContact = contacts.Count - 1;
                                            }
                                            thisContactElementUsed = currentContact;
                                        }

                                        break;

                                }

                            }

                            break;

                    }
                }


                // prepare SQL parameters for adding a contact
                SqlParameter[] prms = new SqlParameter[22];

                prms[0] = new SqlParameter("@contactId", SqlDbType.Int, 4);
                prms[0].Direction = ParameterDirection.Output;

                prms[1] = new SqlParameter("@serviceId", SqlDbType.Int, 4);
                prms[1].Value = serviceId;

                prms[2] = new SqlParameter("@firstName", SqlDbType.VarChar, 35);
                prms[3] = new SqlParameter("@lastName", SqlDbType.VarChar, 35);
                prms[4] = new SqlParameter("@description", SqlDbType.VarChar, 255);
                prms[5] = new SqlParameter("@phoneArea", SqlDbType.Char, 5);
                prms[6] = new SqlParameter("@phone", SqlDbType.VarChar, 8);
                prms[7] = new SqlParameter("@phoneExtension", SqlDbType.VarChar, 8);
                prms[8] = new SqlParameter("@faxArea", SqlDbType.Char, 5);
                prms[9] = new SqlParameter("@fax", SqlDbType.VarChar, 8);
                prms[10] = new SqlParameter("@faxExtension", SqlDbType.VarChar, 8);
                prms[11] = new SqlParameter("@email", SqlDbType.VarChar, 255);
                prms[12] = new SqlParameter("@emailText", SqlDbType.VarChar, 75);
                prms[13] = new SqlParameter("@paon", SqlDbType.VarChar, 100);
                prms[14] = new SqlParameter("@saon", SqlDbType.VarChar, 100);
                prms[15] = new SqlParameter("@streetDescription", SqlDbType.VarChar, 100);
                prms[16] = new SqlParameter("@locality", SqlDbType.VarChar, 35);
                prms[17] = new SqlParameter("@town", SqlDbType.VarChar, 30);
                prms[18] = new SqlParameter("@county", SqlDbType.VarChar, 30);
                prms[19] = new SqlParameter("@postcode", SqlDbType.Char, 8);
                prms[20] = new SqlParameter("@addressUrl", SqlDbType.VarChar, 255);
                prms[21] = new SqlParameter("@addressUrlText", SqlDbType.VarChar, 75);

                // insert each gathered contact
                foreach (AZContact contact in contacts)
                {
                    prms[2].Value = contact.FirstName;
                    prms[3].Value = contact.LastName;
                    prms[4].Value = contact.Description;
                    prms[5].Value = contact.PhoneArea;
                    prms[6].Value = contact.Phone;
                    prms[7].Value = contact.PhoneExtension;
                    prms[8].Value = contact.FaxArea;
                    prms[9].Value = contact.Fax;
                    prms[10].Value = contact.FaxExtension;
                    prms[11].Value = contact.Email;
                    prms[12].Value = contact.EmailText;
                    prms[13].Value = contact.BS7666Address.Paon;
                    prms[14].Value = contact.BS7666Address.Saon;
                    prms[15].Value = contact.BS7666Address.StreetName;
                    prms[16].Value = contact.BS7666Address.Locality;
                    prms[17].Value = contact.BS7666Address.Town;
                    prms[18].Value = contact.BS7666Address.AdministrativeArea;
                    prms[19].Value = contact.BS7666Address.Postcode;
                    if (contact.AddressUrl != null && contact.AddressUrl.Url != null)
                    {
                        prms[20].Value = contact.AddressUrl.Url.ToString();
                        prms[21].Value = contact.AddressUrl.Text;
                    }
                    else
                    {
                        prms[20].Value = DBNull.Value;
                        prms[21].Value = DBNull.Value;
                    }

                    SqlHelper.ExecuteNonQuery(t, CommandType.StoredProcedure, "usp_InsertContact", prms);
                }

            }
        }

        /// <summary>
        /// Imports a Category element into the database
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="t"></param>
        /// <param name="serviceId"></param>
        private static void ImportCategory(XmlReader reader, SqlTransaction t, int serviceId)
        {
            if (serviceId > -1) // which it will be, but that was the default value...
            {
                // prepare basic info about a service as SQL parameters
                SqlParameter[] prms = new SqlParameter[3];

                prms[0] = new SqlParameter("@serviceId", SqlDbType.Int, 4);
                prms[0].Value = serviceId;

                prms[1] = new SqlParameter("@categoryIdentifier", SqlDbType.VarChar, 200);

                prms[2] = new SqlParameter("@scheme", SqlDbType.VarChar, 200);

                // prepare params for noting which IPSV terms were used by districts and boroughs
                SqlParameter[] ipsvPrms = new SqlParameter[2];

                ipsvPrms[0] = new SqlParameter("@serviceId", SqlDbType.Int, 4);
                ipsvPrms[0].Value = serviceId;

                ipsvPrms[1] = new SqlParameter("@ipsvTerm", SqlDbType.VarChar, 100);

                // read data from sub-nodes into parameters
                bool isLgcl = false;
                while (reader.Read() && (reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.Comment))
                {
                    if (reader.NodeType == XmlNodeType.Comment) continue;

                    xmlHasWhiteSpace = (xmlHasWhiteSpace || reader.NodeType == XmlNodeType.Whitespace);

                    // Case statements are in the order they appear in the XML schema, which means
                    // they appear in the order they'll be hit.
                    switch (reader.LocalName)
                    {
                        case "Scheme":

                            isLgcl = false; // For Rother's LGCL. Resets variable for a new term. Delete this when LGCL redundant.

                            string scheme = reader.ReadString();

                            // We're storing the scheme differently.
                            if (scheme == "http://www.esd.org.uk/standards/ipsv") scheme = "IPSV";
                            prms[2].Value = scheme;

                            break;

                        case "Identifier":

                            // Guaranteed to hit this because schema requires an Identifier element,
                            // and we have the data we need at this point so write the data here.

                            if (prms[2].Value.ToString() == "IPSV")
                            {
                                prms[1].Value = reader.ReadString();
                            }
                            else if (prms[2].Value.ToString() == "http://www.esd.org.uk/standards/lgcl")
                            {
                                // Rother still using LGCL, so convert to IPSV to ensure we get their data.
                                // LGCL is the only reason for the EgmsWebMetadata reference, so it can be removed along with all other LGCL code when LGCL is redundant.
                                EsdTermCollection ipsvTerms = lgclIpsvMapping.GetMappedTerms(ipsvList, reader.ReadString());

                                // Just use the first one for now
                                if (ipsvTerms.Count > 0)
                                {
                                    prms[1].Value = ipsvTerms[0].Id;
                                    prms[2].Value = "IPSV";
                                }

                                isLgcl = true;
                            }

                            SqlHelper.ExecuteNonQuery(t, CommandType.StoredProcedure, "usp_ServiceRelateToCategory", prms);
                            break;

                        case "CommonName":

                            // Record each IPSV term used for import, purely to show it to editors
                            // An LGCL term will go in here as if it's IPSV, with no indication that it's really LGCL. Hope to delete LGCL code to fix this.
                            ipsvPrms[1].Value = reader.ReadString();

                            if (isLgcl) ipsvPrms[1].Value += " (LGCL)";

                            SqlHelper.ExecuteNonQuery(t, CommandType.StoredProcedure, "usp_ServiceAddIpsvImported", ipsvPrms);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Imports a ServiceUrl element into the database
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="t"></param>
        /// <param name="serviceId"></param>
        private static void ImportServiceUrl(XmlReader reader, SqlTransaction t, int serviceId)
        {
            if (serviceId > -1) // which it will be, but that was the default value...
            {
                // prepare basic info about a service as SQL parameters
                SqlParameter[] prms = new SqlParameter[4];

                prms[0] = new SqlParameter("@serviceId", SqlDbType.Int, 4);
                prms[0].Value = serviceId;

                prms[1] = new SqlParameter("@url", SqlDbType.VarChar, 255);

                prms[2] = new SqlParameter("@urlTitle", SqlDbType.VarChar, 75);
                prms[2].Value = ""; // UrlText element is optional, so give it a default

                prms[3] = new SqlParameter("@urlDescription", SqlDbType.VarChar, 300);
                prms[3].Value = DBNull.Value; // UrlDescription element is optional, so give it a default

                // read data from sub-nodes into parameters
                while (reader.Read() && (reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.Comment))
                {
                    if (reader.NodeType == XmlNodeType.Comment) continue;

                    xmlHasWhiteSpace = (xmlHasWhiteSpace || reader.NodeType == XmlNodeType.Whitespace);

                    // Case statements are in the order they appear in the XML schema, which means
                    // they appear in the order they'll be hit.
                    switch (reader.LocalName)
                    {
                        case "UrlText":

                            prms[2].Value = reader.ReadString();

                            break;

                        case "UrlDescription":

                            prms[3].Value = reader.ReadString();
                            if (prms[3].Value.ToString() == prms[2].Value.ToString()) prms[3].Value = String.Empty; // No point having description the same as link text
                            break;

                        case "Provider":

                            if (xmlHasWhiteSpace)
                            {
                                reader.Skip();
                            }
                            else
                            {
                                reader.ReadString();
                            }
                            break;

                        case "Url":

                            // Guaranteed to hit this because schema requires a Url element,
                            // and it's the last child of ServiceUrl so write the data here.

                            prms[1].Value = reader.ReadString();
                            if (prms[3].Value.ToString() == prms[1].Value.ToString()) prms[3].Value = String.Empty; // No point having description the same as URL

                            SqlHelper.ExecuteNonQuery(t, CommandType.StoredProcedure, "usp_UrlInsert", prms);
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Writes help info to the console
        /// </summary>
        private static void WriteHelp()
        {
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine();
            Console.WriteLine("AZImporter -source:Hastings -filename:data.xml");
            Console.WriteLine("AZImporter -source:Hastings -url:http://www.somedomain.com/data.xml");
            Console.WriteLine("AZImporter -source:Hastings -webservice");
            Console.WriteLine("AZImporter -source:Hastings -webservice -savexml:c:\\somefile.xml");
            Console.WriteLine();
            Console.WriteLine("The source must be Eastbourne, Hastings, Lewes, Rother or Wealden");
            Console.WriteLine();
        }

    }
}
