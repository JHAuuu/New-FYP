<%@ Page Title="BookCopyManagement" MasterPageFile="~/Master/DashMasterPage.Master" Language="C#" AutoEventWireup="true" CodeBehind="BookCopyManagement.aspx.cs" Inherits="fyp.BookCopyManagement" %>

<asp:Content ID="content1" ContentPlaceHolderID="MainContent" runat="server">
    <link rel="stylesheet" href="assets/css/BookCopyManagement.css?v=1.0">
    <asp:HiddenField ID="hiddenBookCopyId" runat="server" />
    <style>
        .modal {
            position: fixed;
            z-index: 1000;
            padding-top: 50px;
            left: 0;
            top: 0;
            width: 100%;
            height: 100%;
            overflow: auto;
            background-color: rgba(0, 0, 0, 0.5);
        }

        .modal-content {
            background-color: #ffffff;
            margin: auto;
            padding: 30px;
            border-radius: 10px;
            width: 400px;
            box-shadow: 0px 5px 15px rgba(0, 0, 0, 0.3);
            position: relative;
            animation: slide-down 0.4s ease;
        }

        @keyframes slide-down {
            from {
                transform: translateY(-20px);
                opacity: 0;
            }

            to {
                transform: translateY(0);
                opacity: 1;
            }
        }


        /* Close button */
        .close {
            position: absolute;
            top: 15px;
            right: 20px;
            font-size: 24px;
            cursor: pointer;
            color: #666;
            transition: color 0.2s;
        }

            .close:hover {
                color: #333;
            }

        /* Input fields */
        .input-field {
            width: 100%;
            padding: 12px;
            margin: 10px 0;
            border: 1px solid #ddd;
            border-radius: 5px;
            box-sizing: border-box;
            font-size: 16px;
        }

            .input-field:focus {
                border-color: #007bff;
                outline: none;
                box-shadow: 0 0 5px rgba(0, 123, 255, 0.3);
            }

        /* Submit button */
        .button {
            background-color: #007bff;
            color: white;
            padding: 12px;
            margin-top: 10px;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            font-size: 16px;
            width: 100%;
            transition: background-color 0.3s;
        }

            .button:hover {
                background-color: #0056b3;
            }

        .bookCopyCoverPre {
            width: 100%;
            border-radius: 5px;
        }

        .grid-view {
            width: 100%;
            margin: 20px auto;
            border-collapse: collapse;
            font-family: Arial, sans-serif;
            font-size: 14px;
            border: 1px solid #ddd;
            box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.1);
        }

            /* Header Styling */
            .grid-view th {
                background-color: #007bff;
                color: #fff;
                text-align: left;
                padding: 10px;
                text-transform: uppercase;
                border: 1px solid #ddd;
                font-weight: bold;
            }

            /* Data Row Styling */
            .grid-view td {
                padding: 10px;
                border: 1px solid #ddd;
            }

            /* Alternating Row Colors */
            .grid-view tr:nth-child(even) {
                background-color: #f9f9f9;
            }

            /* Hover Effect on Rows */
            .grid-view tr:hover {
                background-color: #f1f1f1;
            }

        /* Buttons in Action Column */
        .grid-button {
            background-color: #28a745;
            color: white;
            padding: 5px 10px;
            border: none;
            border-radius: 3px;
            cursor: pointer;
            font-size: 12px;
            transition: background-color 0.3s ease;
            text-align: center;
        }

            .grid-button:hover {
                background-color: #218838;
            }

        /* Pagination Styling */
        .grid-view .pager {
            text-align: center;
            padding: 10px 0;
            background-color: #f9f9f9;
        }

            .grid-view .pager a {
                margin: 0 5px;
                padding: 5px 10px;
                text-decoration: none;
                color: #007bff;
                border: 1px solid #ddd;
                border-radius: 3px;
                transition: background-color 0.3s ease, color 0.3s ease;
            }

                .grid-view .pager a:hover {
                    background-color: #007bff;
                    color: #fff;
                }

            .grid-view .pager .current-page {
                font-weight: bold;
                color: #fff;
                background-color: #007bff;
                border: 1px solid #007bff;
                border-radius: 3px;
                padding: 5px 10px;
            }
    </style>
    <asp:ScriptManager ID="ScriptManager1" runat="server" />
    <div class="container">
        <div class="book-card">
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>
            <!-- Flexbox container for book info -->
            <div class="book-info">
                <!-- Book Cover -->
                <div class="book-cover">
                    <asp:Image ID="bookCoverImage" runat="server" AlternateText="Harry Potter and the Philosopher's Stone" CssClass="book-cover" />
                    <div class="btn-contain">
                        <div class="btn-book">
                            <button id="btnEditBook" type="button" class="edit-button" onclick="showEditBookModal()">Modify<i class="las la-user-edit"></i></button>

                            <!-- Modal Structure -->
                            <div id="editBookModal" class="modal" style="display: none;">
                                <div class="modal-content">
                                    <span class="close" onclick="hideEditBookModal()">&times;</span>
                                    <h3 style="text-align: center; color: #333;">Edit Book</h3>

                                    <!-- Form fields in modal -->
                                    <asp:Label ID="lblErrorMessage" runat="server" ForeColor="Red" CssClass="error-message"></asp:Label>

                                    <asp:Image ID="bookImagePreview" runat="server" AlternateText="Book Image" Style="max-width: 100px; margin-bottom: 10px; display: none; margin-left: 116px" />

                                    <!-- Book Title -->
                                    <asp:Label ID="lblBookEditTitle" runat="server" Text="Book Title" CssClass="input-label" />
                                    <asp:TextBox ID="txtBookTitle" runat="server" Placeholder="Enter Book Title" CssClass="input-field" MaxLength="100" />
                                    <asp:RequiredFieldValidator ID="rfvBookTitle" runat="server" ControlToValidate="txtBookTitle" ErrorMessage="Book Title is required." ForeColor="Red" />
                                    <br />

                                    <!-- Book Description -->
                                    <asp:Label ID="LblBDesc" runat="server" Text="Book Description" CssClass="input-label" />
                                    <asp:TextBox ID="txtBookDesc" runat="server" Placeholder="Enter Book Description" CssClass="input-field" TextMode="MultiLine" Rows="4" MaxLength="500" />
                                    <asp:RequiredFieldValidator ID="rfvBookDesc" runat="server" ControlToValidate="txtBookDesc" ErrorMessage="Book Description is required." ForeColor="Red" />
                                    <br />

                                    <!-- Book Series -->
                                    <asp:Label ID="lblBookSeries" runat="server" Text="Book Series" CssClass="input-label" />
                                    <asp:TextBox ID="txtBookSeries" runat="server" Placeholder="Enter Book Series" CssClass="input-field" MaxLength="20" />
                                    <asp:RequiredFieldValidator ID="rfvBookSeries" runat="server" ControlToValidate="txtBookSeries" ErrorMessage="Book Series is required." ForeColor="Red" />
                                    <br />

                                    <!-- Author Name -->
                                    <asp:Label ID="Label3" runat="server" Text="Author Name" CssClass="input-label" />
                                    <asp:TextBox ID="txtAuthorName" runat="server" Placeholder="Enter Author Name" CssClass="input-field" MaxLength="100" />
                                    <asp:RequiredFieldValidator ID="rfvAuthorName" runat="server" ControlToValidate="txtAuthorName" ErrorMessage="Author Name is required." ForeColor="Red" />
                                    <br />

                                    <!-- Category List -->
                                    <asp:Label ID="lblCategories" runat="server" Text="Categories" CssClass="input-label" />
                                    <asp:CheckBoxList ID="cblCategoryIds" runat="server" CssClass="input-field"
                                        DataSourceID="SqlDataSourceCategory" DataTextField="CategoryName" DataValueField="CategoryName" />

                                    <asp:SqlDataSource ID="SqlDataSourceCategory" runat="server"
                                        ConnectionString="<%$ ConnectionStrings:ConnectionString %>"
                                        SelectCommand="SELECT CategoryId, CategoryName FROM Category"></asp:SqlDataSource>

                                    <!-- Book Image Upload -->
                                    <asp:Label ID="lblBookImage" runat="server" Text="Book Cover Image (leave original photo if not selected)" CssClass="input-label" />
                                    <asp:FileUpload ID="fileUploadBookImage" runat="server" CssClass="input-field" />
                                    <br />
                                    <asp:RegularExpressionValidator ID="revBookImage" runat="server" ControlToValidate="fileUploadBookImage" ErrorMessage="Only image files (jpg, jpeg, png, gif) are allowed." ForeColor="Red" ValidationExpression="^.*\.(jpg|jpeg|png|gif)$" />
                                    <br />

                                    <asp:Button ID="btnSubmitUp" runat="server" Text="Submit" CssClass="button" OnClick="btnSubmitUp_Click" CausesValidation="False"/>
                                </div>
                            </div>
                            <button type="button" class="delete-button" onclick="confirmDeleteBook()">Delete<i class="las la-trash"></i></button>
                        </div>
                    </div>
                </div>

                <!-- Book Details -->
                <div class="book-details">
                    <!-- Book Title and Author -->
                    <asp:Label ID="lblBookTitle" runat="server" CssClass="book-title"></asp:Label>
                    <br />
                    By
                    <asp:Label ID="lblAuthorName" runat="server" CssClass="book-author"></asp:Label>

                    <!-- Ratings Section -->
                    <div class="ratings">
                        <span>
                            <asp:Label ID="lblRatingAverage" runat="server" Text="Label"></asp:Label>
                        </span>
                    </div>

                    <!-- Book Summary -->
                    <asp:Label ID="lblBookDesc" runat="server" CssClass="book-summary"></asp:Label>

                    <div class="category">
                        Category:
                        <asp:Label ID="lblCategory" runat="server"></asp:Label>
                    </div>
                </div>
            </div>

            <asp:SqlDataSource ID="sqlDSBookCopy" runat="server"
                ConnectionString="<%$ ConnectionStrings:ConnectionString %>"
                SelectCommand="SELECT bc.BookCopyId, bc.PublishDate, bc.PublishOwner, bc.IsAvailable, bc.ISBN, bc.BookCopyImage, bc.BookId, b.BookTitle FROM BookCopy AS bc INNER JOIN Book AS b ON bc.BookId = b.BookId WHERE (bc.BookId = @BookId) AND (bc.IsDeleted = 0)" UpdateCommand="UPDATE bc
SET 
    bc.PublishDate = @PublishDate, 
    bc.PublishOwner = @PublishOwner,
    bc.IsAvailable = @IsAvailable
FROM 
    BookCopy AS bc
INNER JOIN 
    Book AS b ON bc.BookId = b.BookId
WHERE 
    bc.BookId = @BookId AND 
    bc.IsDeleted = 0;
">
                <SelectParameters>
                    <asp:Parameter Name="BookId" Type="Int32" />
                </SelectParameters>
                <UpdateParameters>
                    <asp:Parameter Name="PublishDate" />
                    <asp:Parameter Name="PublishOwner" />
                    <asp:Parameter Name="BookId" />
                </UpdateParameters>
            </asp:SqlDataSource>

            <div class="editions">
                <h3>
                    <asp:Label ID="lblBookCopyCount" runat="server" />
                </h3>
                <div class="top-section">
                    <asp:Button ID="btnAddBookCopy" runat="server" CssClass="add-book-button" Text="Add Book Copy" OnClientClick="showAddBookCopyModal(); return false;" />
                    <div id="addBookCopyModal" class="modal" style="display: none;">
                        <div class="modal-content">
                            <asp:UpdatePanel ID="UpdatePanelAddBookCopy" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <span class="close" onclick="hideAddBookCopyModal()">&times;</span>
                                    <h3 style="text-align: center; color: #333;">Add Book Copy</h3>

                                    <asp:Label ID="lblAddErrorMessage" runat="server" ForeColor="Red" CssClass="error-message"></asp:Label>

                                    <!-- Publish Date -->
                                    <asp:Label ID="lblAddPublishDate" runat="server" Text="Publish Date" CssClass="input-label" />
                                    <asp:TextBox ID="txtAddPublishDate" runat="server" Placeholder="Enter Publish Date (YYYY-MM-DD)" CssClass="input-field" TextMode="Date" />
                                    <asp:RequiredFieldValidator ID="rfvAddPublishDate" runat="server" ControlToValidate="txtAddPublishDate" ErrorMessage="Publish Date is required." ForeColor="Red" />
                                    <br />

                                    <!-- Publish Owner -->
                                    <asp:Label ID="lblAddPublishOwner" runat="server" Text="Publish Owner" CssClass="input-label" />
                                    <asp:TextBox ID="txtAddPublishOwner" runat="server" Placeholder="Enter Publish Owner" CssClass="input-field" MaxLength="100" />
                                    <asp:RequiredFieldValidator ID="rfvAddPublishOwner" runat="server" ControlToValidate="txtAddPublishOwner" ErrorMessage="Publish Owner is required." ForeColor="Red" />
                                    <br />

                                    <!-- Book Image -->
                                    <asp:Label ID="lblAddBookImage" runat="server" Text="Book Cover Image (Optional)" CssClass="input-label" />
                                    <asp:FileUpload ID="fileUploadAddBookImage" runat="server" CssClass="input-field" />
                                    <br />

                                    <!-- Submit Button -->
                                    <asp:Button ID="btnSubmitAddBC" runat="server" Text="Add Book Copy" CssClass="button" OnClick="btnSubmitAddBC_Click" CausesValidation="False"/>
                                </ContentTemplate>
                                <Triggers>
                                    <asp:PostBackTrigger ControlID="btnSubmitAddBC" />
                                </Triggers>
                            </asp:UpdatePanel>
                        </div>
                    </div>

                    <div>
                        Search:
                        <input type="search" placeholder="Search" />
                    </div>
                </div>
                <asp:GridView ID="GridView1" runat="server" AllowPaging="True" AllowSorting="True" AutoGenerateColumns="False"
                    DataKeyNames="BookCopyId" DataSourceID="sqlDSBookCopy"
                    OnRowCommand="GridView1_RowCommand" OnRowUpdating="GridView1_RowUpdating" OnRowEditing="GridView1_RowEditing" OnRowCancelingEdit="GridView1_RowCancelingEdit"
                    CssClass="grid-view">
                    <Columns>
                        <asp:TemplateField HeaderText="BookCopyImage">
                            <ItemTemplate>
                                <asp:Image ID="imgBookCopy" runat="server"
                                    ImageUrl='<%# Eval("BookCopyImage") != DBNull.Value ? "data:image/png;base64," + Convert.ToBase64String((byte[])Eval("BookCopyImage")) : "~/images/defaultCoverBook.png" %>'
                                    AlternateText="No Image Available" Width="100px" Height="150px" />
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:FileUpload ID="fileUploadBookCopyImage" runat="server" />
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="BookTitle">
                            <ItemTemplate>
                                <asp:Label ID="lblBookTitle" runat="server" Text='<%# Eval("BookTitle") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <!-- Make BookTitle non-editable in Edit mode -->
                                <asp:Label ID="lblEditBookTitle" runat="server" Text='<%# Bind("BookTitle") %>'></asp:Label>
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="ISBN">
                            <ItemTemplate>
                                <asp:Label ID="lblISBN" runat="server" Text='<%# Eval("ISBN") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <!-- Optionally allow editing of ISBN here -->
                                <asp:TextBox ID="txtEditISBN" runat="server" Text='<%# Bind("ISBN") %>'></asp:TextBox>
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Publish Date">
                            <ItemTemplate>
                                <asp:Label ID="lblPublishDate" runat="server" Text='<%# Eval("PublishDate") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:Calendar ID="calendarEditPublishDate" runat="server" SelectedDate='<%# Bind("PublishDate") %>'></asp:Calendar>
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="PublishOwner">
                            <ItemTemplate>
                                <asp:Label ID="lblPublishOwner" runat="server" Text='<%# Eval("PublishOwner") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtEditPublishOwner" runat="server" Text='<%# Bind("PublishOwner") %>'></asp:TextBox>
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="IsAvailable">
                            <ItemTemplate>
                                <asp:CheckBox ID="chkIsAvailable" runat="server" Checked='<%# Eval("IsAvailable") %>' Enabled="false" />
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:CheckBox ID="chkEditIsAvailable" runat="server" Checked='<%# Bind("IsAvailable") %>' Enabled="false" />
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Barcode">
                            <ItemTemplate>
                                <asp:Button ID="btnDownloadBarcode" runat="server" Text="Download Barcode" CssClass="barcode-button"
                                    CommandName="DownloadBarcode" CommandArgument='<%# Eval("ISBN") %>' CausesValidation="false" />
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <button id="btnDeleteBookCopy" type="button" class="delete-button" style="width: 100%;" onclick="confirmDeleteBookCopy()">Delete<i class="las la-trash"></i></button>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:CommandField ShowEditButton="True" CausesValidation="False" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>






    <script>
        function showEditBookModal() {
    // Retrieve data from the session variables
    <% 
        if (Session["BookData"] != null)
        {
            var bookData = Session["BookData"];
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var bookDataJson = serializer.Serialize(bookData);
            Response.Write("var bookData = " + bookDataJson + ";");
        }
        else
        {
            Response.Write("alert('Book data is not available in session.');");
        }
    %>

            // Populate modal fields if bookData is available
            if (bookData) {
                document.getElementById("<%= txtBookTitle.ClientID %>").value = bookData.Title;
                        document.getElementById("<%= txtBookDesc.ClientID %>").value = bookData.Description;
                        document.getElementById("<%= txtBookSeries.ClientID %>").value = bookData.Series;
                        document.getElementById("<%= txtAuthorName.ClientID %>").value = bookData.Author;

                        // Set selected categories for CheckBoxList
                        const selectedCategories = bookData.Categories.split(',').map(function (item) { return item.trim(); });

                        const checkBoxList = document.getElementById("<%= cblCategoryIds.ClientID %>").getElementsByTagName("input");
                        for (let i = 0; i < checkBoxList.length; i++) {
                            if (selectedCategories.includes(checkBoxList[i].value)) {
                                checkBoxList[i].checked = true;
                            } else {
                                checkBoxList[i].checked = false;
                            }
                        }

                        // Show the existing book image preview if available
                        if (bookData.CoverImageBase64) {
                            document.getElementById("<%= bookImagePreview.ClientID %>").src = bookData.CoverImageBase64;
                    document.getElementById("<%= bookImagePreview.ClientID %>").style.display = "block";
                }

                // Show the modal
                document.getElementById("editBookModal").style.display = "block";
            }
        }


        function hideEditBookModal() {
            document.getElementById('editBookModal').style.display = 'none';
        }

        function confirmDeleteBook() {
            if (confirm('Are you sure you want to delete this book?')) {
                var bookId = <%= Session["BookId"] %>; // Get the BookId from the session

                fetch('BookCopyManagement.aspx/DeleteBook', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ bookId: bookId })
                })
                    .then(response => response.json())
                    .then(data => {
                        if (data.d === "Success") {
                            // Redirect with a success message in the URL query parameters
                            window.location.href = "BookManagement.aspx?message=deleted";
                        } else {
                            alert(data);  // Display any error message returned from the server
                        }
                    })
                    .catch(error => {
                        alert("Error: " + error);
                    });
            }
        }

        function confirmDeleteBookCopy() {
            if (confirm('Are you sure you want to delete this book?')) {
                var bookId = <%= Session["BookId"] %>; // Get the BookId from the session

                fetch('BookCopyManagement.aspx/DeleteBookCopy', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ bookId: bookId })
                })
                    .then(response => response.json())
                    .then(data => {
                        if (data.d === "Success") {
                            alert("Book Copy deleted successfully.");
                            location.reload();
                        } else {
                            alert(data);  // Display any error message returned from the server
                        }
                    })
                    .catch(error => {
                        alert("Error: " + error);
                    });
            }
        }

        function showAddBookCopyModal() {
            document.getElementById("addBookCopyModal").style.display = "block";
        }

        function hideAddBookCopyModal() {
            document.getElementById("addBookCopyModal").style.display = "none";
        }
    </script>
</asp:Content>
