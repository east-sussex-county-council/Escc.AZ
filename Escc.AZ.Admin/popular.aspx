<%@ Page language="c#" Codebehind="popular.aspx.cs" AutoEventWireup="false" Inherits="Escc.AZ.Admin.popular" %>

<asp:Content runat="server" ContentPlaceHolderID="metadata">
    <Egms:MetadataControl id="headContent" runat="server" 
		Title="Manage popular forms" 
		DateCreated="2005-11-04"
		IpsvPreferredTerms="Local government"
		/>
    <Egms:Css runat="server" Files="FormsSmall" />
    <EastSussexGovUK:ContextContainer runat="server" Desktop="true">
        <Egms:Css runat="server" Files="FormsMedium" MediaConfiguration="Medium" />
        <Egms:Css runat="server" Files="FormsLarge" MediaConfiguration="Large" />
    </EastSussexGovUK:ContextContainer>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="content">
<div class="full-page">
<article>
    <div class="text">
	    <h1>Manage popular forms</h1>
	    <p>The higher the popularity, the higher the form appears in the list of popular forms. Forms with a popularity of 0 drop off the bottom of the list.</p>
	
	    <FormControls:EsccValidationSummary runat="server" />
	
	    <asp:repeater id="formList" runat="server">
	    <headertemplate>
		    <table summary="East Sussex County Council forms, with a editable box to set their popularity" class="itemManager form">
		    <thead><tr><th scope="col">Form</th><th scope="col">Popularity</th></tr></thead>
		    <tbody>
	    </headertemplate>
	    <itemtemplate>
		    <tr>
			    <td>
				    <input type="hidden" id="urlid" runat="server" value='<%# ((System.Data.DataRowView)Container.DataItem)["ServiceUrlId"].ToString() %>' />
				    <%# ((System.Data.DataRowView)Container.DataItem)["UrlTitle"].ToString() %> <span class="service">in <a href="service.aspx?service=<%# ((System.Data.DataRowView)Container.DataItem)["ServiceId"].ToString() %>"><%# ((System.Data.DataRowView)Container.DataItem)["Service"].ToString() %></a></span>
			    </td>
			    <td class="azPopularity">
			    <asp:textbox id="popularity" runat="server" text='<%# ((System.Data.DataRowView)Container.DataItem)["SortPriority"].ToString() %>' ontextchanged="popularity_TextChanged" CssClass="numeric" />
			    <FormControls:EsccRegularExpressionValidator id="regVal" runat="server" controltovalidate="popularity" validationexpression="^[0-9]{1,3}$" ErrorMessage="Popularity must be a number between 0 and 100" />
			    <FormControls:Esccrangevalidator id="rangeVal" runat="server" controltovalidate="popularity" minimumvalue="0" maximumvalue="100" errormessage="Popularity must be a number between 0 and 100" type="Integer" />
			    </td>
		    </tr>
	    </itemtemplate>
	    <footertemplate>
		    </tbody>
		    </table>
	    </footertemplate>
	    </asp:repeater>
	
	    <div class="formButtons"><FormControls:EsccButton id="save" runat="server" text="Save" CssClass="button" onclick="save_Click" /></div>
    </div>
</article>
</div>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="supporting" />