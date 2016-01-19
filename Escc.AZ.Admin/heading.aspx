<%@ Page language="c#" Codebehind="heading.aspx.cs" AutoEventWireup="True" Inherits="Escc.AZ.Admin.heading" EnableViewState="false" %>

<asp:Content runat="server" ContentPlaceHolderID="metadata">
    <Metadata:MetadataControl id="headContent" runat="server"
		Title="Manage A-Z heading"
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
	        <h1 id="h1" runat="server">Manage A&#8211Z heading</h1>

	        <asp:validationsummary runat="server" displaymode="BulletList" showsummary="True" enableclientscript="False" headertext="Heading not saved:" />

            <div class="form service-form">

            <h2 class="first-child">About this heading</h2>
            <div class="formBox">

		        <div class="formPart">
		        <asp:label runat="server" AssociatedControlID="headingTitle" cssclass="formLabel">Heading</asp:label>
		        <asp:textbox runat="server" id="headingTitle" maxlength="255" cssclass="formControl" />
		        <asp:requiredfieldvalidator id="headingTitleValidator" runat="server" display="None" enableclientscript="False" controltovalidate="headingTitle" errormessage="Please add a title"></asp:requiredfieldvalidator>
		        <asp:regularexpressionvalidator id="headingLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="headingTitle" errormessage="Your title is too long: it must be 255 characters or fewer" validationexpression=".{1,255}"></asp:regularexpressionvalidator>
		        </div>
		
		        <div class="formPart">
		        <asp:label runat="server" AssociatedControlID="ipsvTerms" cssclass="formLabel"><acronym title="Integrated Public Sector Vocabulary">IPSV</acronym> preferred terms</asp:label>
		        <asp:textbox runat="server" id="ipsvTerms" cssclass="formControl" />
		        <asp:requiredfieldvalidator id="ipsvRequiredValidator" runat="server" display="None" enableclientscript="False" controltovalidate="ipsvTerms" errormessage="Please add IPSV preferred terms"></asp:requiredfieldvalidator>
		        <Metadata:EsdTermValidator id="ipsvValidator" runat="server" ControlledListName="IPSV" MultipleTerms="true" controltovalidate="ipsvTerms" />
		        </div>
            </div>
	        <h2>Redirect this heading</h2>
            <div class="formBox">
		        <p class="editHelp">This will <strong>redirect</strong> the heading elsewhere on the site, or to another site. If you enter a web address here, any headings added in 'See also' will not be displayed.</p>
		
		        <div class="formPart">
		        <asp:label runat="server" AssociatedControlID="redirectUrl" cssclass="formLabel">Web address</asp:label>
		        <asp:textbox runat="server" id="redirectUrl" maxlength="255" cssclass="formControl url" />
		        <input type="hidden" id="originalRedirectUrl" runat="server" />
		        <asp:regularexpressionvalidator id="urlValidator" runat="server" display="None" enableclientscript="False" controltovalidate="redirectUrl" errormessage="Please use a complete web address beginning with http://&#8230;" validationexpression="https?://[^\s]{6,}" />
		        <asp:regularexpressionvalidator id="urlLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="redirectUrl" errormessage="Your redirect address is too long: it must be 255 characters or fewer" validationexpression=".{1,255}"></asp:regularexpressionvalidator>
		        </div>

		        <div class="formPart">
		        <asp:label runat="server" AssociatedControlID="redirectTitle" cssclass="formLabel">Link title</asp:label>
		        <asp:textbox runat="server" id="redirectTitle" maxlength="255" cssclass="formControl" />
		        <asp:regularexpressionvalidator id="urlTitleLengthValidator" runat="server" display="None" enableclientscript="False" controltovalidate="redirectTitle" errormessage="Your redirect link title is too long: it must be 255 characters or fewer" validationexpression=".{1,255}"></asp:regularexpressionvalidator>
		        </div>
            </div>
	        <h2>See also: these headings</h2>
            <div class="formBox">
	        <ul class="editHelp">
                <li>If there are <strong>no</strong> services selected below, these headings will be "See" links.</li>
                <li>If there <strong>are</strong> services selected below, these headings will be "See also" links.</li>
            </ul>

	        <table id="relatedHeadings" runat="server" class="itemManager"></table>
	
	        <div class="formPart">
	        <asp:label runat="server" AssociatedControlID="possibleRelatedHeadings" cssclass="formLabel">Add heading</asp:label>
	        <asp:dropdownlist id="possibleRelatedHeadings" runat="server" cssclass="formControl" />
	        </div>
            </div>
	        <h2>Services listed under this heading</h2>
            <div  id="servicesFieldset">
            <div class="formBox">
	        <table id="services" runat="server" class="itemManager"></table>
	        <input type="hidden" id="sortEsccServices" runat="server" />
	
	        <div class="formPart">
	        <asp:label runat="server" AssociatedControlID="possibleServices" cssclass="formLabel">Add service</asp:label>
	        <asp:dropdownlist id="possibleServices" runat="server" cssclass="formControl"><asp:listitem></asp:listitem></asp:dropdownlist>
	        </div>
            </div></div>
	
	        <input type="hidden" id="headingId" runat="server" />
	        <div class="formButtons"><FormControls:EsccButton id="submit" runat="server" text="Save" CssClass="button" onclick="submit_Click" /></div>
                </div>
            </div>
       </section>
    </div>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="javascript">
	<script type="text/javascript" src="heading-1.js"></script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="supporting" />