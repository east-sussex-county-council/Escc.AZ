function OnCurrentPageLoad() 
{
	if (document.getElementById && document.getElementsByTagName)
	{
		// add warning to "remove heading" links
	    oHeadingsTable = document.getElementById('ctl00_content_tbody');
		
		if (oHeadingsTable)
		{
			aLinks = oHeadingsTable.getElementsByTagName('a');
			
			for (var i = 0; i < aLinks.length; i++)
			{
				if (aLinks[i].innerHTML == 'Delete') aLinks[i].onclick = headingWarning;
			}
		}
	}
}

// warn user deleting a heading
function headingWarning()
{
	return confirm('This heading will be deleted - this cannot be undone.\n\nAre you sure?');
}

$(OnCurrentPageLoad);
