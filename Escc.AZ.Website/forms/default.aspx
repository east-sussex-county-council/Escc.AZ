<%@ Page language="c#" Codebehind="default.aspx.cs" AutoEventWireup="True" Inherits="Escc.AZ.Website.forms.Forms" EnableViewState="false" %>
<asp:Content runat="server" ContentPlaceholderId="metadata">
	<Metadata:MetadataControl id="headContent" runat="server"
		Title="A-Z of forms: {0}"
        Keywords="a-z, index, search, contents, find, forms, application, application form"
		LgtLType="A to Z"
		IpsvPreferredTerms="Local government"
		DateCreated="2005-11-03"
		Description="Find forms for services provided by East Sussex County Council"
		 />
    <ClientDependency:Css runat="server" Files="AtoZ" />
</asp:Content>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="atoz" />

<asp:Content runat="server" ContentPlaceholderId="related">
	<div class="first"><h2>Related pages</h2>
		<ul class="last">
			<li><a href="/atoz/default.aspx">A&#8211;Z of services</a></li>
		</ul>
	</div>
</asp:Content>

<asp:Content runat="server" ContentPlaceholderId="content">
    <div class="article">
    <article>
        <div class="text">
    	    <h1>A&#8211;Z of forms</h1>

	        <p>Apply for services provided by East Sussex County Council.</p>

	        <div class="roundedBox azNav"><div><div><div>
	        <NavigationControls:AZNavigation id="navigation" runat="server" TargetFile="default.aspx" MergeChars="ABC;DEF;GHI;JKL;MNO;PQR;STU;VWXYZ" ItemSeparator=" " />
	        </div></div></div></div>
	
	        <asp:repeater id="formList" runat="server">
		        <headertemplate>
			        <table class="metaInfo azData" summary="Links to forms, with descriptions of each form">
			        <thead><tr><th scope="col">Form</th><th scope="col">Description</th></tr></thead>
			        <tbody>
		        </headertemplate>
		        <itemtemplate>
			        <tr>
			        <td><a href='<%# System.Web.HttpUtility.HtmlEncode(((System.Data.DataRowView)Container.DataItem)["Url"].ToString()) %>'><%# ((System.Data.DataRowView)Container.DataItem)["UrlTitle"].ToString() %></a></td>
			        <td><%# ((System.Data.DataRowView)Container.DataItem)["UrlDescription"].ToString() %></td>
			        </tr>
		        </itemtemplate>
		        <footertemplate>
			        </tbody>
			        </table>
		        </footertemplate>
	        </asp:repeater>
	
	        <p id="noForms" runat="server">Sorry, there are no forms beginning with this letter.</p>
        </div>

        <EastSussexGovUK:Related runat="server">
            <PagesTemplate>
		        <ul>
			        <li><a href="<%= ResolveUrl("~/") %>default.aspx">A&#8211;Z of services</a></li>
		        </ul>
            </PagesTemplate>
        </EastSussexGovUK:Related>

        <EastSussexGovUK:Share runat="server" />
    </article>
    </div>
</asp:Content>