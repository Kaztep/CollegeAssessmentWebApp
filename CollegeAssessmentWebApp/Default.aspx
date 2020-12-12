<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CollegeAssessmentWebApp._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>&nbsp;</h1>
        <h1>College Assessment Web App</h1>
        <p class="lead">Click the button.</p>
        <asp:Button ID="btnReport" runat="server" Text="Run Report" OnClick="btnReport_Click" />
        <asp:ListBox ID="lstbFileNames" runat="server" Height="80px" Width="663px"></asp:ListBox>
    </div>

    <div class="row">
        <div class="col-md-4">
            <h2>Getting started</h2>
            <p> 
                &nbsp;</p>
            <p> </p>
        </div>
        <div class="col-md-4">
            <h2>Upload Excel File
                <asp:DropDownList ID="CourseSelection" runat="server">
                    <asp:ListItem Value="">Please Select</asp:ListItem> 
                    <asp:ListItem>Course1</asp:ListItem>
                    <asp:ListItem>Course2</asp:ListItem>
                    <asp:ListItem>Course3</asp:ListItem>
                    <asp:ListItem>Course4</asp:ListItem>
                    <asp:ListItem>Course5</asp:ListItem>
                </asp:DropDownList>
                </h2>
            <p>
                <asp:FileUpload ID="FileUpLoad1" runat="server" />
                <asp:Button ID="btnUpload" runat="server" OnClick="btnUpload_Click" Text="Upload File" />
            </p>
            <p>
                <asp:Label ID="Label1" runat="server"></asp:Label>
            </p>
        </div>
        <div class="col-md-4">
            <h2>Test<asp:ListBox ID="ListBox1" runat="server"></asp:ListBox>
            </h2>
            <p></p>
            <p></p>
        </div>
    </div>

</asp:Content>
