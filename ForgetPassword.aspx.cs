using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Net.Mail;


namespace fyp
{
    public partial class ForgetPassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnSEmail_Click(object sender, EventArgs e)
        {
            string email = tBEmail.Text.Trim();

            try
            {
                SendResetEmail(email);
                // Optionally display success message
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void SendResetEmail(string email)
        {
            string emailSend = "chongwb-wm21@student.tarc.edu.my";
            string appPassword = "fmeh bnmi pdgc wpxp";

            ICredentialsByHost credentials = new NetworkCredential(emailSend, appPassword);

            SmtpClient smtpClient = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = credentials
            };

            // Create the email content
            MailMessage mail = new MailMessage
            {
                From = new MailAddress("chongwb-wm21@student.tarc.edu.my"),
                Subject = "Password Reset Request",
                Body = $@"
        <div style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e2e2; border-radius: 8px; background-color: #f9f9f9;'>
            <h2 style='color: #333; text-align: center;'>Password Reset Request</h2>
            <p style='font-size: 16px;'>Hello,</p>
            <p style='font-size: 16px;'>We received a request to reset your password. If you requested this, please click the button below to reset your password. If you did not make this request, you can safely ignore this email.</p>
            <div style='text-align: center; margin: 20px 0;'>
                <a href='https://localhost:44367/changePassword.aspx?email={email}'
                   style='display: inline-block; padding: 12px 24px; font-size: 16px; color: #ffffff; background-color: #007BFF; text-decoration: none; border-radius: 4px;'>
                   Reset Password
                </a>
            </div>
            <p style='font-size: 14px; color: #666;'>If you’re having trouble clicking the button, copy and paste the URL below into your web browser:</p>
            <p style='font-size: 14px; color: #007BFF;'>{HttpUtility.HtmlEncode("https://localhost:44367/changePassword.aspx?email=" + email)}</p>
            <p style='font-size: 16px;'>Thank you,<br/>The Online Library System Team</p>
        </div>",
                IsBodyHtml = true,
            };
            mail.To.Add(email);

            // Send the email
            smtpClient.Send(mail);
        }
    }
}
