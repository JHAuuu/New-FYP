﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Web.Security;
using System.Security.Claims;

namespace fyp
{
    public partial class DashMasterPage : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Get the current page's name
            string currentPage = System.IO.Path.GetFileName(Request.Url.AbsolutePath);

            // Remove the "active" class from all links initially
            dashUrl.Attributes["class"] = "";
            userUrl.Attributes["class"] = "";
            staffUrl.Attributes["class"] = "";
            bookUrl.Attributes["class"] = "";
            announcementUrl.Attributes["class"] = "";
            userTrustUrl.Attributes["class"] = "";

            // Add the "active" class to the link that matches the current page
            switch (currentPage)
            {
                case "DashManagement.aspx":
                    dashUrl.Attributes["class"] = "active";
                    titleLogo.Attributes["class"] = "las la-tachometer-alt";
                    break;
                case "UsersManagement.aspx":
                    userUrl.Attributes["class"] = "active";
                    titleLogo.Attributes["class"] = "las la-users";
                    break;
                case "UserHistory.aspx":
                    userUrl.Attributes["class"] = "active";
                    titleLogo.Attributes["class"] = "las la-history";
                    break;
                case "StaffManagement.aspx":
                    staffUrl.Attributes["class"] = "active";
                    titleLogo.Attributes["class"] = "las la-user-tie";
                    break;
                case "BookManagement.aspx":
                    bookUrl.Attributes["class"] = "active";
                    titleLogo.Attributes["class"] = "las la-atlas";
                    break;
                case "BookCopyManagement.aspx":
                    bookUrl.Attributes["class"] = "active";
                    titleLogo.Attributes["class"] = "las la-atlas";
                    break;
                case "AnnouncementManagement.aspx":
                    announcementUrl.Attributes["class"] = "active";
                    titleLogo.Attributes["class"] = "las la-bullhorn";
                    break;
                case "UserTrustManagement.aspx":
                    userTrustUrl.Attributes["class"] = "active";
                    titleLogo.Attributes["class"] = "las la-handshake";
                    break;

            }

            // Use HttpContext.Current.User to get claims in a master page
            var identity = HttpContext.Current.User as ClaimsPrincipal;

            if (identity != null && identity.Identity.IsAuthenticated)
            {
                // Retrieve the username from the claims
                string userName = identity.FindFirst(ClaimTypes.Name)?.Value;

                // Store the username in the session
                Session["UserName"] = userName;

                string queryFindUser = "SELECT * FROM [User] WHERE username = @username";
                string[] arrFindUser = new string[2];
                arrFindUser[0] = "@userName";
                arrFindUser[1] = Session["UserName"].ToString();
                DataTable dt = fyp.DBHelper.ExecuteQuery(queryFindUser, arrFindUser);

                lblName.Text = dt.Rows[0]["UserName"].ToString();
                lblRole.Text = dt.Rows[0]["UserRole"].ToString();

                if (!IsPostBack)
                {
                    // Check if the user role is Admin
                    if (Session["UserRole"] != null && Session["UserRole"].ToString() == "Admin")
                    {
                        staffMenuItem.Visible = true; // Show the menu item
                    }
                    else
                    {
                        staffMenuItem.Visible = false; // Hide the menu item
                    }
                }
            }
            else
            {
                // Redirect to Login if the user is not authenticated
                //Response.Redirect("Login.aspx");
            }
        }

        public string titleText
        {
            get { return lblTitle.Text; }
            set { lblTitle.Text = value; }
        }
    }
}