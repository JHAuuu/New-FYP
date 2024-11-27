using System;
using fyp.Authentication;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Web.Security;

namespace fyp
{
    public partial class DashManagement : AdminPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            var master = this.Master as DashMasterPage;
            if (master != null)
            {
                master.titleText = "Dashboard";
            }

            // SQL query to count the number of users in the User table
            string countUQuery = "SELECT COUNT(*) AS UserCount FROM [User]";
            DataTable countUTable = fyp.DBHelper.ExecuteQuery(countUQuery);

            // Initialize variable to store the user count
            int userCount = 0;

            userCount = Convert.ToInt32(countUTable.Rows[0]["UserCount"]);

            // Assign the user count to the lblUserNum label for display
            lblUserNum.Text = userCount.ToString();

            // SQL query to count the number of users with UserRole = 'Staff' in the User table
            string countSQuery = "SELECT COUNT(*) AS StaffCount FROM [User] WHERE UserRole = @UserRole";
            object[] countSParams = { "@UserRole", "Staff" }; // Filter parameter

            // Execute the query with the parameter
            DataTable countSTable = fyp.DBHelper.ExecuteQuery(countSQuery, countSParams);

            // Initialize variable to store the user count
            int staffCount = 0;

            staffCount = Convert.ToInt32(countSTable.Rows[0]["StaffCount"]);

            // Assign the user count to the lblUserNum label for display
            lblStaffNum.Text = staffCount.ToString();


        }

        protected void btnViewBorrow_Click(object sender, EventArgs e)
        {
            DateTime startDate;
            DateTime endDate;

            // Validate that start date and end date are not empty
            if (string.IsNullOrWhiteSpace(txtStartDate.Text) || string.IsNullOrWhiteSpace(txtEndDate.Text))
            {
                // Alert the user if the fields are empty
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ValidationAlert", "alert('Please enter both start and end dates.');", true);
                return; // Exit the method if validation fails
            }
            try
            {
                // Try parsing the input values into DateTime objects
                startDate = DateTime.Parse(txtStartDate.Text);
                endDate = DateTime.Parse(txtEndDate.Text);

                // Assuming 'reportData' is a collection of loan report data
                var reportData = GetLoanReports(startDate, endDate); // This should be your method to fetch the report data

                // Generate the PDF report (you can replace this with your method if needed)
                byte[] reportBytes = PdfUtility.GenerateLoanPdfReport("Loan Report", reportData, startDate, endDate);

                // Send the PDF to the browser to display or download
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "inline; filename=LoanReport.pdf"); // 'inline' will display in the browser
                Response.BinaryWrite(reportBytes);
                Response.End();

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                // Handle other exceptions
                ScriptManager.RegisterStartupScript(this, this.GetType(), "GenericError", "alert('An error occurred while generating the report. Please try again later.');", true);
            }
        }

        protected void btnGenerateReport_Click(object sender, EventArgs e)
        {
            DateTime startDate;
            DateTime endDate;

            // Validate that start date and end date are not empty
            if (string.IsNullOrWhiteSpace(txtStartDate.Text) || string.IsNullOrWhiteSpace(txtEndDate.Text))
            {
                // Alert the user if the fields are empty
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ValidationAlert", "alert('Please enter both start and end dates.');", true);
                return; // Exit the method if validation fails
            }
            try
            {
                startDate = DateTime.Parse(txtStartDate.Text);
                endDate = DateTime.Parse(txtEndDate.Text);

                // Retrieve the loan report data
                var loanReports = GetLoanReports(startDate, endDate);

                // Generate the PDF report using the retrieved data
                byte[] pdfBytes = PdfUtility.GenerateLoanPdfReport("Loan Report", loanReports, startDate, endDate);

                // Send the PDF to the browser for download
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "attachment; filename=LoanReport.pdf");
                Response.BinaryWrite(pdfBytes);
                Response.End();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                // Handle other exceptions
                ScriptManager.RegisterStartupScript(this, this.GetType(), "GenericError", "alert('An error occurred while generating the report. Please try again later.');", true);
            }
        }

        private List<LoanReport> GetLoanReports(DateTime startDate, DateTime endDate)
        {
            List<LoanReport> loanReports = new List<LoanReport>();

            string query = @"
                        SELECT 
                Loan.LoanId,
                Loan.StartDate,
                Loan.EndDate,
                BookCopy.BookCopyId,
                Book.BookTitle,
                STRING_AGG(Category.CategoryName, ', ') AS CategoryNames, -- Aggregate categories
                [User].UserId,
                [User].UserName,
                Punishment.LatestReturn
            FROM 
                Loan
            INNER JOIN 
                BookCopy ON Loan.BookCopyId = BookCopy.BookCopyId
            INNER JOIN 
                Book ON BookCopy.BookId = Book.BookId
            LEFT JOIN 
                BookCategory ON Book.BookId = BookCategory.BookId
            LEFT JOIN 
                Category ON BookCategory.CategoryId = Category.CategoryId
            INNER JOIN 
                Patron ON Loan.PatronId = Patron.PatronId
            INNER JOIN 
                [User] ON Patron.UserId = [User].UserId
            LEFT JOIN 
                Punishment ON Loan.LoanId = Punishment.LoanId
            WHERE 
                Loan.StartDate >= @StartDate AND Loan.StartDate <= @EndDate
            GROUP BY 
                Loan.LoanId, Loan.StartDate, Loan.EndDate, BookCopy.BookCopyId, Book.BookTitle, 
                [User].UserId, [User].UserName, Punishment.LatestReturn
            ORDER BY 
                Loan.StartDate;";


            object[] arrFindBorrowBook = new object[4];
            arrFindBorrowBook[0] = "@StartDate";
            arrFindBorrowBook[1] = startDate;
            arrFindBorrowBook[2] = "@EndDate";
            arrFindBorrowBook[3] = endDate;
            DataTable dt = fyp.DBHelper.ExecuteQuery(query, arrFindBorrowBook);

            // Iterate through each row in the DataTable and populate the loanReports list
            foreach (DataRow row in dt.Rows)
            {
                LoanReport loanReport = new LoanReport
                {
                    LoanId = Convert.ToInt32(row["LoanId"]),
                    StartDate = Convert.ToDateTime(row["StartDate"]),
                    EndDate = Convert.ToDateTime(row["EndDate"]),
                    BookCopyId = Convert.ToInt32(row["BookCopyId"]),
                    BookTitle = row["BookTitle"].ToString(),
                    CategoryNames = row["CategoryNames"] != DBNull.Value ? row["CategoryNames"].ToString() : "N/A",
                    UserId = Convert.ToInt32(row["UserId"]),
                    UserName = row["UserName"].ToString(),
                    LatestReturn = row["LatestReturn"] != DBNull.Value ? Convert.ToDateTime(row["LatestReturn"]) : (DateTime?)null
                };

                loanReports.Add(loanReport);
            }
            
            return loanReports;
        }

        protected void btnViewAddBook_Click(object sender, EventArgs e)
        {
            DateTime startDate;
            DateTime endDate;

            // Validate that start date and end date are not empty
            if (string.IsNullOrWhiteSpace(txtBookStartDate.Text) || string.IsNullOrWhiteSpace(txtBookEndDate.Text))
            {
                // Alert the user if the fields are empty
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ValidationAlert", "alert('Please enter both start and end dates.');", true);
                return; // Exit the method if validation fails
            }
            try
            {
                // Try parsing the input values into DateTime objects
                startDate = DateTime.Parse(txtBookStartDate.Text);
                endDate = DateTime.Parse(txtBookEndDate.Text);

                // Assuming 'reportData' is a collection of loan report data
                var reportData = GetAddBookReports(startDate, endDate); // This should be your method to fetch the report data

                // Generate the PDF report (you can replace this with your method if needed)
                byte[] reportBytes = PdfUtility.GenerateBookPdfReport("ADD Book Report", reportData, startDate, endDate);

                // Send the PDF to the browser to display or download
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "inline; filename=Add_Book_Report.pdf"); // 'inline' will display in the browser
                Response.BinaryWrite(reportBytes);
                Response.End();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                // Handle other exceptions
                ScriptManager.RegisterStartupScript(this, this.GetType(), "GenericError", "alert('An error occurred while generating the report. Please try again later.');", true);
            }
        }

        private List<BookReport> GetAddBookReports(DateTime startDate, DateTime endDate)
        {
            List<BookReport> bookReports = new List<BookReport>();

            string query = @"
                SELECT 
                Book.BookId,
                Book.BookTitle,
                Book.BookDesc,
                Book.BookSeries,
                Book.CreatedAt,  -- Add the CreatedAt column
                STRING_AGG(Category.CategoryName, ', ') AS CategoryNames,  -- Aggregate category names
                Author.AuthorName
            FROM 
                Book
            LEFT JOIN 
                BookCategory ON Book.BookId = BookCategory.BookId
            LEFT JOIN 
                Category ON BookCategory.CategoryId = Category.CategoryId
            LEFT JOIN 
                BookAuthor ON Book.BookId = BookAuthor.BookId
            LEFT JOIN 
                Author ON BookAuthor.AuthorId = Author.AuthorId
            WHERE 
                Book.CreatedAt >= @StartDate AND Book.CreatedAt <= @EndDate  -- Filter by CreatedAt
                AND Book.IsDeleted = 0  -- Assuming you want to exclude deleted books
            GROUP BY 
                Book.BookId, Book.BookTitle, Book.BookDesc, Book.BookSeries, Book.CreatedAt, Author.AuthorName
            ORDER BY 
                Book.BookId;";

            object[] parameters = {
                    "@StartDate", startDate,   // Pass your start date here
                    "@EndDate", endDate       // Pass your end date here
                };

            DataTable dt = fyp.DBHelper.ExecuteQuery(query, parameters);

            // Iterate through each row in the DataTable and populate the loanReports list
            foreach (DataRow row in dt.Rows)
            {
                BookReport bookReport = new BookReport
                {
                    BookId = Convert.ToInt32(row["BookId"]),
                    BookTitle = row["BookTitle"].ToString(),
                    BookDesc = row["BookDesc"].ToString(),
                    BookSeries = row["BookSeries"].ToString(),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                    CategoryNames = row["CategoryNames"] != DBNull.Value ? row["CategoryNames"].ToString() : "N/A",
                    AuthorName = row["AuthorName"] != DBNull.Value ? row["AuthorName"].ToString() : "N/A"
                };

                bookReports.Add(bookReport);
            }

            return bookReports;
        }

        protected void btnGenerateAddBook_Click(object sender, EventArgs e)
        {
            DateTime startDate;
            DateTime endDate;

            // Validate that start date and end date are not empty
            if (string.IsNullOrWhiteSpace(txtBookStartDate.Text) || string.IsNullOrWhiteSpace(txtBookEndDate.Text))
            {
                // Alert the user if the fields are empty
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ValidationAlert", "alert('Please enter both start and end dates.');", true);
                return; // Exit the method if validation fails
            }
            try
            {
                // Try parsing the input values into DateTime objects
                startDate = DateTime.Parse(txtBookStartDate.Text);
                endDate = DateTime.Parse(txtBookEndDate.Text);

                // Assuming 'reportData' is a collection of loan report data
                var reportData = GetAddBookReports(startDate, endDate); // This should be your method to fetch the report data

                // Generate the PDF report (you can replace this with your method if needed)
                byte[] reportBytes = PdfUtility.GenerateBookPdfReport("ADD Book Report", reportData, startDate, endDate);

                // Send the PDF to the browser for download
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "attachment; filename=Add_Book_Report.pdf");
                Response.BinaryWrite(reportBytes);
                Response.End();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                // Handle other exceptions
                ScriptManager.RegisterStartupScript(this, this.GetType(), "GenericError", "alert('An error occurred while generating the report. Please try again later.');", true);
            }
        }
    }

}