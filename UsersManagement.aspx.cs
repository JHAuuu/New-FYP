using System;
using fyp.Authentication;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Data;
using Microsoft.AspNet.SignalR;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ZXing;

namespace fyp
{
    public partial class UsersManagement : AdminPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var master = this.Master as DashMasterPage;
            if (master != null)
            {
                master.titleText = "User Management";
            }

            if (!IsPostBack)
            {
                // Check if there's a success message in the session
                if (Session["SuccessMessage"] != null)
                {
                    MessageBox.Text = Session["SuccessMessage"].ToString();
                    Session.Remove("SuccessMessage"); // Clear the message after showing it
                }
                // Initialize the filter
                ViewState["SelectedFilter"] = ddlFilter.SelectedValue; // Store the initial filter value
                BindGrid(); // Bind the GridView initially
            }
        }

        [WebMethod]
        public static string GetUserData(string UserId)
        {
            string htmlResponse = "";

            // SQL query to fetch user details by UserId  
            string query = @"  
        SELECT   
            u.UserName,  
            b.BookTitle,  
            bc.BookCopyId,  
            l.LoanId,  
            l.StartDate,  
            l.EndDate,  
            l.Status,  
            pu.TotalFine  
        FROM   
            [User] u  
        INNER JOIN   
            Patron p ON u.UserId = p.UserId  
        INNER JOIN   
            Loan l ON p.PatronId = l.PatronId  
        INNER JOIN   
            BookCopy bc ON l.BookCopyId = bc.BookCopyId  
        INNER JOIN   
            Book b ON bc.BookId = b.BookId  
        LEFT JOIN   
            Punishment pu ON l.LoanId = pu.LoanId  
        WHERE   
            u.UserId = @UserId  
            AND l.Status IN('returning', 'preloaning')";

            // Parameters for the query  
            string[] arr = new string[2];
            arr[0] = "@UserId";
            arr[1] = UserId;

            // Execute the query  
            DataTable resultTable = DBHelper.ExecuteQuery(query, arr);

            // Check if any records were returned  
            if (resultTable.Rows.Count > 0)
            {
                // Loop through each row in the result table and construct the table rows dynamically  
                foreach (DataRow row in resultTable.Rows)
                {
                    string bookTitle = row["BookTitle"].ToString();
                    DateTime startDate = Convert.ToDateTime(row["StartDate"]);
                    DateTime endDate = Convert.ToDateTime(row["EndDate"]);
                    string loanStatus = row["Status"].ToString();
                    string totalFine = row["TotalFine"] != DBNull.Value ? "RM" + row["TotalFine"].ToString() : "RM0";
                    int loanId = Convert.ToInt32(row["LoanId"]);

                    // Calculate loan day difference  
                    int loanDay = (endDate - startDate).Days;

                    // Set the button text based on loanStatus  
                    string buttonText = loanStatus == "returning" ? "Return" : "Request Loan Book";
                    string buttonClickHandler = loanStatus == "returning"
                        ? $"updateLoanStatus({loanId}, 'returned')"
                        : $"updateLoanStatus({loanId}, 'loaning')";

                    // Construct each row of the table with the loan data  
                    htmlResponse += $@"  
                    <tr>  
                        <td>{bookTitle}</td>  
                        <td>{startDate:yyyy-MM-dd}</td>  
                        <td>{loanDay} days</td>  
                        <td>{totalFine}</td>  
                        <td>{loanStatus}</td>  
                        <td><button class='button' onclick=""{buttonClickHandler}; return false;"">{buttonText}</button></td>  
                    </tr>";
                }
            }
            else
            {
                htmlResponse = "<tr><td colspan='6'>No loan data available.</td></tr>";
            }

            // Log the final HTML response to check if data is being returned  
            Console.WriteLine(htmlResponse);  // Or use your preferred logging method  

            return htmlResponse;
        }

        [WebMethod]
        public static object UpdateLoanStatus(int loanId, string newStatus)
        {
            try
            {
                string query = @"
            UPDATE Loan
            SET Status = @Status
            WHERE LoanId = @LoanId";

                object[] arr = new object[4];
                arr[0] = "@Status";  // Corrected parameter name to match the SQL query
                arr[1] = newStatus;
                arr[2] = "@LoanId";  // Corrected parameter name to match the SQL query
                arr[3] = loanId;

                // Execute the query
                int rowsAffected = DBHelper.ExecuteNonQuery(query, arr);

                // Check if any rows were affected (should be 1 for a successful update)  
                bool updateSuccessful = rowsAffected == 1;

                return new { success = updateSuccessful };
            }
            catch (Exception ex)
            {
                return new { success = false, errorMessage = ex.Message };
            }

        }

        [System.Web.Services.WebMethod]
        public static object CheckUserExistsAndInsertUser(string userName, string userAddress, string userEmail, string educationLevel, string userPhoneNumber, string userPassword)
        {
            // Check if the username or email already exists
            string checkQuery = "SELECT UserName, UserEmail FROM [User] WHERE UserName = @UserName OR UserEmail = @UserEmail";
            string[] checkParams = { "@UserName", userName, "@UserEmail", userEmail };

            DataTable resultTable = DBHelper.ExecuteQuery(checkQuery, checkParams);

            if (resultTable.Rows.Count > 0)
            {
                string message = "";
                bool usernameExists = false;
                bool emailExists = false;

                foreach (DataRow row in resultTable.Rows)
                {
                    if (row["UserName"].ToString() == userName)
                        usernameExists = true;
                    if (row["UserEmail"].ToString() == userEmail)
                        emailExists = true;
                }

                if (usernameExists && emailExists)
                    message = "Both the username and email already exist. Please enter different values.";
                else if (usernameExists)
                    message = "The username already exists. Please enter a different one.";
                else if (emailExists)
                    message = "The email already exists. Please enter a different one.";

                return new { success = false, message };
            }

            // Hash the password
            string hashPass = encryption.HashPassword(userPassword);

            // Insert new user into the [User] table
            string insertUserQuery = "INSERT INTO [User] ([UserName], [UserAddress], [UserEmail], [UserPhoneNumber], [UserPassword]) " +
                                     "OUTPUT INSERTED.UserId VALUES (@UserName, @UserAddress, @UserEmail, @UserPhoneNumber, @UserPassword)";
            string[] insertUserParams = {
        "@UserName", userName,
        "@UserAddress", userAddress,
        "@UserEmail", userEmail,
        "@UserPhoneNumber", userPhoneNumber,
        "@UserPassword", hashPass
    };

            object userResult = DBHelper.ExecuteScalar(insertUserQuery, insertUserParams);

            if (userResult != null)
            {
                int userId = Convert.ToInt32(userResult);

                // Insert into Patron table
                string insertPatronQuery = "INSERT INTO Patron (EduLvl, UserId) VALUES (@EduLvl, @UserId); SELECT SCOPE_IDENTITY();";
                object[] insertPatronParams = {
                    "@EduLvl", educationLevel,
                    "@UserId", userId
                };

                object patronResult = DBHelper.ExecuteScalar(insertPatronQuery, insertPatronParams);

                if (patronResult != null)
                {
                    int patronId = Convert.ToInt32(patronResult);

                    // Insert Trustworthy data
                    string insertTrustQuery = "INSERT INTO Trustworthy (TrustScore, TrustLvl, PatronId) VALUES (@TrustScore, @TrustLvl, @PatronId)";
                    object[] trustParams = {
                        "@TrustScore", "100",
                        "@TrustLvl", "high",
                        "@PatronId", patronId
                    };

                    int trustRowsAffected = DBHelper.ExecuteNonQuery(insertTrustQuery, trustParams);

                    if (trustRowsAffected > 0)
                    {
                        return new { success = true, message = "Registration successful!" };
                    }
                    else
                    {
                        return new { success = false, message = "Error occurred while saving Trust data." };
                    }
                }
                else
                {
                    return new { success = false, message = "Error occurred while registering Patron data." };
                }
            }
            else
            {
                return new { success = false, message = "Error occurred while registering user." };
            }
        }

        private void BindGrid()
        {
            // Get the selected filter value from ViewState
            string selectedValue = ViewState["SelectedFilter"] as string;

            // Define the base SQL query for users with UserRole = 'User'
            string baseQuery = @"
            SELECT DISTINCT 
                CAST([User].UserId AS INT) AS UserId, 
                [User].UserName, 
                [User].UserAddress, 
                [User].UserEmail, 
                [User].UserPhoneNumber 
            FROM [User] 
            WHERE [UserRole] IN ('Student', 'Teacher') AND [IsDeleted] = 0";

            try
            {
                // Modify query based on selected filter
                if (selectedValue == "Loan")
                {
                    SqlDataSource1.SelectCommand = baseQuery + @"
                AND [User].UserId IN (
                    SELECT DISTINCT CAST(U.UserId AS INT) 
                    FROM [User] AS U
                    INNER JOIN Patron AS P ON U.UserId = P.UserId
                    INNER JOIN Loan AS L ON P.PatronId = L.PatronId
                    WHERE L.Status = 'loaning'
                )";
                }
                else if (selectedValue == "Overdue")
                {
                    SqlDataSource1.SelectCommand = baseQuery + @"
                    AND [User].UserId IN (
                        SELECT DISTINCT CAST(U.UserId AS INT)
                        FROM [User] AS U
                        INNER JOIN Patron AS P ON U.UserId = P.UserId
                        INNER JOIN Loan AS L ON P.PatronId = L.PatronId
                        WHERE L.EndDate < CAST(GETDATE() AS DATE) 
                        AND L.LatestReturn IS NOT NULL
                    )";
                }
                else
                {
                    SqlDataSource1.SelectCommand = baseQuery;
                }

                // Re-bind the GridView to apply the filter
                GridView1.DataBind();
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", $"alert('Error: {ex.Message}');", true);
            }
        }

        protected void ddlFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selected filter value
            string selectedValue = ddlFilter.SelectedValue;
            ViewState["SelectedFilter"] = selectedValue; // Update the ViewState with the new filter

            // Re-bind the GridView with the updated filter
            BindGrid();
        }

        protected void GridView1_RowEditing(object sender, GridViewEditEventArgs e)
        {
            // Set the edit index to the selected row
            GridView1.EditIndex = e.NewEditIndex;

            // Re-bind the GridView to apply the current filter in edit mode
            BindGrid();
        }

        protected void GridView1_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            // Cancel edit mode
            GridView1.EditIndex = -1;

            // Re-bind the GridView to apply the current filter
            GridView1.DataBind();
        }

        protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            // Update the user data in the data source
            SqlDataSource1.Update();

            // Reset the edit index
            GridView1.EditIndex = -1;

            // Re-bind the GridView with the current filter value from ViewState
            BindGrid();
        }

        [WebMethod]
        public static Result SendInboxMessage(int userId, string inboxTitle, string inboxContent)
        {
            try
            {
                string insertQuery = @"
                    INSERT INTO Inbox (InboxTitle, InboxContent, UserId) 
                    VALUES (@InboxTitle, @InboxContent, @UserId);
                    SELECT SCOPE_IDENTITY();"; // Retrieve the newly inserted InboxId

                object[] insertParams = {
                    "@InboxTitle", inboxTitle,
                    "@InboxContent", inboxContent,
                    "@UserId", userId
                };

                // Execute the insert query and get the InboxId
                int newInboxId = Convert.ToInt32(DBHelper.ExecuteScalar(insertQuery, insertParams));

                if (newInboxId > 0)
                {
                    // Query to retrieve SendAt based on the new InboxId
                    string selectQuery = "SELECT SendAt FROM Inbox WHERE InboxId = @InboxId";
                    object[] selectParams = { "@InboxId", newInboxId };

                    // Execute the query to get SendAt
                    DateTime sendAt = Convert.ToDateTime(DBHelper.ExecuteScalar(selectQuery, selectParams));

                    // Notify the specific user using SignalR
                    var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                    context.Clients.Client(NotificationHub.GetConnectionId(userId))
                    .broadcastInbox(inboxTitle, inboxContent, sendAt.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    // Handle the case where the insert failed
                    throw new Exception("Failed to insert inbox message.");
                }

                return new Result { Success = true, Message = "Inbox message sent successfully!" };

            }
            catch (Exception ex)
            {
                return new Result { Success = false, Message = "An error occurred: " + ex.Message };
            }
        }

        public class Result
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DownloadBarcode")
            {
                int userId = int.Parse(e.CommandArgument.ToString());
                GenerateAndDownloadQRCode(userId.ToString());
            }
        }

        private void GenerateAndDownloadQRCode(string userId)
        {
            try
            {
                // Create a QR code writer instance
                BarcodeWriter writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE, // QR Code format
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Width = 300, // Width of the QR Code
                        Height = 300, // Height of the QR Code
                        Margin = 10 // Margin around the QR Code
                    }
                };

                // Generate QR Code as a bitmap
                Bitmap qrCodeBitmap = writer.Write(userId);

                // Save to memory stream as PNG
                using (MemoryStream ms = new MemoryStream())
                {
                    qrCodeBitmap.Save(ms, ImageFormat.Png);
                    byte[] qrCodeBytes = ms.ToArray();

                    // Set response headers for file download
                    Response.Clear();
                    Response.ContentType = "image/png";
                    Response.AddHeader("Content-Disposition", $"attachment; filename=User_{userId}_QRCode.png");
                    Response.BinaryWrite(qrCodeBytes);
                    Response.End();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Response.Write($"Error generating QR code: {ex.Message}");
            }
        }

    }
}