<%@ Page language="c#" Codebehind="popular.aspx.cs" AutoEventWireup="True" Inherits="Escc.AZ.Website.forms.Popular" EnableViewState="false" %>
<%@ Register tagPrefix="EastSussexGovUK" tagName="Share" src="~/share.ascx" %>
<asp:Content runat="server" ContentPlaceholderId="metadata">
	<Metadata:MetadataControl id="headContent" runat="server"
		Title="Most popular East Sussex County Council forms: "
		Keywords="a-z, index, search, contents, find, forms"
		LgtLType="A to Z"
		IpsvPreferredTerms="Local government"
		DateCreated="2005-11-04"
		 />
    <EastSussexGovUK:ContextContainer runat="server" Desktop="true">
        <ClientDependency:Css Files="AtoZ" runat="server" />
    </EastSussexGovUK:ContextContainer>
</asp:Content>

<asp:Content runat="server" ContentPlaceholderId="content">
    <div class="article">
    <article>
        <div class="text-content content">
        	<h1>Most popular forms</h1>

	        <div class="roundedBox azNav"><div><div><div>
	        <NavigationControls:AZNavigation id="navigation" runat="server" TargetFile="forms.aspx" MergeChars="ABC;DEF;GHI;JKL;MNO;PQR;STU;VWXYZ" />
	        </div></div></div></div>
	
	        <asp:repeater id="formList" runat="server">
		        <headertemplate>
			        <ol class="az">
		        </headertemplate>
		        <itemtemplate>
			        <li><a href='<%# ((System.Data.DataRowView)Container.DataItem)["Url"].ToString() %>'><%# ((System.Data.DataRowView)Container.DataItem)["UrlTitle"].ToString() %></a></li>
		        </itemtemplate>
		        <footertemplate>
			        </ol>
		        </footertemplate>
	        </asp:repeater>

            <EastSussexGovUK:Share runat="server" />
        </div>
    </article>
    </div>
	    <div class="supporting related-links text-content content-small content-medium">
            <h2>Related pages</h2>
		    <ul>
			    <li><a href="<%= ResolveUrl("~/") %>default.aspx">A&#8211;Z of services</a></li>
		    </ul>
        </div>
</asp:Content>	
