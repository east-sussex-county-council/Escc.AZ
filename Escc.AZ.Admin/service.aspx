<%@ Page language="c#" Codebehind="service.aspx.cs" AutoEventWireup="True" Inherits="Escc.AZ.Admin.service" EnableViewState="false" %>

<asp:Content runat="server" ContentPlaceHolderID="metadata">
	<Metadata:MetadataControl id="headContent" runat="server"
		Title="Manage A-Z service"
		DateCreated="2004-05-24"
		IpsvPreferredTerms="Local government"
	 />
    <ClientDependency:Css runat="server" Files="FormsSmall;AtoZManage" />
    <EastSussexGovUK:ContextContainer runat="server" Desktop="true">
        <ClientDependency:Css runat="server" Files="FormsMedium" MediaConfiguration="Medium" />
        <ClientDependency:Css runat="server" Files="FormsLarge" MediaConfiguration="Large" />
    </EastSussexGovUK:ContextContainer>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="content">
<div class="full-page">
    <section>
        <div class="text">
	        <h1 id="h1" runat="server">Manage A–Z service</h1>
	
	        <asp:validationsummary runat="server" displaymode="BulletList" showsummary="True" enableclientscript="False" headertext="Service not saved:" />

            <div class="form service-form">
	            <FormControls:EsccButton id="saveTop" runat="server" Text="Save" onclick="submit_Click" cssclass="aural" />

	            <h2 class="first-child">About this service</h2>
                <div class="formBox">

		            <div class="formPart">
		            <asp:label runat="server" AssociatedControlID="serviceTitle" cssclass="formLabel">Title</asp:label>
		            <asp:textbox id="serviceTitle" runat="server" maxlength="250" cssclass="formControl"></asp:textbox>
		            <asp:requiredfieldvalidator id="serviceTitleValidator" runat="server" display="None" enableclientscript="False" controltovalidate="serviceTitle" errormessage="Please add a title"></asp:requiredfieldvalidator>
		            <asp:regularexpressionvalidator id="serviceTitleLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="serviceTitle" errormessage="Your title is too long: it must be 250 characters or fewer" validationexpression=".{1,250}"></asp:regularexpressionvalidator>
		            </div>
		
		            <div class="formPart">
		            <asp:label runat="server" AssociatedControlID="description" class="formLabel">Description</asp:label>
		            <asp:textbox id="description" runat="server" textmode="MultiLine" maxlength="2500" cssclass="formControl"></asp:textbox>
		            <asp:requiredfieldvalidator id="descriptionValidator" runat="server" display="None" enableclientscript="False" controltovalidate="description" errormessage="Please add a description"></asp:requiredfieldvalidator>
		            <asp:customvalidator id="descriptionWordsValidator" runat="server" display="None" enableclientscript="False" controltovalidate="description" errormessage="Your description is too long: it must be 60 words or fewer" onservervalidate="CheckDescriptionWords"></asp:customvalidator>
		            <asp:regularexpressionvalidator id="descriptionLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="description" errormessage="Your description is too long: it must be 2500 characters or fewer" validationexpression="[\w\W]{1,2500}"></asp:regularexpressionvalidator>
		            </div>
	
		            <p class="editHelp">Separate <strong>multiple keywords</strong> with semi-colons</p>
		            <div class="formPart">
		            <asp:label runat="server" AssociatedControlID="keywords" class="formLabel">Free text keywords</asp:label>
		            <asp:textbox id="keywords" runat="server" maxlength="250" cssclass="formControl"></asp:textbox>
		            <asp:requiredfieldvalidator id="keywordsValidator" runat="server" display="None" enableclientscript="False" controltovalidate="keywords" errormessage="Please add free text keywords for your service"></asp:requiredfieldvalidator>
		            <asp:regularexpressionvalidator id="keywordsLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="keywords" errormessage="You have too many keywords: you must use 250 characters or fewer" validationexpression=".{1,250}"></asp:regularexpressionvalidator>
		            </div>
	            </div>

	            <h2>Headings &#8211; where this service is listed</h2>
                <div class="formBox">

	                <table id="relatedHeadings" runat="server" summary="Headings to which this service is already related" class="itemManager"></table>

	                <asp:placeholder id="ipsvInfo" runat="server">
	                <p class="fyi"><abbr title="Integrated Public Sector Vocabulary">IPSV</abbr> term(s) used to match headings</p>
	                <ul id="ipsvTerms" runat="server" class="fyi"></ul>
	                </asp:placeholder>
	
	                <div class="formPart" id="editHeadings" runat="server">
	                <asp:label runat="server" AssociatedControlID="possibleRelatedHeadings" class="formLabel">Add heading</asp:label>
	                <asp:dropdownlist id="possibleRelatedHeadings" runat="server" cssclass="formControl" />
	                </div>
                </div>

	            <h2>Links</h2>
                <div class="formBox">
	
    	            <p class="editHelp">The <strong>first link</strong> will also appear on <strong>www.accesseastsussex.org</strong></p>

		            <asp:placeholder id="currentUrls" runat="server"></asp:placeholder>
		
		            <asp:placeholder id="editUrls" runat="server">
		            <fieldset class="azUrlEdit">
			            <legend>Add a link</legend>
			            <div class="azUrlData">
				            <div class="formPart">
				            <asp:label runat="server" AssociatedControlID="url" class="formLabel">Link</asp:label>
				            <asp:textbox id="url" runat="server" maxlength="255" cssclass="formControl"></asp:textbox>
				            </div>

				            <div class="formPart">
				            <asp:label runat="server" AssociatedControlID="urlTitle" class="formLabel">Link text</asp:label>
				            <asp:textbox id="urlTitle" runat="server" maxlength="75" cssclass="formControl"></asp:textbox>
				            </div>

				            <div class="formPart">
				            <asp:label runat="server" AssociatedControlID="urlDescription" class="formLabel">Description</asp:label>
				            <asp:textbox id="urlDescription" runat="server" textmode="multiline" cssclass="formControl" />
				            </div>
			            </div>
		            </fieldset>
		            </asp:placeholder>
		
		            <input type="hidden" id="urlIds" runat="server" />
		
		            <!-- UrlValidator regex taken from AccessEastSussex A-Z XML schema -->
		            <asp:regularexpressionvalidator id="urlValidator" runat="server" display="None" enableclientscript="False" controltovalidate="url" errormessage="Please use a complete web address beginning with http://&#8230;" validationexpression="^(http|https|ftp)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&amp;%\$\-]+)*@)*((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|localhost|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{2}))(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\?\'\\\+&amp;%\$#\=~_\-]+))*$" />
		            <asp:regularexpressionvalidator id="urlLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="url" errormessage="Your link address is too long: it must be 255 characters or fewer" validationexpression=".{1,255}"></asp:regularexpressionvalidator>
		            <asp:regularexpressionvalidator id="urlTitleLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="urlTitle" errormessage="Your link text is too long: it must be 75 characters or fewer" validationexpression=".{1,75}"></asp:regularexpressionvalidator>
		            <asp:regularexpressionvalidator id="urlDescriptionLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="urlDescription" errormessage="Your link description is too long: it must be 300 characters or fewer" validationexpression=".{1,300}"></asp:regularexpressionvalidator>

	            </div>
	
	            <h2>Contacts</h2>
                <div class="formBox">

		            <p class="editHelp" id="noContactHelp" runat="server">To <strong>add a contact</strong>, first <strong>save</strong> this service</p>
		            <div id="contacts" runat="server"></div>
		
		            <p id="editContacts" runat="server">
		            <a id="newContactLink" runat="server" class="contentIndent">Add a new contact</a>
		            </p>
	
                </div>

	            <input type="hidden" id="serviceId" runat="server" name="serviceId" />
	            <div class="formButtons"><FormControls:EsccButton id="submit" runat="server" Text="Save" CssClass="button" onclick="submit_Click" /></div>
                </div>
            </div>
       </section>
    </div>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="javascript">
	<script type="text/javascript" src="service-1.js"></script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="supporting" />