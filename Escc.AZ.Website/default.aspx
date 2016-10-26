<%@ Page language="c#" Codebehind="default.aspx.cs" AutoEventWireup="True" Inherits="Escc.AZ.Website.Headings" ValidateRequest="false" EnableViewState="false"  %>
<%@ Register tagPrefix="EastSussexGovUK" tagName="Share" src="~/share.ascx" %>
<asp:Content runat="server" ContentPlaceholderId="metadata">
	<Metadata:MetadataControl id="headContent" runat="server"
		Title="A-Z of services: "
		Keywords="a-z, index, search, contents, find"
		LgtLType="A to Z"
		IpsvPreferredTerms="Local government"
		DateCreated="2004-08-05"
		Description="Find services provided by the county, district and borough councils in East Sussex."
		 />
    <meta name="robots" content="noindex, follow" />
    <ClientDependency:Script runat="server" Files="AtoZ" MergeWithSimilar="false" ID="azScript" />
    <EastSussexGovUK:ContextContainer runat="server" Desktop="true">
        <ClientDependency:Css Files="AtoZ;FormsSmall" runat="server" />
        <ClientDependency:Css runat="server" Files="FormsMedium" MediaConfiguration="Medium" />
        <ClientDependency:Css runat="server" Files="FormsLarge" MediaConfiguration="Large" />
    </EastSussexGovUK:ContextContainer>
</asp:Content>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="atoz" />

<asp:Content runat="server" ContentPlaceholderId="content">
	<div class="article">
    <article>
	    <div class="content text-content">
            <h1>A&#8211;Z of services</h1>

            <p><a href="#services" class="keyboard aural">Skip to services</a></p>
	
            <p>For a list of services you can apply for online or by post, see our <a href="<%= ResolveUrl("~/") %>forms/">A&#8211;Z of forms</a>.</p>
	        <p>If you can't find what you want, try searching the whole site using the main search box at the top right of any page.</p>
	        <p>For a summary of this information in standard format, large print, on audio tape or in other languages, please call us on 01273 481920 or email <a href="/contactus/emailus/email.aspx?n=Corporate Communications&amp;e=corporate.communications&amp;d=eastsussex.gov.uk">Corporate Communications</a>.</p>

        <div class="form short-form">
            <az:AZSearchForm id="azSearchForm" runat="server"></az:AZSearchForm>
        </div>

	        <div id="services">
	        <h2 id="criteria" runat="server">Showing </h2>
	        <p id="notFound" runat="server">Sorry. There are no services matching the letter you selected.</p>
	        <p id="notFoundSearch" runat="server">Sorry. There are no entries in the A&#8211;Z of services matching your search. Try <a href="/search/search.aspx?q={0}" id="searchLink" runat="server">searching for '<asp:Literal ID="searchTerm" runat="server" />'</a> on the rest of the site.</p>
	        <ol class="az" id="headingList" runat="server"></ol>
	        </div>
        </div>
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