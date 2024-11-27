using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class changePassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Retrieve the email from the query string
            string email = Request.QueryString["email"];

            if (!string.IsNullOrEmpty(email))
            {
                // Display the email in the label
                /*Label1.Text = $"Resetting password for: {email}"*/
                ;
            }
            else
            {
                //Label1.Text = "Invalid email address.";
            }
        }

        protected void btnSPass_Click(object sender, EventArgs e)
        {
            // Retrieve email and new password from the form
            string UserEmail = Request.QueryString["email"];
            string newPassword = newPass.Text;
            try
            {
                if (string.IsNullOrEmpty(newPassword))
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Please provide a valid email and password.');", true);
                    return;
                }
                else
                {
                    String hashPass = encryption.HashPassword(newPassword);
                    string updateQuery = "UPDATE [User] SET [UserPassword] = @UserPassword WHERE [UserEmail] = @UserEmail";
                    object[] updateParams = {
                    "@UserPassword", hashPass,
                    "@UserEmail", UserEmail
                };

                    // Execute the update query
                    int rowsAffected = fyp.DBHelper.ExecuteNonQuery(updateQuery, updateParams);

                    // Optionally, check if the update was successful
                    if (rowsAffected > 0)
                    {
                        ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Password has been successfully updated.');", true);
                        Response.Redirect("Login.aspx");
                    }
                    else
                    {
                        // Update failed
                        ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('User with this email does not exist.');", true);
                    }
                }
            }catch(Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", $"alert('An error occurred: {ex.Message}');", true);
            }
            

        }


    }
}