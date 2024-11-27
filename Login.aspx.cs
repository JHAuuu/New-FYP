using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Windows;
using System.Web.Security;
using System.Security.Claims;
using Microsoft.Owin.Security;


namespace fyp
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (User.Identity.IsAuthenticated)
                {
                    // Use ClaimsPrincipal to retrieve user claims
                    var identity = (System.Security.Claims.ClaimsPrincipal)User;

                    // Extract claims from the identity
                    string userId = identity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    string userRole = identity.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                    // Redirect based on the role
                    if (userRole == "Admin" || userRole == "Staff")
                    {
                        Response.Redirect("DashManagement.aspx");
                    }
                    else
                    {
                        try
                        {
                            string patronQuery = @"SELECT PatronId
                              FROM Patron
                            WHERE UserId = @userId
                            ";
                            int getPatron = Convert.ToInt32(DBHelper.ExecuteScalar(patronQuery, new string[]{
                        "userId", userId
                        }));

                            Session["PatronId"] = getPatron;
                        }
                        catch (Exception ex)
                        {

                        }
                        Response.Redirect("Home.aspx");
                    }
                }
            }
            
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUserName.Text.Trim();
            string password = txtPass.Text.Trim();
            // Check if username and password are provided
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Text = "Please enter both username and password.";
            }
            else
            {
                string queryFindUser = "SELECT * FROM [User] Where username = @userName";
                string[] arrFindUser = new string[2];
                arrFindUser[0] = "@userName";
                arrFindUser[1] = username;
                DataTable dt = DBHelper.ExecuteQuery(queryFindUser, arrFindUser);

                String uname;
                bool lockStatus;
                DateTime lockdatetime = DateTime.Now;

                if (dt.Rows.Count == 0)
                {
                    // No matching username found
                    MessageBox.Text = "Invalid Username Or Password! Please Try Again.";
                }
                else
                {
                    uname = dt.Rows[0]["UserName"].ToString();
                    if (dt.Rows[0]["locked"].ToString() == "1")
                    {
                        lockStatus = true;
                    }
                    else
                    {
                        lockStatus = false;
                    }

                    if (lockStatus == true)
                    {
                        lockdatetime = Convert.ToDateTime(dt.Rows[0]["lockDateTime"].ToString());
                        lockdatetime = Convert.ToDateTime(lockdatetime.ToString("dd/MM/yyyy HH:mm:ss"));

                    }
                    if (lockStatus == true)
                    {
                        DateTime dateTime = Convert.ToDateTime(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                        TimeSpan ts = dateTime.Subtract(lockdatetime);
                        Int32 minutesLocked = Convert.ToInt32(ts.TotalMinutes);
                        Int32 pendingminutes = 5 - minutesLocked;
                        if (pendingminutes <= 0)
                        {
                            unlockAccount();
                        }
                        else
                        {
                            MessageBox.Text = "Your Account has been locked for 5 minutes for 3 Invalid Attempts. It will be automatically unlocked with " + pendingminutes + " Minutes";
                        }

                    }
                    else
                    {
                        string storedPassword = dt.Rows[0]["UserPassword"].ToString();

                        if (encryption.IsPasswordMatch(storedPassword, password))
                        {
                            // Passwords match, perform actions (set session variables, redirect, etc.)
                            Session["UserRole"] = dt.Rows[0]["UserRole"].ToString();
                            Session["UserId"] = dt.Rows[0]["UserId"].ToString();
                            Session["UserName"] = dt.Rows[0]["UserName"].ToString();
                            // Retrieve user information
                            string userId = dt.Rows[0]["UserId"].ToString();
                            string userRole = dt.Rows[0]["UserRole"].ToString();

                            // Create claims for the authenticated user
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, username),         // Store the username
                                new Claim(ClaimTypes.NameIdentifier, userId), // Store the user ID
                                new Claim(ClaimTypes.Role, userRole)          // Store the user role
                            };
                            // Create an identity with the claims
                            var identity = new ClaimsIdentity(claims, "ApplicationCookie");

                            // Access the OWIN authentication manager
                            var authManager = Request.GetOwinContext().Authentication;

                            if (chbRemember.Checked)
                            {
                                // Persistent authentication
                                authManager.SignIn(new AuthenticationProperties
                                {
                                    IsPersistent = true,                     // Persistent cookie
                                    ExpiresUtc = DateTime.UtcNow.AddDays(2)  // Cookie expires in 2 days
                                }, identity);
                            }
                            else
                            {
                                // Session-based authentication
                                authManager.SignIn(new AuthenticationProperties
                                {
                                    IsPersistent = false                     // Non-persistent cookie
                                }, identity);
                            }
                                
                            if (Session["UserRole"]?.ToString() == "Student" || Session["UserRole"]?.ToString() == "Teacher")
                            {
                                SetPatron(dt.Rows[0]["UserId"].ToString());
                                Response.Redirect("Home.aspx");
                            }
                            else if (Session["UserRole"]?.ToString() == "Admin" || Session["UserRole"]?.ToString() == "Staff")
                            {
                                SetPatron(dt.Rows[0]["UserId"].ToString());
                                Response.Redirect("Home.aspx");
                                //Response.Redirect("DashManagement.aspx");
                            }
                        }
                        else
                        {
                            int attemptCount;
                            if (Session["invalidloginattempt"] != null)
                            {
                                attemptCount = Convert.ToInt16(Session["invalidloginattempt"].ToString());
                                attemptCount = attemptCount + 1;
                            }
                            else
                            {
                                attemptCount = 1;
                            }
                            Session["invalidloginattempt"] = attemptCount;
                            if (attemptCount == 3)
                            {
                                MessageBox.Text = "Your Account has been locked for 5 minutes for 3 Invalid Attempts. It will be automatically unlocked with 5 Minutes";
                                changeLockStatus();
                            }
                            else
                            {
                                MessageBox.Text = "Invalid Username Or Password! Please Try Again. You still have " + (3 - attemptCount) + " times to login";
                            }
                        }
                    }

                }

            }

            
        }

        private void SetPatron(string userId)
        {
            try
            {
                string patronQuery = @"SELECT PatronId
      ,EduLvl
      ,UserId
  FROM Patron
WHERE UserId = @userId
";

                if (!String.IsNullOrEmpty(Session["UserId"].ToString()))
                {
                    string userid = Session["UserId"].ToString();
                    int getPatron = Convert.ToInt32(DBHelper.ExecuteScalar(patronQuery, new string[]{
                        "userId", userId
                    }));

                    Session["PatronId"] = getPatron;


                }



            }
            catch(Exception ex)
            {

            }
        }

        void changeLockStatus()
        {
            // Define the update query with placeholders for parameters
            string updateQuery = "UPDATE [User] SET locked = @locked, lockDateTime = @lockDateTime WHERE username = @username";

            // Set the username and lock date values
            string username = txtUserName.Text.Trim();
            DateTime lockDateTime = DateTime.Now;

            // Execute the update query using DBHelper.ExecuteNonQuery
            int rowsAffected = fyp.DBHelper.ExecuteNonQuery(
                updateQuery,
                "@locked", 1,                     // Sets locked = 1
                "@lockDateTime", lockDateTime,     // Sets lockDateTime to the current date and time
                "@username", username              // Filters by username
            );

            // Optional: Check if the update was successful
            if (rowsAffected > 0)
            {
                Console.WriteLine("User lock status updated successfully.");
            }
            else
            {
                Console.WriteLine("User not found or update failed.");
            }
        }

        void unlockAccount()
        {
            // Define the update query with placeholders for parameters
            string updateQuery = "UPDATE [User] SET locked = @locked, lockDateTime = @lockDateTime WHERE UserName = @UserName";

            // Set parameter values
            string username = txtUserName.Text.Trim();

            // Execute the update query using DBHelper.ExecuteNonQuery
            int rowsAffected = fyp.DBHelper.ExecuteNonQuery(
                updateQuery,
                "@locked", 0,                    // Sets locked = 0 to unlock
                "@lockDateTime", DBNull.Value,    // Sets lockDateTime to NULL
                "@UserName", username             // Filters by UserName
            );
        }

        

    }
    
}