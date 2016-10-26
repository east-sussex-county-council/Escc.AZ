<%@ Page language="c#" Codebehind="contact.aspx.cs" AutoEventWireup="True" Inherits="Escc.AZ.Admin.contact" EnableViewState="false" %>

<asp:Content runat="server" ContentPlaceHolderID="metadata">
	<Metadata:MetadataControl id="headContent" runat="server"
		Title="Manage A-Z contact"
		DateCreated="2004-05-24"
		IpsvPreferredTerms="Local government"
	 />
    <EastSussexGovUK:ContextContainer runat="server" Desktop="true">
    <ClientDependency:Css runat="server" Files="FormsSmall;AtoZManage" />
        <ClientDependency:Css runat="server" Files="FormsMedium" MediaConfiguration="Medium" />
        <ClientDependency:Css runat="server" Files="FormsLarge" MediaConfiguration="Large" />
    </EastSussexGovUK:ContextContainer>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="content">
<div class="full-page">
<section>
<div class="content text-content">

	<h1 id="h1" runat="server">Manage A–Z contact</h1>
	
	<asp:validationsummary runat="server" displaymode="BulletList" showsummary="True" enableclientscript="False" headertext="Contact not saved:" />

    <div class="form service-form">

	    <h2>Name and description</h2>
        <div class="formBox">

		    <div class="formPart">
		    <div class="formLabel"><asp:label runat="server" associatedcontrolid="firstName">First name</asp:label></div>
		    <asp:textbox id="firstName" runat="server" maxlength="35" cssclass="formControl"></asp:textbox>
		    <asp:regularexpressionvalidator id="firstNameLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="firstName" errormessage="Your first name is too long: it must be 35 characters or fewer" validationexpression=".{1,35}"></asp:regularexpressionvalidator>
		    <asp:customvalidator id="firstNameHasLastNameValidator" runat="server" display="None" enableclientscript="False" controltovalidate="firstName" errormessage="When you add a first name, add a last name too" onservervalidate="CheckFirstNameHasLastName"></asp:customvalidator>
		    </div>
		
		    <div class="formPart">
		    <div class="formLabel"><asp:label runat="server" associatedcontrolid="lastName">Last name</asp:label></div>
		    <asp:textbox id="lastName" runat="server" maxlength="35" cssclass="formControl"></asp:textbox>
		    <asp:regularexpressionvalidator id="lastNameLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="lastName" errormessage="Your last name is too long: it must be 35 characters or fewer" validationexpression=".{1,35}"></asp:regularexpressionvalidator>
		    <asp:customvalidator id="lastNameHasFirstNameValidator" runat="server" display="None" enableclientscript="False" controltovalidate="lastName" errormessage="When you add a last name, add a first name too" onservervalidate="CheckLastNameHasFirstName"></asp:customvalidator>
		    </div>
		
		    <div class="formPart">
		    <div class="formLabel"><asp:label runat="server" associatedcontrolid="description">Description</asp:label></div>
		    <asp:textbox id="description" runat="server" textmode="MultiLine" maxlength="255" cssclass="formControl"></asp:textbox>
		    <asp:regularexpressionvalidator id="descriptionLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="description" errormessage="Your description is too long: it must be 255 characters or fewer" validationexpression="[\w\W]{1,255}"></asp:regularexpressionvalidator>
		    </div>

        </div>
	    <h2>Phone, fax and email</h2>
        <div class="formBox">
	
		    <div class="formPart">
		    <div class="formLabel"><asp:label runat="server" associatedcontrolid="phoneArea">Phone number</asp:label></div>
		    <div class="formControl">
		    <asp:textbox id="phoneArea" runat="server" maxlength="5" cssclass="phone phoneFragment" tooltip="Area code"></asp:textbox>
		    <asp:regularexpressionvalidator id="phoneAreaValidator" runat="server" display="None" enableclientscript="False" controltovalidate="phoneArea" errormessage="The phone area code must be 5 digits or fewer, with no punctuation" validationexpression="[0-9]{1,5}"></asp:regularexpressionvalidator>
		    <asp:customvalidator id="phoneAreaHasPhoneValidator" runat="server" display="None" enableclientscript="False" controltovalidate="phoneArea" errormessage="The phone area code must have a phone number" onservervalidate="CheckPhoneAreaHasPhone"></asp:customvalidator>
		
		    <asp:textbox id="phone" runat="server" maxlength="9" cssclass="phone phoneFragment" tooltip="Phone number"></asp:textbox>
		    <asp:regularexpressionvalidator id="phoneValidator" runat="server" display="None" enableclientscript="False" controltovalidate="phone" errormessage="The phone number must be 9 digits or fewer, with no punctuation" validationexpression="[0-9 ]{1,9}"></asp:regularexpressionvalidator>
		    <asp:customvalidator id="phoneHasAreaValidator" runat="server" display="None" enableclientscript="False" controltovalidate="phone" errormessage="The phone number must have an area code" onservervalidate="CheckPhoneHasArea"></asp:customvalidator>
		
		    <asp:label runat="server" associatedcontrolid="phoneExtension" class="phoneFragment"><abbr title="Extension">ext</abbr></asp:label>
		    <asp:textbox id="phoneExtension" runat="server" maxlength="8" cssclass="phone phoneFragment" tooltip="Extension number"></asp:textbox>
		    <asp:regularexpressionvalidator id="phoneExtensionValidator" runat="server" display="None" enableclientscript="False" controltovalidate="phoneExtension" errormessage="The phone extension must be 8 digits or fewer, with no punctuation" validationexpression="[0-9]{1,8}"></asp:regularexpressionvalidator>
		    <asp:customvalidator id="phoneExtensionRequiresPhoneValidator" runat="server" display="None" enableclientscript="False" controltovalidate="phoneExtension" errormessage="The phone extension must have an area code and phone number" onservervalidate="CheckPhoneExtensionHasPhone"></asp:customvalidator>
		    </div>
		    </div>
	
		    <div class="formPart">
		    <div class="formLabel"><asp:label runat="server" associatedcontrolid="faxArea">Fax number</asp:label></div>
		    <div class="formControl">
		    <asp:textbox id="faxArea" runat="server" maxlength="5" cssclass="phone phoneFragment" tooltip="Fax area code"></asp:textbox>
		    <asp:regularexpressionvalidator id="faxAreaValidator" runat="server" display="None" enableclientscript="False" controltovalidate="faxArea" errormessage="The fax area code must be 5 digits or fewer, with no punctuation" validationexpression="[0-9]{1,5}"></asp:regularexpressionvalidator>
		    <asp:customvalidator id="faxAreaHasFaxValidator" runat="server" display="None" enableclientscript="False" controltovalidate="faxArea" errormessage="The fax area code must have a fax number" onservervalidate="CheckFaxAreaHasFax"></asp:customvalidator>

		    <asp:textbox id="fax" runat="server" maxlength="8" cssclass="phone phoneFragment" tooltip="Fax number"></asp:textbox>
		    <asp:regularexpressionvalidator id="faxValidator" runat="server" display="None" enableclientscript="False" controltovalidate="fax" errormessage="The fax number must be 8 digits or fewer, with no punctuation" validationexpression="[0-9]{1,8}"></asp:regularexpressionvalidator>
		    <asp:customvalidator id="faxHasAreaValidator" runat="server" display="None" enableclientscript="False" controltovalidate="fax" errormessage="The fax number must have an area code" onservervalidate="CheckFaxHasArea"></asp:customvalidator>

		    <asp:label runat="server" associatedcontrolid="faxExtension" class="phoneFragment"><abbr title="Extension">ext</abbr></asp:label>
		    <asp:textbox id="faxExtension" runat="server" maxlength="8" cssclass="phone phoneFragment" tooltip="Extension number"></asp:textbox>
		    <asp:regularexpressionvalidator id="faxExtensionValidator" runat="server" display="None" enableclientscript="False" controltovalidate="faxExtension" errormessage="The fax extension must be 8 digits or fewer, with no punctuation" validationexpression="[0-9]{1,8}"></asp:regularexpressionvalidator>
		    <asp:customvalidator id="faxExtensionRequiresFaxValidator" runat="server" display="None" enableclientscript="False" controltovalidate="faxExtension" errormessage="The fax extension must have an area code and fax number" onservervalidate="CheckFaxExtensionHasFax"></asp:customvalidator>
		    </div>
		    </div>

		    <div class="formPart">
		    <div class="formLabel"><asp:label runat="server" associatedcontrolid="email">Email address</asp:label></div>
		    <asp:textbox id="email" runat="server" maxlength="255" cssclass="formControl"></asp:textbox>
		    <asp:regularexpressionvalidator id="emailLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="email" errormessage="The email address is too long: it must be 255 characters or fewer" validationexpression=".{1,255}"></asp:regularexpressionvalidator>
		    <asp:regularexpressionvalidator id="emailValidator" runat="server" display="None" enableclientscript="False" controltovalidate="email" errormessage="Please enter a valid email address" validationexpression="[^\s:]+@[^\s]+\.[^\s]+"></asp:regularexpressionvalidator>
		    </div>

		    <div class="formPart">
		    <div class="formLabel"><asp:label runat="server" associatedcontrolid="emailText">Recipient's name</asp:label></div>
		    <asp:textbox id="emailText" runat="server" maxlength="75" cssclass="formControl"></asp:textbox>
		    <asp:regularexpressionvalidator id="emailTextLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="emailText" errormessage="The email recipient's name is too long: it must be 75 characters or fewer" validationexpression=".{1,75}"></asp:regularexpressionvalidator>
		    <asp:customvalidator id="emailTextRequiresEmailValidator" runat="server" display="None" enableclientscript="False" controltovalidate="emailText" errormessage="Please add an email address for the email recipient" onservervalidate="CheckForEmailAddress"></asp:customvalidator>
		    </div>

        </div>
	    <h2>Postal address</h2>
        <div class="formBox">

		    <!-- NOTE: validators for SAON and PAON are based on BS7666 schema, but adapted because we're not using the first ten characters correctly yet -->
		
		    <div class="formPart">
		    <asp:label runat="server" associatedcontrolid="saon" class="formLabel">Room, floor or unit</asp:label>
		    <asp:textbox id="saon" runat="server" maxlength="90" cssclass="formControl"></asp:textbox>
		    <asp:regularexpressionvalidator id="saonLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="saon" errormessage="The room, floor or unit is too long: it must be 90 characters or fewer" validationexpression=".{1,90}"></asp:regularexpressionvalidator>
		    <asp:regularexpressionvalidator id="saonRegexValidator" runat="server" display="None" enableclientscript="False" controltovalidate="saon" errormessage="The room, floor or unit includes characters which are not allowed" validationexpression="^([a-zA-Z0-9:;&lt;=&gt;\?@%&amp;'\(\)\*\+,\-\. ]{0,90})?$"></asp:regularexpressionvalidator>
		    <asp:customvalidator id="saonRequiresTownValidator" runat="server" display="None" enableclientscript="False" controltovalidate="saon" errormessage="When entering a room, floor or unit, specify the town and county" onservervalidate="CheckTownAndCounty"></asp:customvalidator>
		    </div>
		
		    <div class="formPart">
		    <asp:label runat="server" associatedcontrolid="paon" class="formLabel">Building name/number</asp:label>
		    <asp:textbox id="paon" runat="server" maxlength="90" cssclass="formControl"></asp:textbox>
		    <asp:regularexpressionvalidator id="paonLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="paon" errormessage="The building name/number is too long: it must be 90 characters or fewer" validationexpression=".{1,90}"></asp:regularexpressionvalidator>
		    <asp:regularexpressionvalidator id="paonRegexValidator" runat="server" display="None" enableclientscript="False" controltovalidate="paon" errormessage="The building name/number includes characters which are not allowed" validationexpression="^([a-zA-Z0-9:;&lt;=&gt;\?@%&amp;'\(\)\*\+,\-\. ]{0,90})?$"></asp:regularexpressionvalidator>
		    <asp:customvalidator id="paonRequiresTownValidator" runat="server" display="None" enableclientscript="False" controltovalidate="paon" errormessage="When entering a building name/number, specify the town and county" onservervalidate="CheckTownAndCounty"></asp:customvalidator>
		    </div>

		    <div class="formPart">
		    <asp:label runat="server" associatedcontrolid="street" class="formLabel">Street</asp:label>
		    <asp:textbox id="street" runat="server" maxlength="100" cssclass="formControl"></asp:textbox>
		    <asp:regularexpressionvalidator id="streetLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="street" errormessage="The street is too long: it must be 100 characters or fewer" validationexpression=".{1,100}"></asp:regularexpressionvalidator>
		    <asp:customvalidator id="streetRequiresTownValidator" runat="server" display="None" enableclientscript="False" controltovalidate="street" errormessage="When entering a street, specify the town and county" onservervalidate="CheckTownAndCounty"></asp:customvalidator>
		    </div>

		    <div class="formPart">
		    <asp:label runat="server" associatedcontrolid="locality" class="formLabel">Village or part of town</asp:label>
		    <asp:textbox id="locality" runat="server" maxlength="100" cssclass="formControl"></asp:textbox>
		    <asp:regularexpressionvalidator id="localityLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="locality" errormessage="The village or part of town is too long: it must be 35 characters or fewer" validationexpression=".{1,35}"></asp:regularexpressionvalidator>
		    <asp:customvalidator id="localityRequiresTownValidator" runat="server" display="None" enableclientscript="False" controltovalidate="locality" errormessage="When entering a village or part of town, specify the town and county" onservervalidate="CheckTownAndCounty"></asp:customvalidator>
		    </div>

		    <div class="formPart">
		    <div class="formLabel"><asp:label runat="server" associatedcontrolid="town">Town</asp:label></div>
		    <asp:textbox id="town" runat="server" maxlength="30" cssclass="formControl"></asp:textbox>
		    <asp:regularexpressionvalidator id="townLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="town" errormessage="The town name is too long: it must be 30 characters or fewer" validationexpression=".{1,30}"></asp:regularexpressionvalidator>
		    <asp:customvalidator id="townRequiresStreetAddressValidator" runat="server" display="None" enableclientscript="False" controltovalidate="town" errormessage="Fill in the street address when specifying a town" onservervalidate="CheckForStreetAddress"></asp:customvalidator>
		    </div>
		
		    <div class="formPart">
		    <div class="formLabel"><asp:label runat="server" associatedcontrolid="county">County</asp:label></div>
		    <asp:dropdownlist id="county" runat="server" cssclass="formControl">
		    <asp:listitem></asp:listitem>
		    <asp:listitem value="East Sussex">East Sussex</asp:listitem>
		    <asp:listitem value="Hampshire">Hampshire</asp:listitem>
		    <asp:listitem value="Kent">Kent</asp:listitem>
		    <asp:listitem value="Nottinghamshire">Nottinghamshire</asp:listitem>
		    <asp:listitem value="Surrey">Surrey</asp:listitem>
		    <asp:listitem value="West Sussex">West Sussex</asp:listitem>
		    </asp:dropdownlist>

		    <asp:regularexpressionvalidator id="countyLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="county" errormessage="The county name is too long: it must be 30 characters or fewer" validationexpression=".{1,30}"></asp:regularexpressionvalidator>
		    <asp:customvalidator id="countyRequiresStreetAddressValidator" runat="server" display="None" enableclientscript="False" controltovalidate="county" errormessage="Fill in the street address when specifying a county" onservervalidate="CheckForStreetAddress"></asp:customvalidator>
		    </div>
		
		    <div class="formPart">
		    <div class="formLabel"><asp:label runat="server" associatedcontrolid="postcode">Postcode</asp:label></div>
		    <div class="formControl"><asp:textbox id="postcode" runat="server" maxlength="9" cssclass="postcode"></asp:textbox></div>
		    <asp:regularexpressionvalidator id="postcodeValidator" runat="server" display="None" enableclientscript="False" controltovalidate="postcode" errormessage="Please enter a valid postcode" validationexpression="[A-Z]{1,2}[0-9R][0-9A-Z]? [0-9][A-Z]{2}"></asp:regularexpressionvalidator>
		    </div>

        </div>

	    <!-- AddressUrl hidden because ESCC isn't using these fields; only Hastings is -->
	    <input type="hidden" id="addressUrl" runat="server" name="addressUrl" />
	    <input type="hidden" id="addressUrlText" runat="server" name="addressUrlText" />
	
	    <input type="hidden" id="contactId" runat="server" name="contactId" />
	    <input type="hidden" id="serviceId" runat="server" name="serviceId" />
	    <div class="formButtons"><FormControls:EsccButton id="submit" runat="server" text="Save" CssClass="button" onclick="submit_Click" /></div>
    </div>
</div>
</section>
</div>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="javascript">
    <script type="text/javascript" src="contact-1.js"></script>
</asp:Content>