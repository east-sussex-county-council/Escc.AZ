# Escc.AZ

The A-Z on www.eastsussex.gov.uk is a polyhierarchical list of services with headings that can be cross-referenced or redirected.

## Integration with the shared A-Z project
This project was designed to integrate with the Access East Sussex A-Z project at www.accesseastsussex.org, importing and exporting data according to a shared XML schema, but supporting a richer set of information in the East Sussex County Council A-Z. Services imported from the Access East Sussex A-Z were categorised automatically based on their IPSV terms. 

The Access East Sussex A-Z included services from East Sussex County Council and all 5 district and borough councils in East Sussex. However, the Access East Sussex A-Z is now obsolete, therefore so too are the importer and exporter projects in this solution.

## Database permissions

For the reader connection, create a User with Execute permissions on the following stored procedures:

* `usp_SelectHeadingsByIndex`
* `usp_SelectHeadingsBySearch`
* `usp_SelectServicesForExport`
* `usp_SelectServicesForHeading`
* `usp_UrlSelectForms`
* `usp_UrlSelectPopularForms`
* `usp_UrlSelectForScanner`

For the writer connection, create a User with Execute permissions to all the other stored procedures, and `usp_UrlSelectPopularForms`.