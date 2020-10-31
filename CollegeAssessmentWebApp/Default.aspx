<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CollegeAssessmentWebApp._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>College Assessment Web App</h1>
        <p class="lead">Click the button.</p>
        <asp:Button ID="btnReport" runat="server" Text="RunReport" OnClick="btnReport_Click" />
    </div>

    <div class="row">
        <div class="col-md-4">
            <h2>Getting started</h2>
            <p> </p>
            <p> </p>
        </div>
        <div class="col-md-4">
            <h2>Test</h2>
            <p></p>
            <p></p>
        </div>
        <div class="col-md-4">
            <h2>Test</h2>
            <p></p>
            <p></p>
        </div>
    </div>

</asp:Content>
