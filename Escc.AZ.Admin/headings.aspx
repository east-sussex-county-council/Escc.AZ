<%@ Page language="c#" Codebehind="headings.aspx.cs" AutoEventWireup="True" Inherits="Escc.AZ.Admin.headings" EnableViewState="false" %>

<asp:Content runat="server" ContentPlaceHolderID="metadata">
	<Metadata:MetadataControl id="headContent" runat="server" 
		Title="Manage A-Z headings"
		DateCreated="2004-05-24"
		IpsvPreferredTerms="Local government"
		 />
    <ClientDependency:Css runat="server" Files="AtozManage;ContentSmall" />
    <EastSussexGovUK:ContextContainer runat="server" Desktop="true">
        <ClientDependency:Css runat="server" Files="ContentMedium" MediaConfiguration="Medium" />
        <ClientDependency:Css runat="server" Files="ContentLarge" MediaConfiguration="Large" />
    </EastSussexGovUK:ContextContainer>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="content">
<div class="full-page">
<section>
<div class="content text-content">
	<h1>Manage A&#8211;Z headings</h1>
	<div id="azBox" runat="server"></div>
	<p><a href="heading.aspx">Add a new heading&#8230;</a></p>
	<table summary="Headings beginning with the currently selected letter" class="itemManager">
	<thead><tr><th scope="col">Heading</th><th scope="col">Services</th><th scope="col">&nbsp;</th></tr></thead>
	<tbody id="tbody" runat="server"></tbody>
	</table>
	<p><a href="heading.aspx">Add a new heading&#8230;</a></p>
</div>
</section>
</div>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="javascript">
	<script type="text/javascript" src="headings-1.js"></script>
</asp:Content>