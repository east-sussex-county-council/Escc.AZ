function OnCurrentPageLoad() 
{
	if (document.getElementById && document.getElementsByTagName)
	{
		// add warning to "remove heading" links
	    oServicesTable = document.getElementById('ctl00_content_tbody');
		
		if (oServicesTable)
		{
			aLinks = oServicesTable.getElementsByTagName('a');
			
			for (var i = 0; i < aLinks.length; i++)
			{
				if (aLinks[i].innerHTML == 'Delete') aLinks[i].onclick = serviceWarning;
			}
		}
	}
}

// warn user deleting a service
function serviceWarning()
{
	return confirm('This service will be deleted - this cannot be undone.\n\nAre you sure?');
}


$(OnCurrentPageLoad);
