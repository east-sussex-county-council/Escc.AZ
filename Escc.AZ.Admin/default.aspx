<%@ Page Language="C#" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="metadata">
	<Metadata:MetadataControl id="headcontent" runat="server" 
		title="Manage A-Z of council services"
		IpsvPreferredTerms="Internet"
		DateCreated="2011-04-15"
		/>
    <ClientDependency:Css runat="server" Files="LandingSmall" />
    <EastSussexGovUK:ContextContainer runat="server" Desktop="true">
        <ClientDependency:Css runat="server" Files="LandingMedium" MediaConfiguration="Medium" />
        <ClientDependency:Css runat="server" Files="LandingLarge" MediaConfiguration="Large" />
    </EastSussexGovUK:ContextContainer>
</asp:Content>

<asp:Content ID="Content2" runat="server" ContentPlaceHolderID="content">
<div class="full-page">
<section>
    <div class="text">
        <h1>Manage A-Z of council services</h1>
    </div>
        
        
<div id="ctl00_content_ctl05_container" class="landing descriptions">
    <div id="ctl00_content_ctl05_section1" class="odd group1 offset-pair1">
        <h2 id="listTitle01"><a href="<%= ResolveUrl("~/") %>headings.aspx">Manage headings</a></h2>
        
        <p id="desc01">Create, edit and delete the headings which appear in the A-Z listings on this website.</p>
        
        
        
        
    </div>
    <div id="ctl00_content_ctl05_section2" class="even group2 offset-pair2">
        <h2 id="listTitle02"><a href="<%= ResolveUrl("~/") %>services.aspx">Manage services</a></h2>
        
        <p id="desc02">Create, edit and delete the services which are listed when a heading is selected in the A-Z listings on this website. Place services provided by other authorities under headings used by this site.</p>
        
        
        
        
    </div>
    <div id="ctl00_content_ctl05_section3" class="odd group3 offset-pair2">
        <h2 id="listTitle03"><a href="<%= ResolveUrl("~/") %>orphans.aspx">Orphaned services</a></h2>
        
        <p id="desc03">Services not placed under a heading will not be listed in the A-Z on this website. They are listed separately here, so that headings can be assigned.</p>
        
        
        
        
    </div>
    <div id="ctl00_content_ctl05_section4" class="even group1 offset-pair1">
        <h2 id="listTitle04"><a href="<%= ResolveUrl("~/") %>popular.aspx">Manage popular forms</a></h2>
        
        <p id="desc04">Control which forms are listed on the <a href="/atoz/popular.aspx">most popular forms page</a>, and in which order.</p>
        
            
        
        
    </div>
    </div>


</section>
</div>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="supporting" />