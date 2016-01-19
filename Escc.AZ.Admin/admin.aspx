<%@ Page language="c#" Codebehind="admin.aspx.cs" AutoEventWireup="True" Inherits="Escc.AZ.Admin.admin" EnableViewState="false" %>

<asp:Content runat="server" ContentPlaceHolderID="metadata">
	<Metadata:MetadataControl id="headContent" runat="server"
		title="A-Z administration tasks"
		datecreated="2006-06-15"
		ipsvpreferredterms="Local government"
	 />
    <ClientDependency:Css runat="server" Files="FormsSmall" />
    <EastSussexGovUK:ContextContainer runat="server" Desktop="true">
        <ClientDependency:Css runat="server" Files="FormsMedium" MediaConfiguration="Medium" />
        <ClientDependency:Css runat="server" Files="FormsLarge" MediaConfiguration="Large" />
    </EastSussexGovUK:ContextContainer></asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="content">
<div class="full-page">
    <section>
	    <h1 class="text">A-Z administration tasks</h1>
	    <div class="form simple-form">
        <div class="formBox roundedBox"><div><div><div>
	    <fieldset class="formPart">
	    <legend class="formLabel">Select task</legend>
	
	    <asp:radiobuttonlist id="tasks" runat="server" cssclass="formControl radioButtonList" repeatdirection="Horizontal" repeatlayout="Flow">
		    <asp:listitem value="Update headings' IPSV compliance" />
		    <asp:listitem value="Update ESCC services' IPSV compliance" />
	    </asp:radiobuttonlist>
	
	    </fieldset>
	    </div></div></div></div>
	    <div class="formButtons"><FormControls:EsccButton id="submit" runat="server" text="Next" onclick="submit_Click" /></div>
        </div>
    </section>
</div>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="supporting" />