<%@ Page language="c#" Codebehind="orphans.aspx.cs" AutoEventWireup="True" Inherits="Escc.AZ.Admin.orphans" EnableViewState="false" %>

<asp:Content runat="server" ContentPlaceHolderID="metadata">
	<Metadata:MetadataControl id="headContent" runat="server" 
		Title="Manage A-Z services" 
		DateCreated="2004-05-24"
		IpsvPreferredTerms="Local government"
		/>
</asp:Content>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="atoz" />
<asp:Content ID="Content2" runat="server" ContentPlaceHolderID="feedback" />

<asp:Content runat="server" ContentPlaceHolderID="content">
<div class="full-page">
    <section>
        <div class="content text-content">
            <h1>Orphaned A&#8211;Z services</h1>
            <p>These services have <strong>no heading</strong>, and <strong>will not be displayed</strong> on www.eastsussex.gov.uk. They <strong>will display</strong> on other authorities' sites.</p>
            <table summary="Services beginning with the currently selected letter" class="itemManager">
            <thead><tr><th scope="col">Service</th><th scope="col"><acronym title="Integrated Public Sector Vocabulary">IPSV</acronym> terms</th><th scope="col">Authority</th><th scope="col">&nbsp;</th></tr></thead>
            <tbody id="tbody" runat="server"></tbody>
            </table>
        </div>
    </section>
</div>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="javascript">
	<script type="text/javascript" src="services.js"></script>
</asp:Content>
