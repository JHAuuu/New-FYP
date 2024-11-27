<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Master/DashMasterPage.Master" AutoEventWireup="true" CodeBehind="DashManagement.aspx.cs" Inherits="fyp.DashManagement" %>

<asp:Content ID="content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .card {
            background-color: #ffffff;
            padding: 20px;
            margin: 20px 0;
            border-radius: 8px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
            transition: all 0.3s ease;
        }

            .card:hover {
                box-shadow: 0 6px 12px rgba(0, 0, 0, 0.2);
                transform: translateY(-5px);
            }

            .card h2 {
                font-size: 24px;
                margin: 0 0 10px 0;
                color: #333;
                font-weight: 600;
            }

            .card p {
                font-size: 16px;
                color: #555;
                line-height: 1.6;
            }

        #dateRangeSection {
            margin: 20px 0;
            display: flex;
            flex-direction: column;
            gap: 10px;
        }

            #dateRangeSection label {
                font-size: 14px;
                font-weight: 500;
                color: #555;
            }

            #dateRangeSection input[type="date"] {
                padding: 10px;
                font-size: 16px;
                border: 1px solid #ccc;
                border-radius: 4px;
                width: 100%;
                max-width: 250px;
            }

        .btn {
            padding: 10px 20px;
            border-radius: 4px;
            font-size: 16px;
            cursor: pointer;
            border: none;
            transition: background-color 0.3s ease;
            display: inline-block;
            text-align: center;
            width: auto;
        }

        .btn-primary {
            background-color: #007bff;
            color: white;
        }

            .btn-primary:hover {
                background-color: #0056b3;
            }

        .btn-secondary {
            background-color: #6c757d;
            color: white;
        }

            .btn-secondary:hover {
                background-color: #5a6268;
            }

        button:disabled {
            background-color: #ccc;
            cursor: not-allowed;
        }

        .dashboard {
            display: flex;
            height: 110px;
            gap: 20px;
        }

        .card1 {
            width: 33%;
            height: 100%; /* Adjust this height to make the card shorter */
            padding: 10px 15px;
            color: white;
            border-radius: 5px;
            box-shadow: 0 2px 5px rgba(0, 0, 0, 0.2);
            position: relative;
            display: flex;
            flex-direction: column;
            justify-content: space-between;
            background-color: #8390A2;
        }

            .card1 .number {
                font-size: 20px;
                font-weight: 600;
                margin-bottom: 2px;
            }

            .card1 .label {
                font-size: 16px;
                margin-bottom: 2px; /* Adjusted margin for better spacing */
            }

            .card1 .icon {
                font-size: 40px;
                opacity: 0.3;
                position: absolute;
                top: 5px;
                right: 10px;
            }

            .card1 .more-info {
                display: flex;
                justify-content: space-between;
                align-items: center;
                padding: 3px 5px;
                background-color: rgba(0, 0, 0, 0.1); /* Light background color */
                border-radius: 3px;
                transition: background-color 0.3s;
                cursor: pointer;
            }

                .card1 .more-info:hover {
                    background-color: rgba(0, 0, 0, 0.2); /* Darker background on hover */
                }
    </style>
    <div class="dashboard">
        <div class="card1">
            <asp:Label ID="lblStaffNum" runat="server" class="number"></asp:Label>
            <div class="label">Library Staff</div>
            <div class="icon"><i class="las la-user-tie"></i></div>
            <div class="more-info" onclick="redirectToStaffManagement()">
    <span>More info</span>
    <span>➔</span>
</div>
        </div>
        <div class="card1">
            <asp:Label ID="lblUserNum" runat="server" class="number"></asp:Label>
            <div class="label">User</div>
            <div class="icon"><i class="las la-users"></i></div>
            <div class="more-info" onclick="redirectToUserManagement()">
                <span>More info</span>
                <span>➔</span>
            </div>
        </div>
    </div>

    <div class="card" id="borrowedReport">
        <h2>Book Borrowed Report</h2>
        <p>Here you can view the report of all the borrowed books.</p>

        <!-- Time Range Selection (Initially Hidden) -->
        <div id="dateRangeSection">
            <label for="startDate">Start Date:</label>
            <asp:TextBox ID="txtStartDate" runat="server" TextMode="Date" CssClass="date-picker" OnChange="updateEndDateMin('<%= txtStartDate.ClientID %>', '<%= txtEndDate.ClientID %>')" />

            <label for="endDate">End Date:</label>
            <asp:TextBox ID="txtEndDate" runat="server" TextMode="Date" CssClass="date-picker" />
        </div>

        <div>
            <asp:Button ID="btnGenerateReport" runat="server" Text="Generate Report" CssClass="btn btn-primary" OnClick="btnGenerateReport_Click"/>
            <asp:Button ID="btnViewBorrow" runat="server" Text="View Report" CssClass="btn btn-secondary" OnClick="btnViewBorrow_Click" />
        </div>
    </div>

    <div class="card" id="addedReport">
        <h2>Book Added Report</h2>
        <p>Here you can view the report of all the newly added books.</p>
        <div id="dateRangeSection">
            <label for="startDate">Start Date:</label>
            <asp:TextBox ID="txtBookStartDate" runat="server" TextMode="Date" CssClass="date-picker" OnChange="updateEndDateMin('<%= txtBookStartDate.ClientID %>', '<%= txtBookEndDate.ClientID %>')" />

            <label for="endDate">End Date:</label>
            <asp:TextBox ID="txtBookEndDate" runat="server" TextMode="Date" CssClass="date-picker" />
        </div>

        <div>
            <asp:Button ID="btnGenerateAddBook" runat="server" Text="Generate Report" CssClass="btn btn-primary" OnClick="btnGenerateAddBook_Click"/>
            <asp:Button ID="btnViewAddBook" runat="server" Text="View Report" CssClass="btn btn-secondary" OnClick="btnViewAddBook_Click" />
        </div>
    </div>
    <script>
        window.onload = function () {
            var today = new Date();
            var formattedDate = today.toISOString().split('T')[0]; // Get the date in YYYY-MM-DD format

            // Apply max date restriction for both start and end date fields
            applyMaxDateRestriction('<%= txtStartDate.ClientID %>');
            applyMaxDateRestriction('<%= txtEndDate.ClientID %>');
            applyMaxDateRestriction('<%= txtBookStartDate.ClientID %>');
            applyMaxDateRestriction('<%= txtBookEndDate.ClientID %>');

            // Set the initial min date of the end date field based on the start date for both ranges
            updateEndDateMin('<%= txtStartDate.ClientID %>', '<%= txtEndDate.ClientID %>');
            updateEndDateMin('<%= txtBookStartDate.ClientID %>', '<%= txtBookEndDate.ClientID %>');

        // Add onchange event listeners to start date fields to dynamically set end date minimums
        document.getElementById('<%= txtStartDate.ClientID %>').addEventListener("change", function () {
            updateEndDateMin('<%= txtStartDate.ClientID %>', '<%= txtEndDate.ClientID %>');
        });
        document.getElementById('<%= txtBookStartDate.ClientID %>').addEventListener("change", function () {
            updateEndDateMin('<%= txtBookStartDate.ClientID %>', '<%= txtBookEndDate.ClientID %>');
        });
        };

        // Function to apply max date restriction to the date fields
        function applyMaxDateRestriction(elementId) {
            var today = new Date();
            var formattedDate = today.toISOString().split('T')[0]; // Get the date in YYYY-MM-DD format
            document.getElementById(elementId).max = formattedDate;
        }

        // Function to update the minimum date of the end date field based on the start date
        function updateEndDateMin(startDateElementId, endDateElementId) {
            var startDate = document.getElementById(startDateElementId).value;

            if (startDate) {
                // Set the min date of the end date field to the selected start date
                document.getElementById(endDateElementId).min = startDate;
            }
        }

        function redirectToStaffManagement() {
            window.location.href = 'StaffManagement.aspx';
        }
        function redirectToUserManagement() {
            window.location.href = 'UsersManagement.aspx';
        }
    </script>
</asp:Content>
