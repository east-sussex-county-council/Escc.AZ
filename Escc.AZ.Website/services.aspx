<%@ Page language="c#" Codebehind="Services.aspx.cs" AutoEventWireup="True" Inherits="Escc.AZ.Website.Services" EnableViewState="false" %>
<%@ Register tagPrefix="EastSussexGovUK" tagName="Share" src="~/share.ascx" %>
<asp:Content runat="server" ContentPlaceholderId="metadata">
	<Metadata:MetadataControl id="headContent" runat="server"
		LgtLType="A to Z"
		DateCreated="2004-08-05"
		IpsvPreferredTerms="Local government"
	 />
    <EastSussexGovUK:ContextContainer runat="server" Desktop="true">
        <ClientDependency:Css Files="AtoZ" runat="server" />
    </EastSussexGovUK:ContextContainer>
</asp:Content>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="atoz" />

<asp:Content runat="server" ContentPlaceholderId="content">
	<div class="article">
    <article>
        <asp:placeholder id="content" runat="server" />

    </article>
    </div>

    <div class="supporting related-links text-content content-small content-medium">
        <h2>Related pages</h2>
		<ul>
			<li><a href="<%= ResolveUrl("~/") %>forms/default.aspx">A&#8211;Z of forms</a></li>
		</ul>
    </div>
    
    <div class="content text-content">
    <EastSussexGovUK:Share runat="server" />
    </div>
</asp:Content>
