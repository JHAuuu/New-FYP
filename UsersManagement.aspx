<%@ Page Title="UsersManagement" Language="C#" MasterPageFile="~/Master/DashMasterPage.Master" AutoEventWireup="true" CodeBehind="UsersManagement.aspx.cs" Inherits="fyp.UsersManagement" %>

<asp:Content ID="content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .user-list {
            margin-top: 8px;
            background-color: #fff;
            width: 100%;
            border-collapse: collapse;
            border-radius: 5px;
            border-style: hidden;
            box-shadow: 0 0 0 2px #666;
        }

            .user-list td,
            .user-list th {
                padding: 1rem;
                font-weight: 600;
                text-align: center;
                border-bottom: 1px solid #ddd;
            }

            .user-list th {
                border-bottom: 2px solid #000000;
            }


            .user-list .action-icons a {
                text-decoration: none;
                font-size: 18px;
                margin: 0 5px;
                color: #333;
            }

        .modal {
            display: none; /* Hidden by default */
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


        #filter {
            padding: 3px 10px;
            font-size: 13px;
            border-radius: 5px;
        }

        .filter-btn {
            padding: 3px 10px;
            font-size: 13px;
            background-color: #4CAF50;
            color: white;
            border: none;
            border-radius: 5px;
        }

            .filter-btn:hover {
                background-color: #419544;
                transition: 300ms;
            }

        .title {
            display: flex;
            justify-content: space-between;
        }

        .add-user-button {
            padding: 5px 92px;
            font-size: 13px;
            background-color: #8390A2;
            color: white;
            border: none;
            border-radius: 5px;
        }

            .add-user-button:hover {
                background-color: #58606c;
                transition: 300ms;
            }

            .action-button {
        margin-right: 5px; /* Add a 5px gap between buttons */
        display: inline-block; /* Ensure they stay inline */
    }
            .education-level-dropdown {
            margin-bottom: 20px;
            width: 100%;
            padding: 10px;
            border: 1px solid #ccc;
            border-radius: 5px;
            font-size: 14px;
            background-color: #fff;
            color: #333;
            transition: border-color 0.3s;
        }

            /* Hover effect for dropdown */
            .education-level-dropdown:hover {
                border-color: #5cb85c;
            }

            /* Focus effect for dropdown */
            .education-level-dropdown:focus {
                border-color: #66afe9;
                outline: none;
            }
    </style>

    <h3>User list</h3>

    <div class="title">
        <asp:DropDownList ID="ddlFilter" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlFilter_SelectedIndexChanged">
            <asp:ListItem Value="All">All Users</asp:ListItem>
            <asp:ListItem Value="Loan">Book loaning users</asp:ListItem>
            <asp:ListItem Value="Overdue">Users who loan overdue books</asp:ListItem>
        </asp:DropDownList>

        <asp:Button ID="btnAddUser" runat="server" CssClass="add-user-button" Text="Register Users" OnClientClick="showAddUserModal(); return false;" />

        <div id="addUserModal" class="modal" style="display: none;">
            <div class="modal-content">
                <span class="close" onclick="hideAddUserModal()">&times;</span>
                <h3 style="text-align: center; color: #333;">Register New User</h3>

                <asp:Label ID="lblErrorMessage" runat="server" ForeColor="Red" CssClass="error-message"></asp:Label>

                <asp:Label ID="lblUserName" runat="server" AssociatedControlID="txtUserName" Text="User Name:" CssClass="form-label"></asp:Label>
                <asp:TextBox ID="txtUserName" runat="server" Placeholder="User Name" CssClass="input-field" /><br />
                <asp:Label ID="lblUserAddress" runat="server" AssociatedControlID="txtUserAddress" Text="User Address:" CssClass="form-label"></asp:Label>
                <asp:TextBox ID="txtUserAddress" runat="server" Placeholder="User Address" CssClass="input-field" /><br />
                <asp:Label ID="lblUserEmail" runat="server" AssociatedControlID="txtUserEmail" Text="User Email:" CssClass="form-label"></asp:Label>
                <asp:TextBox ID="txtUserEmail" runat="server" Placeholder="User Email" CssClass="input-field" /><br />
                <label class="education-level">Education Level</label>
            <asp:RequiredFieldValidator ID="rfvEducationLevel" runat="server" ControlToValidate="ddlEducationLevel" InitialValue="" ErrorMessage="*Please select an education level" CssClass="error-message" Display="Dynamic"></asp:RequiredFieldValidator><br />
            <asp:DropDownList ID="ddlEducationLevel" runat="server" CssClass="education-level-dropdown">
                <asp:ListItem Text="Diploma" Value="Diploma" />
                <asp:ListItem Text="Degree" Value="Degree" />
                <asp:ListItem Text="Master" Value="Master" />
            </asp:DropDownList><br />
                <asp:Label ID="lblUserPhoneNumber" runat="server" AssociatedControlID="txtUserPhoneNumber" Text="User Phone Number:" CssClass="form-label"></asp:Label>
                <asp:TextBox ID="txtUserPhoneNumber" runat="server" Placeholder="User Phone Number" CssClass="input-field" /><br />
                <asp:Label ID="lblUserPassword" runat="server" AssociatedControlID="txtUserPassword" Text="User Password:" CssClass="form-label"></asp:Label>
                <asp:TextBox ID="txtUserPassword" runat="server" Placeholder="User Password" TextMode="Password" CssClass="input-field" /><br />
                <br />

                <asp:Button ID="btnSubmitUser" runat="server" Text="Submit" CssClass="button" UseSubmitBehavior="false" OnClientClick="validateAndSubmitUser(); return false;" />
            </div>
        </div>
    </div>
    <asp:Label ID="MessageBox" runat="server" ForeColor="Red"></asp:Label>
    <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:ConnectionString %>" SelectCommand="SELECT [UserId], [UserName], [UserAddress], [UserEmail], [UserPhoneNumber] FROM [User] WHERE ([UserRole] IN ('Student', 'Teacher') AND [IsDeleted] = 0)" UpdateCommand="UPDATE [User]
SET 
    [UserName] = @UserName,
    [UserAddress] = @UserAddress,
    [UserEmail] = @UserEmail,
    [UserPhoneNumber] = @UserPhoneNumber
WHERE 
    [UserId] = @UserId;">
        <UpdateParameters>
            <asp:Parameter Name="UserName" />
            <asp:Parameter Name="UserAddress" />
            <asp:Parameter Name="UserEmail" />
            <asp:Parameter Name="UserPhoneNumber" />
            <asp:Parameter Name="UserId" />
        </UpdateParameters>
    </asp:SqlDataSource>

    <asp:GridView ID="GridView1" runat="server" OnRowEditing="GridView1_RowEditing"
        OnRowCancelingEdit="GridView1_RowCancelingEdit"
        OnRowCommand="GridView1_RowCommand" OnRowUpdating="GridView1_RowUpdating" AllowPaging="True" AllowSorting="True" AutoGenerateColumns="False"
        CssClass="user-list" CellPadding="4" DataSourceID="SqlDataSource1" DataKeyNames="UserId" ForeColor="#8390A2">
        <Columns>
            <asp:BoundField DataField="UserId" HeaderText="UserId" SortExpression="UserId" ReadOnly="True" />
            <asp:BoundField DataField="UserName" HeaderText="Name" SortExpression="UserName" />
            <asp:BoundField DataField="UserAddress" HeaderText="Address" SortExpression="UserAddress" />
            <asp:BoundField DataField="UserEmail" HeaderText="Email" SortExpression="UserEmail" />
            <asp:BoundField DataField="UserPhoneNumber" HeaderText="Phone Number" SortExpression="UserPhoneNumber" />

            <asp:TemplateField HeaderText="Barcode">
                            <ItemTemplate>
                                <asp:Button ID="btnDownloadBarcode" runat="server" Text="Download User's QRcode" CssClass="barcode-button"
                                    CommandName="DownloadBarcode" CommandArgument='<%# Eval("UserId") %>' CausesValidation="false" />
                            </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Action">
                <ItemTemplate>
                    <asp:LinkButton ID="EditButton" runat="server" CommandName="Edit" CssClass="action-button">
                    <i class="las la-user-edit"></i>
                    </asp:LinkButton>
                    <a href="javascript:void(0);" onclick="window.location.href='UserHistory.aspx?UserId=<%# Eval("UserId") %>'" class="action-button">
                        <i class="las la-history"></i>
                    </a>
                    <a href="javascript:void(0);" onclick="showInboxModal('<%# Eval("UserId") %>')" class="action-button">
                        <i class="las la-inbox"></i>
                    </a>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:LinkButton ID="UpdateButton" runat="server" CommandName="Update" Text="Update" CssClass="button" />
                    <asp:LinkButton ID="CancelButton" runat="server" CommandName="Cancel" Text="Cancel" CssClass="button" />
                </EditItemTemplate>
            </asp:TemplateField>
        </Columns>
        <HeaderStyle ForeColor="Black" />
    </asp:GridView>
    <div id="inboxModal" class="modal" style="display: none;">
        <div class="modal-content">
            <span class="close" onclick="hideInboxModal()">&times;</span>
            <h3 style="text-align: center; color: #333;">Send Inbox Message</h3>

            <asp:Label ID="lblInboxErrorMessage" runat="server" ForeColor="Red" CssClass="error-message"></asp:Label>
            <asp:TextBox ID="txtInboxTitle" runat="server" Placeholder="Inbox Title" CssClass="input-field" /><br />
            <asp:TextBox ID="txtInboxContent" runat="server" Placeholder="Inbox Content" CssClass="input-field" TextMode="MultiLine" Rows="4" /><br />

            <asp:Button ID="btnSendInbox" runat="server" Text="Send" CssClass="button" UseSubmitBehavior="false" OnClientClick="validateAndSendInbox(); return false;" />
        </div>
    </div>
    <asp:HiddenField ID="hfUserId" runat="server" />
    <script>
        // Show the inbox modal with the userId parameter
        function showInboxModal(userId) {
            console.log("Modal triggered with UserId:", userId);
            document.getElementById('inboxModal').style.display = 'block';
            document.getElementById('<%= hfUserId.ClientID %>').value = userId; // Store the userId for later submission
        }

        // Hide the inbox modal
        function hideInboxModal() {
            document.getElementById('inboxModal').style.display = 'none';
        }

        // Validate and submit the inbox message
        function validateAndSendInbox() {
            const inboxTitle = document.getElementById('<%= txtInboxTitle.ClientID %>').value;
            const inboxContent = document.getElementById('<%= txtInboxContent.ClientID %>').value;

            // Validate fields
            if (!inboxTitle || !inboxContent) {
                document.getElementById('<%= lblInboxErrorMessage.ClientID %>').innerText = "Both title and content are required.";
                return;
            }

            // Call backend method to insert the message
            fetch('UsersManagement.aspx/SendInboxMessage', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    userId: document.getElementById('<%= hfUserId.ClientID %>').value,
                    inboxTitle: inboxTitle,
                    inboxContent: inboxContent
                })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.d.Success) {
                        // Clear any previous error messages
                        document.getElementById('<%= lblInboxErrorMessage.ClientID %>').innerText = "";
                        // Success: Close the modal and show a success message
                        alert(data.d.Message); // Use 'data.d.Message' as the success message
                        hideInboxModal();
                    } else {
                        // Display error message
                        document.getElementById('<%= lblInboxErrorMessage.ClientID %>').innerText = data.d.Message;
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    // Display the error in lblInboxErrorMessage
                    document.getElementById('<%= lblInboxErrorMessage.ClientID %>').innerText = "An unexpected error occurred. Please try again.";
                });
        }


        //register user button action
        function showAddUserModal() {
            document.getElementById('addUserModal').style.display = 'block';
        }

        function hideAddUserModal() {
            document.getElementById('addUserModal').style.display = 'none';
        }

        function validateAndSubmitUser() {
            const userName = document.getElementById('<%= txtUserName.ClientID %>').value;
            const userAddress = document.getElementById('<%= txtUserAddress.ClientID %>').value;
            const userEmail = document.getElementById('<%= txtUserEmail.ClientID %>').value;
            const educationLevel = document.getElementById('<%= ddlEducationLevel.ClientID %>').value;
            const userPhoneNumber = document.getElementById('<%= txtUserPhoneNumber.ClientID %>').value;
            const userPassword = document.getElementById('<%= txtUserPassword.ClientID %>').value;

            // Validate All Fields Are Not Empty
            if (!userName || !userAddress || !userEmail || !userPhoneNumber || !userPassword) {
                document.getElementById('<%= lblErrorMessage.ClientID %>').innerText = "All fields are required and cannot be empty.";
                return;
            } else {
                // Validate Username
                if (userName.length < 3) {
                    document.getElementById('<%= lblErrorMessage.ClientID %>').innerText = "Username must be at least 3 characters long.";
                    return;
                }

                // Validate Email Format
                const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/; // Simple regex for email validation
                if (!emailPattern.test(userEmail)) {
                    document.getElementById('<%= lblErrorMessage.ClientID %>').innerText = "Please enter a valid email address.";
                    return;
                }

                // Validate Phone Number (Malaysian format)
                const phonePattern = /^(01[0-9]{8,9})$/; // Matches Malaysian phone numbers starting with 01 followed by 7 to 9 digits
                if (!phonePattern.test(userPhoneNumber)) {
                    document.getElementById('<%= lblErrorMessage.ClientID %>').innerText = "Phone number must match Malaysian format (e.g., 0192485083 or 01155005083).";
                    return;
                }
            }

            fetch('UsersManagement.aspx/CheckUserExistsAndInsertUser', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    userName: userName,
                    userAddress: userAddress,
                    userEmail: userEmail,
                    educationLevel: educationLevel,
                    userPhoneNumber: userPhoneNumber,
                    userPassword: userPassword
                })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.d.success) {
                        // Display success message or reload the page
                        alert(data.d.message);
                        location.reload();
                    } else {
                        // Display the error message in the modal
                        document.getElementById('<%= lblErrorMessage.ClientID %>').innerText = data.d.message;
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                });
        }
    </script>
</asp:Content>


