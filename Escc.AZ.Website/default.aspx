<%@ Page language="c#" Codebehind="default.aspx.cs" AutoEventWireup="True" Inherits="Escc.AZ.Website.Headings" ValidateRequest="false" EnableViewState="false" MasterPageFile="~/masterpages/EastSussexGovUK.master"  %>
<%@ Register TagPrefix="az" Namespace="Escc.AZ" Assembly="Escc.AZ" %>
<asp:Content runat="server" ContentPlaceholderId="metadata">
	<Egms:MetadataControl id="headContent" runat="server"
		Title="A-Z of services: "
		Keywords="a-z, index, search, contents, find"
		LgtLType="A to Z"
		IpsvPreferredTerms="Local government"
		DateCreated="2004-08-05"
		Description="Find services provided by the county, district and borough councils in East Sussex."
		 />
    <meta name="robots" content="noindex, follow" />
    <Egms:Css Files="AtoZ;FormsSmall" runat="server" />
    <Egms:Script runat="server" Files="AtoZ" MergeWithSimilar="false" ID="azScript" />
    <EastSussexGovUK:ContextContainer runat="server" Desktop="true">
        <Egms:Css runat="server" Files="FormsMedium" MediaConfiguration="Medium" />
        <Egms:Css runat="server" Files="FormsLarge" MediaConfiguration="Large" />
    </EastSussexGovUK:ContextContainer>
</asp:Content>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="atoz" />

<asp:Content runat="server" ContentPlaceholderId="content">
	<div class="article">
    <article>
	    <div class="text">
            <h1>A&#8211;Z of services</h1>

            <p><a href="#services" class="keyboard aural">Skip to services</a></p>
	
	        <p>Find services provided by the county, district and borough councils in East Sussex.</p>
            <p>For a list of services you can apply for online or by post, see our <a href="/atoz/forms/">A&#8211;Z of forms</a>.</p>
	        <p>If you can't find what you want, try searching the whole site using the main search box at the top right of any page.</p>
	        <p>For a summary of this information in standard format, large print, on audio tape or in other languages, please call us on 01273 481920 or email <a href="/contactus/emailus/email.aspx?n=Corporate Communications&amp;e=corporate.communications&amp;d=eastsussex.gov.uk">Corporate Communications</a>.</p>
            </div>

        <div class="form short-form">
            <az:AZSearchForm id="azSearchForm" runat="server"></az:AZSearchForm>
        </div>

	        <div id="services" class="text">
	        <h2 id="criteria" runat="server">Showing </h2>
	        <p id="notFound" runat="server">Sorry. There are no services matching the letter you selected.</p>
	        <p id="notFoundSearch" runat="server">Sorry. There are no entries in the A&#8211;Z of services matching your search. Try <a href="/search/search.aspx?q={0}" id="searchLink" runat="server">searching for '<asp:Literal ID="searchTerm" runat="server" />'</a> on the rest of the site.</p>
	        <ol class="az" id="headingList" runat="server"></ol>
	        </div>

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