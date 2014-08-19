using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using eastsussexgovuk.webservices.EgmsWebMetadata;
using eastsussexgovuk.webservices.TextXhtml.HouseStyle;
using Escc.AddressAndPersonalDetails;
using Microsoft.ApplicationBlocks.Data;
using Microsoft.ApplicationBlocks.ExceptionManagement;

namespace Escc.AZ.Exporter
{
    /// <summary>
    /// Export ESCC A-Z data to an XML file which validates against the accesseastsussex.org A-Z schema
    /// </summary>
    class Exporter
    {
        private static bool xmlIsValid = true;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // Get data from database.

                // Although headings aren't being exported, get them from the db because
                // they have the IPSV categories we need, and because we can re-use the code
                // to parse the rows of raw data. IPSV categories are related to headings 
                // so that services can be associated with headings on import.

                Console.WriteLine("Connecting to database");

                SqlConnection conn = new SqlConnection(ConfigurationManager.AppSettings["DbConnectionStringAZ"]);
                DataTable data = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "usp_SelectServicesForExport").Tables[0];
                var headings = AZObjectBuilder.BuildHeadingsFromRawData(data);

                // Reorganise the data into a collection of services
                ArrayList servicesDone = new ArrayList();
                ArrayList services = new ArrayList();
                foreach (AZHeading heading in headings)
                {
                    foreach (AZService service in heading.Services)
                    {
                        if (!servicesDone.Contains(service.Id))
                        {
                            services.Add(service);
                            servicesDone.Add(service.Id);
                        }
                    }
                }


                Console.WriteLine("Opening XML file");

                // set up the XML writer and namespaces
                string tempFile = String.Format("{0}.temp", ConfigurationManager.AppSettings["ExportPath"]);
                XmlTextWriter writer = new XmlTextWriter(tempFile, System.Text.Encoding.UTF8);
                writer.Formatting = Formatting.Indented;

                writer.Namespaces = true;
                string nsAtoZ = "http://www.accesseastsussex.org/services";
                string bs7666Hack;

                // variables used in loop
                string serviceId;
                string contactId;
                bool hasAddress;

                // write XML declaration
                writer.WriteStartDocument();

                // write root element
                writer.WriteStartElement("Services", nsAtoZ);

                // write element for each service
                foreach (AZService service in services)
                {
                    // Some services actually just say "see your district and borough for that", so don't export them
                    if (service.Contacts.Count == 0 && service.Urls.Count == 1 && service.Urls[0].Url.ToString().Contains("/districtandborough/"))
                    {
                        Console.WriteLine(String.Format("Ignoring link to district/borough service: '{0}'", service.Service));
                        continue;
                    }

                    Console.WriteLine(String.Format("Writing service: '{0}'", service.Service));

                    serviceId = service.Id.ToString(CultureInfo.CurrentCulture);

                    writer.WriteStartElement("Service");
                    writer.WriteAttributeString("ServiceId", serviceId);
                    writer.WriteAttributeString("PublicationRule", "true"); // field not used, so fill with constant value

                    // once-only info about service
                    writer.WriteStartElement("ServiceName");
                    writer.WriteString(service.Service);
                    writer.WriteEndElement();

                    if (service.Description.Length > 0)
                    {
                        writer.WriteStartElement("ServiceDescription");
                        writer.WriteString(service.Description);
                        writer.WriteEndElement();
                    }

                    writer.WriteStartElement("Area");
                    writer.WriteString("East Sussex");
                    writer.WriteEndElement();

                    if (service.Keywords.Length > 0)
                    {
                        writer.WriteStartElement("Keywords");
                        writer.WriteString(service.Keywords);
                        writer.WriteEndElement();
                    }

                    // write IPSV categories
                    foreach (EsdTerm term in service.IpsvPreferredTerms)
                    {
                        writer.WriteStartElement("Category");
                        writer.WriteAttributeString("ServiceId", serviceId);

                        // CategoryId is optional and it's not in the EsdTerm object model, so leave it out
                        // writer.WriteAttributeString("CategoryId", "");

                        writer.WriteStartElement("Scheme");
                        writer.WriteString("http://www.esd.org.uk/standards/ipsv"); // we're only using IPSV for the forseeable future, so just hard-code it
                        writer.WriteEndElement();

                        writer.WriteStartElement("Identifier");
                        writer.WriteString(term.Id);
                        writer.WriteEndElement();

                        writer.WriteStartElement("CommonName");
                        writer.WriteString(term.Text);
                        writer.WriteEndElement();

                        writer.WriteEndElement(); // end Category element
                    }

                    // write postal addresses
                    foreach (AZContact contact in service.Contacts)
                    {
                        BS7666Address addr = contact.BS7666Address;
                        hasAddress = ((addr.Uprn != null && addr.Uprn.Length > 0) ||
                            (addr.Usrn != null && addr.Usrn.Length > 0) ||
                            (addr.Paon != null && addr.Paon.Length > 0) ||
                            (addr.Saon != null && addr.Saon.Length > 0) ||
                            (addr.StreetName != null && addr.StreetName.Length > 0) ||
                            (addr.Locality != null && addr.Locality.Length > 0) ||
                            (addr.Town != null && addr.Town.Length > 0) ||
                            (addr.AdministrativeArea != null && addr.AdministrativeArea.Length > 0) ||
                            (addr.Postcode != null && addr.Postcode.Length > 0));

                        contactId = contact.Id.ToString(CultureInfo.CurrentCulture);

                        writer.WriteStartElement("Address");
                        writer.WriteAttributeString("AddressId", contactId); // our database structure is different - we don't have a separate id for the address
                        writer.WriteAttributeString("ServiceId", serviceId);
                        writer.WriteAttributeString("HasAddress", hasAddress.ToString().ToLower());

                        // AddressUrl and AddressUrlText are optional
                        if (contact.AddressUrl != null && contact.AddressUrl.Url != null && contact.AddressUrl.Url.ToString().Length > 0)
                        {
                            writer.WriteStartElement("AddressUrl");
                            writer.WriteString(contact.AddressUrl.Url.ToString());
                            writer.WriteEndElement();

                            if (contact.AddressUrl.Text.Length > 0)
                            {
                                writer.WriteStartElement("AddressUrlText");
                                writer.WriteString(contact.AddressUrl.Text);
                                writer.WriteEndElement();
                            }
                        }

                        writer.WriteStartElement("Validation");
                        writer.WriteString("not-checked"); // we're not checking any A-Z addresses against PAF or NLPG yet
                        writer.WriteEndElement();

                        // Easting, northing, UPRN and USRN are optional, and we don't store any of them

                        // writer.WriteStartElement("Easting"); 
                        // writer.WriteEndElement();

                        // writer.WriteStartElement("Northing"); 
                        // writer.WriteEndElement();

                        // writer.WriteStartElement("UPRN"); 
                        // writer.WriteEndElement();

                        // writer.WriteStartElement("USRN"); 
                        // writer.WriteEndElement();

                        if (addr.Paon != null && addr.Paon.Length > 0)
                        {
                            // We're not using PAON quite correctly, so pad and truncate to fit the regex
                            bs7666Hack = String.Format("          {0}", addr.Paon);
                            if (bs7666Hack.Length > 100) bs7666Hack = bs7666Hack.Substring(0, 100);

                            writer.WriteStartElement("PAON");
                            writer.WriteString(bs7666Hack);
                            writer.WriteEndElement();
                        }

                        if (addr.Saon != null && addr.Saon.Length > 0)
                        {
                            // We're not using SAON quite correctly, so pad and truncate to fit the regex
                            bs7666Hack = String.Format("          {0}", addr.Saon);
                            if (bs7666Hack.Length > 100) bs7666Hack = bs7666Hack.Substring(0, 100);

                            writer.WriteStartElement("SAON");
                            writer.WriteString(bs7666Hack);
                            writer.WriteEndElement();
                        }

                        if (addr.StreetName != null && addr.StreetName.Length > 0)
                        {
                            writer.WriteStartElement("Street");
                            writer.WriteString(addr.StreetName);
                            writer.WriteEndElement();
                        }

                        if (addr.Locality != null && addr.Locality.Length > 0)
                        {
                            writer.WriteStartElement("Locality");
                            writer.WriteString(addr.Locality);
                            writer.WriteEndElement();
                        }

                        if (addr.Town != null && addr.Town.Length > 0)
                        {
                            writer.WriteStartElement("Town");
                            writer.WriteString(addr.Town);
                            writer.WriteEndElement();
                        }

                        if (addr.AdministrativeArea != null && addr.AdministrativeArea.Length > 0)
                        {
                            writer.WriteStartElement("AdministrativeArea");
                            writer.WriteString(addr.AdministrativeArea);
                            writer.WriteEndElement();
                        }

                        if (addr.Postcode != null && addr.Postcode.Length > 0)
                        {
                            writer.WriteStartElement("Postcode");
                            writer.WriteString(addr.Postcode);
                            writer.WriteEndElement();
                        }

                        // Write contacts

                        // Write one <Contact /> for name/desc/email, and one for each type of phone number
                        if (contact.FirstName.Length > 0 || contact.LastName.Length > 0 || contact.Description.Length > 0 || contact.Email.Length > 0)
                        {
                            writer.WriteStartElement("Contact");
                            writer.WriteAttributeString("ContactId", contactId);
                            writer.WriteAttributeString("AddressId", contactId);

                            if (contact.FirstName.Length > 0)
                            {
                                writer.WriteStartElement("FirstName");
                                writer.WriteString(contact.FirstName);
                                writer.WriteEndElement();
                            }

                            if (contact.LastName.Length > 0)
                            {
                                writer.WriteStartElement("LastName");
                                writer.WriteString(contact.LastName);
                                writer.WriteEndElement();
                            }

                            if (contact.Description.Length > 0)
                            {
                                writer.WriteStartElement("ContactDescription");
                                writer.WriteString(contact.Description);
                                writer.WriteEndElement();
                            }

                            if (contact.EmailText.Length > 0)
                            {
                                writer.WriteStartElement("EmailText");
                                writer.WriteString(contact.EmailText);
                                writer.WriteEndElement();
                            }

                            if (contact.Email.Length > 0)
                            {
                                writer.WriteStartElement("EmailAddress");
                                writer.WriteString(contact.Email);
                                writer.WriteEndElement();
                            }

                            writer.WriteEndElement(); // end Contact element
                        }

                        // Phone element is optional, but if there's no phone there shouldn't be an area code or extension either
                        if (contact.Phone.Length > 0)
                        {
                            writer.WriteStartElement("Contact");
                            writer.WriteAttributeString("ContactId", contactId);
                            writer.WriteAttributeString("AddressId", contactId);

                            writer.WriteStartElement("PhoneText"); // PhoneText element is optional, but this property can always be marked "Tel: "
                            writer.WriteString("Tel");
                            writer.WriteEndElement();

                            if (contact.PhoneArea.Length > 0)
                            {
                                writer.WriteStartElement("PhoneArea");
                                writer.WriteString(contact.PhoneArea);
                                writer.WriteEndElement();
                            }

                            writer.WriteStartElement("Phone");
                            writer.WriteString(contact.Phone.Replace(" ", String.Empty)); // our database allows 9 characters for 0345 60 80 190, but schema only allows 8.
                            writer.WriteEndElement();

                            if (contact.PhoneExtension.Length > 0)
                            {
                                writer.WriteStartElement("PhoneExtension");
                                writer.WriteString(contact.PhoneExtension);
                                writer.WriteEndElement();
                            }

                            writer.WriteEndElement(); // end Contact element
                        }

                        // Phone element is optional, but if there's no fax there shouldn't be an area code or extension either
                        if (contact.Fax.Length > 0)
                        {
                            writer.WriteStartElement("Contact");
                            writer.WriteAttributeString("ContactId", contactId);
                            writer.WriteAttributeString("AddressId", contactId);

                            writer.WriteStartElement("PhoneText"); // PhoneText element is optional, but this property can always be marked "Fax: "
                            writer.WriteString("Fax");
                            writer.WriteEndElement();

                            if (contact.FaxArea.Length > 0)
                            {
                                writer.WriteStartElement("PhoneArea");
                                writer.WriteString(contact.FaxArea);
                                writer.WriteEndElement();
                            }

                            writer.WriteStartElement("Phone");
                            writer.WriteString(contact.Fax);
                            writer.WriteEndElement();

                            if (contact.FaxExtension.Length > 0)
                            {
                                writer.WriteStartElement("PhoneExtension");
                                writer.WriteString(contact.FaxExtension);
                                writer.WriteEndElement();
                            }

                            writer.WriteEndElement(); // end Contact element
                        }

                        writer.WriteEndElement(); // end Address element
                    }

                    // Write urls
                    foreach (AZUrl url in service.Urls)
                    {
                        writer.WriteStartElement("ServiceUrl");
                        // writer.WriteAttributeString("UrlId", ""); URLs have an id, but not stored in current object model
                        writer.WriteAttributeString("ServiceId", serviceId);

                        // UrlText is optional, but it's already been parsed so there's something there, even if it's just the URL
                        writer.WriteStartElement("UrlText");
                        writer.WriteString(url.Text);
                        writer.WriteEndElement();

                        // UrlDescription is optional
                        if (url.Description != null && url.Description.Length > 0)
                        {
                            writer.WriteStartElement("UrlDescription");
                            writer.WriteString(url.Description);
                            writer.WriteEndElement();
                        }

                        // writer.WriteStartElement("Provider"); // Provider is optional, and we don't use it
                        // writer.WriteEndElement();


                        writer.WriteStartElement("Url");
                        writer.WriteString(url.Url.ToString());
                        writer.WriteEndElement();

                        writer.WriteEndElement(); // end ServiceUrl element
                    }



                    writer.WriteEndElement();
                }

                // end document
                writer.WriteEndElement();
                writer.WriteComment(String.Format("A-Z data exported and validated by East Sussex County Council, {0}", DateTimeFormatter.FullBritishDateWithDayAndTime(DateTime.Now)));
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();

                // validate the document against the schema and publish exception if it fails
                Console.WriteLine("Validating XML");
                Exporter.xmlIsValid = true;

                XmlTextReader reader = new XmlTextReader(tempFile);
                XmlValidatingReader vreader = new XmlValidatingReader(reader);
                XmlSchemaCollection xsc = new XmlSchemaCollection();
                xsc.Add(nsAtoZ, new XmlTextReader(ConfigurationManager.AppSettings["SchemaPath"]));
                vreader.Schemas.Add(xsc);

                try
                {
                    while (vreader.Read())
                    {
                    }
                }
                catch (XmlSchemaException ex)
                {
                    // XML is invalid
                    Exporter.xmlIsValid = false;
                    Console.WriteLine(ex.Message);
                    Exporter.PublishExceptionWithXml(tempFile, ex, vreader.LineNumber, vreader.LinePosition);
                }
                catch (XmlException ex)
                {
                    // XML is not well formed
                    Exporter.xmlIsValid = false;
                    Console.WriteLine(ex.Message);
                    Exporter.PublishExceptionWithXml(tempFile, ex, vreader.LineNumber, vreader.LinePosition);
                }
                finally
                {
                    vreader.Close();
                    reader.Close();
                }

                if (File.Exists(tempFile))
                {
                    if (Exporter.xmlIsValid)
                    {
                        // If valid XML copy new export over existing export
                        File.Copy(tempFile, ConfigurationManager.AppSettings["ExportPath"], true);
                    }

                    // delete the temp file if success; if failure, it'll be useful for working out what's wrong
                    if (xmlIsValid) File.Delete(tempFile);
                }


            }
            catch (Exception ex)
            {
                Exporter.xmlIsValid = false;
                Console.WriteLine(ex.Message);
                ExceptionManager.Publish(ex);
            }
        }


        /// <summary>
        /// Publish the relevant XML along with a related XmlException
        /// </summary>
        /// <param name="xmlFilename"></param>
        /// <param name="innerEx"></param>
        /// <param name="lineNumber">The line of the XML where the error occurred</param>
        /// <param name="linePosition">The character position on the line of the XML where the error occurred</param>
        private static void PublishExceptionWithXml(string xmlFilename, Exception innerEx, int lineNumber, int linePosition)
        {
            // If XML was not well formed or invalid, read it as text and publish as an exception
            StreamReader sr = File.OpenText(xmlFilename);
            string textLine;
            StringBuilder sb = new StringBuilder(innerEx.GetType().ToString())
                .Append(Environment.NewLine)
                .Append(Environment.NewLine)
                .Append("Full XML has been saved to a temporary file: ")
                .AppendFormat("{0}.temp", ConfigurationManager.AppSettings["ExportPath"])
                .Append(Environment.NewLine)
                .Append(Environment.NewLine);
            while ((textLine = sr.ReadLine()) != null)
            {
                sb.Append(textLine).Append(Environment.NewLine);
            }
            sr.Close();

            XmlException newException = new XmlException(sb.ToString(), innerEx, lineNumber, linePosition);
            newException.Source = Assembly.GetExecutingAssembly().GetName().Name;
            ExceptionManager.Publish(newException);
        }
    }
}
