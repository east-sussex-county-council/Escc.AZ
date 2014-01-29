/**
* Submit the form to update the listing whenever a council is (de)selected
* @returns void
*/
function AZSearch_OnCheckboxChange()
{
	this.form.submit();
}


/**
* Create a checkbox to enable/disable the autopostback options.
* Create using JavaScript because it's not relevant if JavaScript is unavailable.
* @returns void
*/
function AZSearch_CreateDisableOption()
{
	if (navigator.userAgent.indexOf('MSIE 5.0') > -1) return; // IE5.0 just makes a mess
	
	if (document.createElement && document.createTextNode && document.getElementById)
	{
		var boxId = 'autoPostback';
		
		var checkbox = document.createElement('input');
		checkbox.setAttribute('type', 'checkbox');
		checkbox.setAttribute('id', boxId);
		checkbox.setAttribute('name', boxId);
		checkbox.onclick = AZSearch_SwitchAutoPostback;

		var label = document.createElement('label');
		label.setAttribute('for', boxId);
		label.className = 'aural';
		label.appendChild(checkbox);
		label.appendChild(document.createTextNode('Automatically reload this page when you change search options'));

		var dynamicOption = document.getElementById('ctl00_content_councilList');
		dynamicOption.parentNode.insertBefore(label, dynamicOption);
		
		// Select if not postback, or if box was selected at postback
		// Wait until now as IE won't recognise .checked until control added to page
		if (document.URL.indexOf('?azq=') == -1 || document.URL.indexOf(boxId) > -1)
		{
            document.getElementById(boxId).checked = true;
		}
		
		AZSearch_SwitchAutoPostback();
	}
}

/**
* Get an array of checkboxes for selecting the councils to include
* @returns HtmlInput[]
*/
function AZSearch_GetCouncilBoxes()
{
	if (document.getElementById)
	{
	    var aBoxes = new Array(document.getElementById('ctl00_content_acc'), document.getElementById('ctl00_content_ae'), document.getElementById('ctl00_content_ah'), document.getElementById('ctl00_content_al'), document.getElementById('ctl00_content_ar'), document.getElementById('ctl00_content_aw'));
		return aBoxes;
	}
	else return null;
}


/**
* If autopostback option is selected, add event handlers to checkboxes and radio buttons. If not, remove event handlers.
* @returns void
*/
function AZSearch_SwitchAutoPostback()
{
	if (document.getElementById)
	{
		if ($('#autoPostback').is(":checked"))
		{
			var aBoxes = AZSearch_GetCouncilBoxes();
			if (aBoxes)
			{
				for (var i = 0; i < aBoxes.length; i++)
				{
					aBoxes[i].onclick = AZSearch_OnCheckboxChange;
				}
			}
		}
		else
		{
			var aBoxes = AZSearch_GetCouncilBoxes();
			if (aBoxes)
			{
				for (var i = 0; i < aBoxes.length; i++)
				{
					aBoxes[i].onclick = null;
				}
			}
		}
	}
}

$(AZSearch_CreateDisableOption);