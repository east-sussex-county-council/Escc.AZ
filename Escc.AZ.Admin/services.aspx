<%@ Page language="c#" Codebehind="services.aspx.cs" Inherits="Escc.AZ.Admin.services" EnableViewState="false" %>

<asp:Content runat="server" ContentPlaceHolderID="metadata">
	<Egms:MetadataControl id="headContent" runat="server" 
		Title="Manage A-Z services"
		DateCreated="2004-05-24"
		IpsvPreferredTerms="Local government"
		 />
    <Egms:Css runat="server" Files="AtozManage" />
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="content">
<div class="full-page">
<section>
<div class="text">
	<h1>Manage A&#8211;Z services</h1>
	<div id="azBox" runat="server"></div>
	<p><a href="service.aspx">Add a new service&#8230;</a></p>
	<table summary="Services beginning with the currently selected letter" class="itemManager">
	<thead><tr><th scope="col">Service</th><th scope="col">Authority</th><th scope="col">&nbsp;</th></tr></thead>
	<tbody id="tbody" runat="server"></tbody>
	</table>
	<p><a href="service.aspx">Add a new service&#8230;</a></p>
</div>
</section>
</div>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="javascript">
	<script type="text/javascript" src="services-1.js"></script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="supporting" />