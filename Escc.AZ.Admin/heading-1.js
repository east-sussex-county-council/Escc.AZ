function OnCurrentPageLoad() 
{
	// give focus to title
	if (document.getElementById) 
	{
	    document.getElementById('ctl00_content_headingTitle').focus(); 
	
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

			// add warning to "remove service" links
			oServicesTable = document.getElementById('ctl00_content_services');
			
			if (oServicesTable)
			{
				aLinks = oServicesTable.getElementsByTagName('a');
				
				for (var i = 0; i < aLinks.length; i++)
				{
					if (aLinks[i].innerHTML == 'Remove') aLinks[i].onclick = serviceWarning;
				}
			}
		}
		
		// disable service section is heading redirected
		disableServices(document.getElementById('ctl00_content_redirectUrl'));
		
		// add event handlers
		document.getElementById('aspnetForm').onsubmit = redirectWarning;
		document.getElementById('ctl00_content_redirectUrl').onkeyup = disableServices;
	}
}

// warn user removing a heading
function headingWarning()
{
	if (document.getElementById)
	{
	    sCurrentHeading = document.getElementById('ctl00_content_headingTitle').value;
		return confirm('This heading will be removed from \'' + sCurrentHeading + '\' only.\n\nIt will still be available and may appear in the A-Z.\n\nAre you sure?');
	}
}

// warn user removing a service
function serviceWarning()
{
	if (document.getElementById)
	{
	    sCurrentHeading = document.getElementById('ctl00_content_headingTitle').value;
		return confirm('This service will be removed from \'' + sCurrentHeading + '\' only.\n\nIt will still be available and may be related to other headings.\n\nAre you sure?');
	}
}

// warn user redirecting a heading
function redirectWarning()
{
	if (document.getElementById)
	{
		// check whether a redirect url has been added
	    oUrl = document.getElementById('ctl00_content_redirectUrl');
	    oOriginalUrl = document.getElementById('ctl00_content_originalRedirectUrl');

	    if (oUrl && oOriginalUrl && oUrl.value.length > 0 && oOriginalUrl.value.length == 0 && document.getElementById('ctl00_content_services'))
		{
			// heading newly redirected
			bResult = confirm('You have chosen to redirect this heading.\n\nAll related services will be removed. However, removed services will still be available and may be related to other headings.\n\nAre you sure?'); 
			if (bResult) 
			{
				return true; 
			}
			else
			{
				oUrl.select();
				return false;
			}
		}
		else return true;
	}
}

// if heading is being redirect, there's no point having services - so disable the section
function disableServices(oElement)
{
	if (document.getElementById)
	{
		// allow calling as event handler or standalone function
		if (oElement == null) oElement = this;
		
		if (oElement.value.length > 0)
		{
			// disable
		    document.getElementById('ctl00_content_possibleServices').disabled = true;
		    document.getElementById('servicesFieldset').className = "disabled";
		
		}
		else
		{
			// enable
		    document.getElementById('ctl00_content_possibleServices').disabled = false;
		    document.getElementById('servicesFieldset').className = "";
		}
	}
}

$(OnCurrentPageLoad);
