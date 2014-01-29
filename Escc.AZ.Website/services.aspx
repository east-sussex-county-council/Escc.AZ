<%@ Page language="c#" Codebehind="Services.aspx.cs" AutoEventWireup="True" Inherits="Escc.AZ.Website.Services" EnableViewState="false" MasterPageFile="~/masterpages/EastSussexGovUK.master" %>
<asp:Content runat="server" ContentPlaceholderId="metadata">
	<Egms:MetadataControl id="headContent" runat="server"
		LgtLType="A to Z"
		DateCreated="2004-08-05"
		IpsvPreferredTerms="Local government"
	 />
    <Egms:Css Files="AtoZ" runat="server" />
</asp:Content>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="atoz" />

<asp:Content runat="server" ContentPlaceholderId="related">
	    <div class="first"><h2>Related pages</h2>
		    <ul class="last">
			    <li><a href="/atoz/forms/default.aspx">A&#8211;Z of forms</a></li>
		    </ul>
	    </div>
</asp:Content>

<asp:Content runat="server" ContentPlaceholderId="content">
	<div class="article">
    <article>
        <asp:placeholder id="content" runat="server" />

        <EastSussexGovUK:Related runat="server">
            <PagesTemplate>
		    <ul>
			    <li><a href="/atoz/forms/default.aspx">A&#8211;Z of forms</a></li>
		    </ul>
            </PagesTemplate>
        </EastSussexGovUK:Related>

        <EastSussexGovUK:Share runat="server" />
    </article>
    </div>
</asp:Content>
