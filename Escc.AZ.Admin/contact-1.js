// set up event handlers when page loads, and place cursor
function OnCurrentPageLoad()
{
	if (document.getElementById)
	{
	    document.getElementById('ctl00_content_phoneArea').onkeyup = AreaCode;
	    document.getElementById('ctl00_content_faxArea').onkeyup = AreaCode;

	    document.getElementById('ctl00_content_paon').onkeyup = SelectCounty;
	    document.getElementById('ctl00_content_saon').onkeyup = SelectCounty;
	    document.getElementById('ctl00_content_street').onkeyup = SelectCounty;
	    document.getElementById('ctl00_content_locality').onkeyup = SelectCounty;
	    document.getElementById('ctl00_content_town').onkeyup = SelectCounty;

	    document.getElementById('ctl00_content_firstName').focus();
	}
}

// if there's text in the specified field, but no county - pre-select East Sussex
function SelectCounty()
{
	if (document.getElementById)
	{
		oCounty = document.getElementById('county');
		
		if (this.value.length > 0 && oCounty.selectedIndex == 0)
		{
			for (var i = 0; i < oCounty.length; i++)
			{
				if (oCounty.options[i].value == 'East Sussex')
				{
					oCounty.options[i].selected = true;
					break;
				}
			}	
		}
	}
}

// a five-figure area code is complete, so move to the next field
function AreaCode()
{
	if (document.getElementById && this.value.length == 5) 
	{
		sNextFieldId = this.id.substring(0, this.id.length-4);
		oNextField = document.getElementById(sNextFieldId);
		if (oNextField != null) oNextField.focus();
	}
}

$(OnCurrentPageLoad);
