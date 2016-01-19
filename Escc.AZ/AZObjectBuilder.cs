using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using Escc.AddressAndPersonalDetails;
using Escc.Web.Metadata;
using Microsoft.ApplicationBlocks.ExceptionManagement;

namespace Escc.AZ
{
    /// <summary>
    /// Utility business logic class for building A-Z objects
    /// </summary>
    public static class AZObjectBuilder
    {
        /// <summary>
        /// Builds the service from raw data.
        /// </summary>
        /// <param name="serviceData">The service data.</param>
        /// <returns></returns>
        public static AZService BuildServiceFromRawData(DataTable serviceData)
        {
            if (serviceData == null) throw new ArgumentNullException("serviceData");

            // collections to remember items already related (to display, and to stop them being selected again)
            ArrayList alreadyRelated = new ArrayList();
            ArrayList urlsDone = new ArrayList();
            ArrayList contactsDone = new ArrayList();

            // Build the service
            AZService currentService = new AZService();
            currentService.Service = serviceData.Rows[0]["Service"].ToString();
            currentService.Description = serviceData.Rows[0]["Description"].ToString();
            currentService.Keywords = serviceData.Rows[0]["Keywords"].ToString();
            currentService.Authority = (Authority)Enum.Parse(typeof(Authority), serviceData.Rows[0]["Authority"].ToString(), true);
            currentService.IpsvPreferredTerms.ReadString(serviceData.Rows[0]["IpsvImported"].ToString());

            // loop through multiple rows - remember ids of those done
            foreach (DataRow row in serviceData.Rows)
            {
                // related headings
                if (row["HeadingId"] != DBNull.Value && !alreadyRelated.Contains(row["HeadingId"]))
                {
                    AZHeading head = new AZHeading();
                    head.Id = Int32.Parse(row["HeadingId"].ToString(), CultureInfo.InvariantCulture);
                    head.Heading = row["Heading"].ToString();
                    currentService.Headings.Add(head);

                    alreadyRelated.Add(row["HeadingId"]);
                }

                // related urls
                if (row["ServiceUrlId"] != DBNull.Value && !urlsDone.Contains(row["ServiceUrlId"]))
                {
                    try
                    {
                        AZUrl url = new AZUrl();
                        url.Id = Int32.Parse(row["ServiceUrlId"].ToString(), CultureInfo.InvariantCulture);
                        url.Url = new Uri(row["Url"].ToString());
                        url.Text = row["UrlTitle"].ToString();
                        url.Description = row["UrlDescription"].ToString();
                        currentService.Urls.Add(url);
                    }
                    catch (UriFormatException ex)
                    {
                        // publish exception with details of URL, then carry on
                        NameValueCollection extraInfo = new NameValueCollection(3);
                        extraInfo["Service"] = currentService.Service;
                        extraInfo["Authority"] = currentService.Authority.ToString();
                        extraInfo["UrlId"] = row["ServiceUrlId"].ToString();
                        extraInfo["Url"] = row["Url"].ToString();

                        ExceptionManager.Publish(ex, extraInfo);

                    }

                    urlsDone.Add(row["ServiceUrlId"]);
                }

                // related contacts
                if (row["ContactId"] != DBNull.Value && !contactsDone.Contains(row["ContactId"]))
                {
                    AZContact contact = AZObjectBuilder.BuildContactFromRawData(row);
                    if (contact != null) currentService.AddContact(contact);

                    contactsDone.Add(row["ContactId"]);
                }
            }

            return currentService;

        }

        /// <summary>
        /// Extract AZHeading object and its related AZService objects from the raw data returned from the db
        /// </summary>
        /// <param name="data">Raw data returned from the db, sorted by heading, sub-sorted by service</param>
        /// <returns>AZHeading object with populated properties, including Services property containing AZService objects</returns>
        public static IList<AZHeading> BuildHeadingsFromRawData(DataTable data)
        {
            if (data == null) throw new ArgumentNullException("data");

            // create collection to be returned
            IList<AZHeading> collection = new List<AZHeading>();

            // check we have some data
            if (data.Rows.Count > 0)
            {
                // variables for working with headings during loop
                AZHeading[] headings = new AZHeading[data.Rows.Count];
                AZService[] services = null;
                int headingIndex = -1;
                int serviceIndex = -1;
                int currentHeading = -1;
                int currentService = -1;
                int headingId;
                int serviceId;
                ArrayList urlsDone = new ArrayList();
                ArrayList contactsDone = new ArrayList();
                ArrayList ipsvDone = new ArrayList();

                foreach (DataRow row in data.Rows)
                {
                    headingId = Convert.ToInt32(row["HeadingId"], CultureInfo.InvariantCulture);

                    if (currentHeading != headingId)
                    {
                        // finish off the old heading by adding services
                        if (headingIndex > -1) foreach (AZService service in services) if (service != null) headings[headingIndex].AddService(service);

                        // this row is the start of a new heading
                        headingIndex++;
                        currentHeading = headingId;
                        if (ipsvDone.Count > 0) ipsvDone.Clear();

                        headings[headingIndex] = new AZHeading();
                        headings[headingIndex].Id = headingId;
                        headings[headingIndex].Heading = row["Heading"].ToString();

                        // a new heading means a new set of services
                        services = new AZService[data.Rows.Count];
                        serviceIndex = -1;
                        currentService = -1;

                        // NOTE: IPSV preferred terms treated as relational data because they're used to control import/export.
                        //		 Non-preferred terms treated as strings because that's the only way they're ever used,
                        //		 and re-using the IPSV structure would generate overhead with extra rows in queries
                        headings[headingIndex].IpsvNonPreferredTerms = row["IpsvNonPreferredTerms"].ToString();
                    }

                    // add ipsv term if required
                    if (row["Identifier"] != DBNull.Value && !ipsvDone.Contains(row["Identifier"]))
                    {
                        EsdTerm term = new EsdTerm();
                        term.Id = row["Identifier"].ToString();
                        term.Text = row["CommonName"].ToString();
                        headings[headingIndex].IpsvPreferredTerms.Add(term);
                        ipsvDone.Add(row["Identifier"]);
                    }

                    // check whether we're dealing with a new service in this row
                    serviceId = Convert.ToInt32(row["ServiceId"], CultureInfo.InvariantCulture);

                    if (currentService != serviceId)
                    {
                        // this row is the start of a new service
                        serviceIndex++;
                        currentService = serviceId;
                        if (urlsDone.Count > 0) urlsDone.Clear();
                        if (contactsDone.Count > 0) contactsDone.Clear();

                        services[serviceIndex] = new AZService();
                        services[serviceIndex].Id = serviceId;
                        services[serviceIndex].Service = row["Service"].ToString();
                        if (row["Description"] != DBNull.Value) services[serviceIndex].Description = row["Description"].ToString();
                        if (row["Keywords"] != DBNull.Value) services[serviceIndex].Keywords = row["Keywords"].ToString();

                        // get Authority enum based on int from db
                        services[serviceIndex].Authority = (Authority)Enum.Parse(typeof(Authority), row["Authority"].ToString());

                        // get url
                        if (row["ServiceUrlId"] != DBNull.Value && !urlsDone.Contains(row["ServiceUrlId"]))
                        {
                            try
                            {
                                services[serviceIndex].Urls.Add(new Uri(row["Url"].ToString()), row["UrlTitle"].ToString(), row["UrlDescription"].ToString());
                            }
                            catch (UriFormatException ex)
                            {
                                // publish exception with details of URL, then carry on
                                NameValueCollection extraInfo = new NameValueCollection(3);
                                extraInfo["ServiceId"] = serviceId.ToString(CultureInfo.CurrentCulture);
                                extraInfo["Service"] = services[serviceIndex].Service;
                                extraInfo["Authority"] = services[serviceIndex].Authority.ToString();
                                extraInfo["UrlId"] = row["ServiceUrlId"].ToString();
                                extraInfo["Url"] = row["Url"].ToString();

                                ExceptionManager.Publish(ex, extraInfo);
                            }
                            urlsDone.Add(row["ServiceUrlId"]);
                        }

                        if (!contactsDone.Contains(row["ContactId"]))
                        {
                            AZContact contact = AZObjectBuilder.BuildContactFromRawData(row);
                            if (contact != null) services[serviceIndex].AddContact(contact);
                            contactsDone.Add(row["ContactId"]);
                        }
                    }
                    // process multiple-rows-for-a-service
                    else
                    {
                        // urls are one possible cause of multiple-rows-per-service
                        // check for url in this row, and make sure not already done
                        if (row["ServiceUrlId"] != DBNull.Value && !urlsDone.Contains(row["ServiceUrlId"]))
                        {
                            services[serviceIndex].Urls.Add(new Uri(row["Url"] as string), row["UrlTitle"] as string, row["UrlDescription"].ToString());
                            urlsDone.Add(row["ServiceUrlId"]);
                        }

                        // contacts are one possible cause of multiple-rows-per-service
                        // check for contact in this row, and make sure not already done
                        if (!contactsDone.Contains(row["ContactId"]))
                        {
                            AZContact contact = AZObjectBuilder.BuildContactFromRawData(row);
                            if (contact != null) services[serviceIndex].AddContact(contact);
                            contactsDone.Add(row["ContactId"]);
                        }
                    }
                }

                // Normally services are added to a heading when the next heading is found. 
                // That doesn't happen for last heading, so add them here instead.
                if (headingIndex > -1) foreach (AZService service in services) if (service != null) headings[headingIndex].AddService(service);

                // Now move the headings into the collection
                foreach (AZHeading heading in headings) if (heading != null) collection.Add(heading);
            }

            return collection;
        }

        /// <summary>
        /// Helper method to check for and build an AZContact from a row of raw data
        /// </summary>
        /// <param name="row">A row of raw data</param>
        public static AZContact BuildContactFromRawData(DataRow row)
        {
            if (row == null) throw new ArgumentNullException("row");

            // have to check every property because they're all optional
            if ((row["FirstName"] != DBNull.Value && row["FirstName"].ToString().Length > 0) ||
                (row["LastName"] != DBNull.Value && row["LastName"].ToString().Length > 0) ||
                (row["ContactDescription"] != DBNull.Value && row["ContactDescription"].ToString().Length > 0) ||
                (row["PhoneArea"] != DBNull.Value && row["PhoneArea"].ToString().Length > 0 && row["Phone"] != DBNull.Value && row["Phone"].ToString().Length > 0) ||
                (row["FaxArea"] != DBNull.Value && row["FaxArea"].ToString().Length > 0 && row["Fax"] != DBNull.Value && row["Fax"].ToString().Length > 0) ||
                (row["Email"] != DBNull.Value && row["Email"].ToString().Length > 0) ||
                (row["Town"] != DBNull.Value && row["Town"].ToString().Length > 0)
                )
            {
                // build the contact
                AZContact contact = new AZContact();
                contact.Id = (int)row["ContactId"];
                contact.ServiceId = (int)row["ServiceId"];
                contact.FirstName = row["FirstName"].ToString();
                contact.LastName = row["LastName"].ToString();
                contact.Description = row["ContactDescription"].ToString();
                contact.PhoneArea = row["PhoneArea"].ToString();
                contact.Phone = row["Phone"].ToString();
                contact.PhoneExtension = row["PhoneExtension"].ToString();
                contact.FaxArea = row["FaxArea"].ToString();
                contact.Fax = row["Fax"].ToString();
                contact.FaxExtension = row["FaxExtension"].ToString();
                contact.Email = row["Email"].ToString();
                contact.EmailText = row["EmailText"].ToString();

                if (row["AddressUrl"] != DBNull.Value && row["AddressUrl"].ToString().Length > 0)
                {
                    contact.AddressUrl = new AZUrl();
                    contact.AddressUrl.Url = new Uri(row["AddressUrl"].ToString());
                    if (row["AddressUrlText"] != DBNull.Value) contact.AddressUrl.Text = row["AddressUrlText"].ToString();
                }

                BS7666Address addr = new BS7666Address();
                addr.Paon = row["PAON"].ToString();
                addr.Saon = row["SAON"].ToString();
                addr.StreetName = row["StreetDescription"].ToString();
                addr.Locality = row["Locality"].ToString();
                addr.Town = row["Town"].ToString();
                addr.AdministrativeArea = row["County"].ToString();
                addr.Postcode = row["Postcode"].ToString();
                contact.BS7666Address = addr;

                // add contact to service
                return contact;
            }
            else return null;
        }

    }
}
