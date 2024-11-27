using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class LoanDetail : System.Web.UI.Page
    {
        static int userid = 0;
        public static int loanId = 0;
        public string recommended = "none";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Check if the "LoanId" query string parameter exists
                if (!string.IsNullOrEmpty(Request.QueryString["LoanId"]))
                {
                    // Retrieve the value of the "LoanId" query string parameter
                    loanId = Convert.ToInt32(Request.QueryString["LoanId"]);

                }

                if (Session["PatronId"] != null)
                {
                    userid = Convert.ToInt32(Session["PatronId"].ToString());
                }

                if(userid != 0 && loanId != 0)
                {
                    getLoan();
                }
                
            }
        }


        public void getLoan()
        {
            try
            {
                string query = @"SELECT 
    Loan.LoanId,
    Loan.StartDate,
    Loan.EndDate,
    Loan.IsApprove,
    Loan.PatronId,
    Loan.BookCopyId,
    Loan.Status,
    Loan.LatestReturn,
    Loan.IsCommented,
    Loan.Recommended,
    BookCopy.ISBN,
    BookCopy.BookId,
    BookCopy.BookCopyImage, -- Fields from BookCopy
    BookCopy.PublishDate, 
    BookCopy.PublishOwner, 
    Book.BookTitle,
    Book.BookDesc -- Fields from Book table, add more as needed 
FROM 
    Loan
INNER JOIN 
    BookCopy ON Loan.BookCopyId = BookCopy.BookCopyId
INNER JOIN 
    Book ON BookCopy.BookId = Book.BookId
WHERE 
    Loan.PatronId = @userId AND Loan.LoanId = @loanId";
                DataTable originalDt = DBHelper.ExecuteQuery(query, new string[]{
                    "userId", userid.ToString(),
                    "loanId",loanId.ToString()
                });

                if (originalDt.Rows.Count > 0)
                {
                    DataRow row = originalDt.Rows[0];

                    // Assign values using GetSafeValue
                    lblBookTitle.Text = GetSafeValue(row, "BookTitle");
                    lblISBN.Text = GetSafeValue(row, "ISBN");
                    lblStartDate.Text = !string.IsNullOrEmpty(GetSafeValue(row, "StartDate"))
                        ? DateTime.Parse(GetSafeValue(row, "StartDate")).ToString("dd MMM yyyy")
                        : "N/A";
                    lblEndDate.Text = !string.IsNullOrEmpty(GetSafeValue(row, "EndDate"))
                        ? DateTime.Parse(GetSafeValue(row, "EndDate")).ToString("dd MMM yyyy")
                        : "N/A";
                    lblPubDate.Text = !string.IsNullOrEmpty(GetSafeValue(row, "PublishDate"))
                        ? DateTime.Parse(GetSafeValue(row, "PublishDate")).ToString("dd MMM yyyy")
                        : "N/A";
                    lblOwner.Text = GetSafeValue(row, "PublishOwner");

                    // Handle status
                    string status = GetSafeValue(row, "Status").ToLower();
                    switch (status)
                    {
                        case "loaning":
                            lblStatus.Text = "Loaning";
                            break;
                        case "latereturn":
                            lblStatus.Text = "Late Return";
                            break;
                        case "returned":
                            lblStatus.Text = "Returned";
                            break;
                        default:
                            lblStatus.Text = "Unknown";
                            break;
                    }

                    // Handle LatestReturn
                    lblLatestReturn.Text = !string.IsNullOrEmpty(GetSafeValue(row, "LatestReturn"))
                        ? DateTime.Parse(GetSafeValue(row, "LatestReturn")).ToString("dd MMM yyyy")
                        : "N/A";

                    // Handle Days Left to Return
                    if (!status.Equals("returned", StringComparison.OrdinalIgnoreCase))
                    {
                        DateTime startDate = DateTime.Parse(GetSafeValue(row, "StartDate"));
                        DateTime endDate = DateTime.Parse(GetSafeValue(row, "EndDate"));
                        int daysDifference = (endDate - startDate).Days;

                        if (daysDifference >= 0)
                        {
                            lblDaysLeftToReturn.Text = $"{Math.Abs(daysDifference)} days left to return";
                            pnlDaysLeftToReturn.Visible = true;
                        }
                        else
                        {
                            pnlDaysLeftToReturn.Visible = false;
                        }
                    }
                    else
                    {
                        pnlDaysLeftToReturn.Visible = false;
                    }

                    string recommendedValue = GetSafeValue(row, "Recommended");
                    if (string.IsNullOrEmpty(recommendedValue))
                    {
                        recommended = "none"; // None of the buttons are active
                    }
                    else if (recommendedValue == "True")
                    {
                        recommended = "up"; // Up button active
                    }
                    else if (recommendedValue == "False")
                    {
                        recommended = "down"; // Down button active
                    }

                    string getCommented = (GetSafeValue(row, "IsCommented"));
                    if(status == "returned" && getCommented == "False")
                    {
                       
                            pnlComment.Visible = true; // Down button active
                        
                    }
                    else
                    {
                        pnlComment.Visible = false;
                    }
                    

                    imgBook.ImageUrl = ImageHandler.GetImage((byte[])row["BookCopyImage"]);

                    

                }


            }
            catch (Exception ex)
            {
                
            }
        }

        [System.Web.Services.WebMethod(Description = "Book Recommended")]
        public static void bookRecommended()
        {
            try
            {
                string query = @"UPDATE Loan
SET Recommended = 1
WHERE LoanId = @loanId
AND PatronId = @userId;";
                DataTable originalDt = DBHelper.ExecuteQuery(query, new string[]{
                    "userId", userid.ToString(),
                    "loanId",loanId.ToString()
                });
            }catch(Exception ex)
            {

            }
            
        }

        [System.Web.Services.WebMethod(Description = "Book Not Recommended")]
        public static void bookNotRecommended()
        {
            try
            {
                string query = @"UPDATE Loan
SET Recommended = 0
WHERE LoanId = @loanId
AND PatronId = @userId;";
                DataTable originalDt = DBHelper.ExecuteQuery(query, new string[]{
                    "userId", userid.ToString(),
                    "loanId",loanId.ToString()
                });
            }
            catch (Exception ex)
            {

            }

        }

        public string GetBookId()
        {
            int bookId = 0;

            // Retrieve the BookId from the database when the loan details are loaded
            if (loanId != 0)
            {
                // Assuming you have a method to fetch the BookId from the database using the loanId
                string query = "SELECT BookCopy.BookId FROM Loan INNER JOIN BookCopy ON Loan.BookCopyId = BookCopy.BookCopyId WHERE Loan.LoanId = @loanId AND Loan.PatronId = @userId";
                bookId = Convert.ToInt32(DBHelper.ExecuteScalar(query, new string[] { 
                    "loanId", loanId.ToString(), 
                    "userId", userid.ToString() 
                }));

                if (bookId != 0)
                {
                    return bookId.ToString();
                }
            }
            return null;

        }

        private string GetSafeValue(DataRow row, string columnName)
        {
            return row[columnName] != DBNull.Value ? row[columnName].ToString() : string.Empty;
        }
    }
}