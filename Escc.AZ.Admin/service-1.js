function OnCurrentPageLoad() 
{
	if (document.getElementById)
	{
	    if (document.getElementById('ctl00_content_serviceTitle').disabled == false)
		{
		    document.getElementById('ctl00_content_serviceTitle').focus(); 
		}

		if (document.getElementsByTagName)
		{
			// add warning to "remove heading" links
		    oHeadingsTable = document.getElementById('ctl00_content_relatedHeadings');
			
			if (oHeadingsTable)
			{
				aLinks = oHeadingsTable.getElementsByTagName('a');

				for (var i = 0; i < aLinks.length; i++)
				{
					if (aLinks[i].innerHTML == 'Remove') aLinks[i].onclick = headingWarning;
				}
			}

			// add warning to "delete link" links
			oUrlsTable = document.getElementById('ctl00_content_relatedUrls');
			
			if (oUrlsTable)
			{
				aLinks = oUrlsTable.getElementsByTagName('a');
				
				for (var i = 0; i < aLinks.length; i++)
				{
					if (aLinks[i].innerHTML == 'Delete') aLinks[i].onclick = urlWarning;
				}
			}

			// add warning to "delete link" links
			oContactsTable = document.getElementById('ctl00_content_contacts');
			
			if (oContactsTable)
			{
				aLinks = oContactsTable.getElementsByTagName('a');
				
				for (var i = 0; i < aLinks.length; i++)
				{
					if (aLinks[i].innerHTML == 'Delete') aLinks[i].onclick = contactWarning;
				}
			}
		}
	}
}

// warn user removing a heading
function headingWarning()
{
	if (document.getElementById)
	{
	    sCurrentService = document.getElementById('ctl00_content_serviceTitle').value;
		return confirm('The \'' + sCurrentService + '\' service will be removed from this heading.\n\nThe heading will still be available and may appear in the A-Z.\n\nAre you sure?');
	}
}

// warn user deleting a link
function urlWarning()
{
	return confirm('This link will be deleted - this cannot be undone.\n\nAre you sure?');
}

// warn user deleting a contact
function contactWarning()
{
	return confirm('This contact will be deleted - this cannot be undone.\n\nAre you sure?');
}

$(OnCurrentPageLoad);
