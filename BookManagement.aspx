<%@ Page Title="BookManagement" MasterPageFile="~/Master/DashMasterPage.Master" Language="C#" AutoEventWireup="true" CodeBehind="BookManagement.aspx.cs" Inherits="fyp.BookManagement" EnableViewState="true" EnableEventValidation="true"%>

<asp:Content ID="content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .searchbar {
    display: flex;
    align-items: center;
    background: #f1f1f1;
    height: 50px;
    margin: 1em auto;
    border-radius: 25px;
    border: 2px solid #ddd;
    width: 100%;
    max-width: 600px;
    padding: 0;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    transition: box-shadow 0.3s;
}

.searchbar:hover {
    box-shadow: 0 6px 12px rgba(0, 0, 0, 0.2);
}

.search-input {
    flex: 1;
    border: none;
    height: 100%;
    font-size: 16px;
    font-family: 'Poppins', sans-serif;
    color: #333;
    padding: 0 15px; /* Adjust padding for better spacing */
    background: transparent;
    border-radius: 25px 0 0 25px; /* Round left corners */
}

.search-input:focus {
    outline: none;
}

.search-input::placeholder {
    color: #aaa;
    font-style: italic;
}

.search-btn {
    background-color: #007bff;
    color: white;
    border: none;
    height: 100%;
    border-radius: 25px; /* Round right corners */
    font-size: 16px;
    font-family: 'Poppins', sans-serif;
    padding: 0 20px;
    cursor: pointer;
    transition: background-color 0.3s, box-shadow 0.3s;
}

.search-btn:hover {
    background-color: #0056b3;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.2);
}

.search-btn:focus {
    outline: none;
}

    </style>
    <asp:ScriptManager runat="server" EnablePageMethods="true" />
    <link rel="stylesheet" href="assets/css/BookManagement.css?v=1.0">

    <div class="upper">
        <div class="searchbar">
            <asp:TextBox ID="searchBar" runat="server" CssClass="search-input" placeholder="Search for a book..."></asp:TextBox>
            <asp:Button ID="btnSearch" runat="server" CssClass="search-btn" Text="Search" OnClick="btnSearch_Click" CausesValidation="False"/>
        </div>
        <div class="buttons">
            <asp:Button ID="btnAddBook" runat="server" CssClass="add-Book-button" Text="Add Book" OnClientClick="showAddBookModal(); return false;" />

            <div id="addBookModal" class="modal" style="display: none;">
                <div class="modal-content">
                    <span class="close" onclick="hideAddBookModal()">&times;</span>
                    <h3 style="text-align: center; color: #333;">Add New Book</h3>

                    <asp:Label ID="lblErrorMessage" runat="server" ForeColor="Red" CssClass="error-message"></asp:Label>
                    <asp:Label ID="lblBookTitle" runat="server" Text="Book Title" CssClass="book-title"></asp:Label>
                    <asp:TextBox ID="txtBookTitle" runat="server" Placeholder="Book Title" CssClass="input-field" MaxLength="100" /><br />
                    <asp:RequiredFieldValidator ID="rfvBookTitle" runat="server" ControlToValidate="txtBookTitle" ErrorMessage="Book Title is required." ForeColor="Red" />
                    <br />
                    <asp:Label ID="LblBDesc" runat="server" Text="Book Description" CssClass="input-label" />
                    <asp:TextBox ID="txtBookDesc" runat="server" Placeholder="Book Description" CssClass="input-field" TextMode="MultiLine" Rows="4" MaxLength="500" /><br />
                    <asp:RequiredFieldValidator ID="rfvBookDesc" runat="server" ControlToValidate="txtBookDesc" ErrorMessage="Book Description is required." ForeColor="Red" />
                    <br />
                    <asp:Label ID="lblBookSeries" runat="server" Text="Book Series" CssClass="input-label" />
                    <asp:TextBox ID="txtBookSeries" runat="server" Placeholder="Book Series" CssClass="input-field" MaxLength="20" /><br />
                    <asp:RequiredFieldValidator ID="rfvBookSeries" runat="server" ControlToValidate="txtBookSeries" ErrorMessage="Book Series is required." ForeColor="Red" />
                    <br />

                    <!-- Dropdown for CategoryId -->
                    <asp:Label ID="lblCategories" runat="server" Text="Categories" CssClass="input-label" />
                    <asp:CheckBoxList ID="cblCategoryIds" runat="server" CssClass="input-field"
                        DataSourceID="SqlDataSourceCategory" DataTextField="CategoryName" DataValueField="CategoryId" />

                    <asp:SqlDataSource ID="SqlDataSourceCategory" runat="server"
                        ConnectionString="<%$ ConnectionStrings:ConnectionString %>"
                        SelectCommand="SELECT CategoryId, CategoryName FROM Category"></asp:SqlDataSource>

                    <asp:Label ID="lblBookImage" runat="server" Text="Book Cover Image" CssClass="input-label" />
                    <asp:FileUpload ID="fileUploadBookImage" runat="server" CssClass="input-field" /><br />
                    <asp:RequiredFieldValidator ID="rfvBookImage" runat="server" ControlToValidate="fileUploadBookImage" ErrorMessage="Book Image is required." ForeColor="Red" />
                    <br />
                    <asp:RegularExpressionValidator ID="revBookImage" runat="server" ControlToValidate="fileUploadBookImage" ErrorMessage="Only image files (jpg, jpeg, png, gif) are allowed." ForeColor="Red" ValidationExpression="^.*\.(jpg|jpeg|png|gif)$" />
                    <br />
                    <asp:Button ID="btnSubmitBook" runat="server" Text="Submit" CssClass="button" OnClick="btnSubmitBook_Click" />
                </div>
            </div>
        </div>
    </div>

    <asp:Label ID="MessageBox" runat="server" ForeColor="Red"></asp:Label>

    <asp:SqlDataSource
        ID="SqlDataSource1"
        runat="server"
        ConnectionString="<%$ ConnectionStrings:ConnectionString %>"
        SelectCommand="SELECT BookId, BookTitle, BookDesc, BookSeries, BookImage FROM Book WHERE IsDeleted = 0"></asp:SqlDataSource>
    <div class="product-container">
        <asp:Repeater ID="BooksRepeater" runat="server" DataSourceID="SqlDataSource1">
            <ItemTemplate>
                <div class="card">
                    <div class="content">
                        <div class="title"><%# Eval("BookTitle") %></div>
                        <div class="image">
                            <asp:Image ID="imageBook" runat="server"
                                ImageUrl='<%# Eval("BookImage") != DBNull.Value ? "data:image/png;base64," + Convert.ToBase64String((byte[])Eval("BookImage")) : "images/defaultCoverBook.png" %>'
                                AlternateText='<%# Eval("BookTitle") %>' />
                        </div>
                        <b>ID: <%# Eval("BookId") %></b>
                        <asp:Label ID="lblBookDesc" runat="server" Text='<%# Eval("BookDesc") %>' CssClass="text"></asp:Label>
                    </div>
                    <%--<asp:LinkButton ID="btnView" runat="server" Text="View Book" CssClass="view-book"  
                            CommandArgument='<%# Eval("BookId") %>' OnCommand="btnView_Command" />--%>
                    <asp:Button ID="btnView" runat="server" Text="View Book" CssClass="view-book"
    CommandArgument='<%# Eval("BookId") %>' OnClick="btnView_Click" CausesValidation="false" />
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <script>
        window.onload = function () {
            const urlParams = new URLSearchParams(window.location.search);
            if (urlParams.get('message') === 'deleted') {
                alert("Book deleted successfully.");
            }
        };

        //register user button action
        function showAddBookModal() {
            document.getElementById('addBookModal').style.display = 'block';
        }

        function hideAddBookModal() {
            document.getElementById('addBookModal').style.display = 'none';
        }

        function validateCheckboxList(sender, args) {
            var checkBoxList = document.getElementById('<%= cblCategoryIds.ClientID %>');
            var checkboxes = checkBoxList.getElementsByTagName("input");
            args.IsValid = Array.from(checkboxes).some(checkbox => checkbox.checked);
        }
    </script>
</asp:Content>
