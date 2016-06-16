<%@ Page language="c#" Codebehind="IpsvData.aspx.cs" AutoEventWireup="True" Inherits="Escc.AZ.Admin.IpsvData" %>

<asp:Content runat="server" ContentPlaceHolderID="metadata">
    <title runat="server">IpsvData</title>
    <ClientDependency:Css runat="server" Files="ContentSmall" />
    <EastSussexGovUK:ContextContainer runat="server" Desktop="true">
        <ClientDependency:Css runat="server" Files="ContentMedium" MediaConfiguration="Medium" />
        <ClientDependency:Css runat="server" Files="ContentLarge" MediaConfiguration="Large" />
    </EastSussexGovUK:ContextContainer>
</asp:Content>
