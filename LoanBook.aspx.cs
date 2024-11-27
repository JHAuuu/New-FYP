using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class LoanBook : System.Web.UI.Page
    {

        static int userid = 0;
        public static int bookid = 0;
        public int currentYear;
        public int currentMonth;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                currentYear = DateTime.Now.Year;
                currentMonth = DateTime.Now.Month;
               
                    
                    if (Session["PatronId"] != null)
                    {
                        userid = Convert.ToInt32(Session["PatronId"].ToString());
                    }



                
            }
        }

        public DataTable getAllBookCopy()
        {
            try
            {

                string query = @"SELECT BookCopyId
      ,PublishDate
      ,PublishOwner
      ,IsAvailable
      ,BookCopyImage
      ,BookId
  FROM BookCopy
WHERE BookId = @bookId
AND IsAvailable = 1
";

                DataTable originalDt = DBHelper.ExecuteQuery(query, new string[]{
                    "bookId", bookid.ToString()
                });

                DataTable dt = new DataTable();
                dt.Columns.Add("BookCopyId", typeof(string));
                dt.Columns.Add("PublishDate", typeof(string));
                dt.Columns.Add("PublishOwner", typeof(string));
                dt.Columns.Add("IsAvailable", typeof(string));
                dt.Columns.Add("BookCopyImage", typeof(string));
                dt.Columns.Add("BookId", typeof(string));

                foreach (DataRow originalRow in originalDt.Rows)
                {
                    DataRow newRow = dt.NewRow();
                    foreach (DataColumn column in originalDt.Columns)
                    {
                        if (column.ColumnName == "BookCopyImage" && originalRow[column] != DBNull.Value)
                        {

                            newRow[column.ColumnName] = ImageHandler.GetImage((byte[])originalRow[column]);
                        }
                        else if (column.ColumnName != "BookCopyImage" && originalRow[column] != DBNull.Value)
                        {
                            newRow[column.ColumnName] = originalRow[column].ToString();
                        }
                        else
                        {
                            newRow[column.ColumnName] = DBNull.Value;
                        }

                    }
                    dt.Rows.Add(newRow);
                }


                return dt;

            }
            catch (Exception ex)
            {
                return new DataTable();
            }


        }
        public string GetLoanedDatesForBook(object bookId)
        {
            int bookIdInt = Convert.ToInt32(bookId);
            // Example logic to get loaned dates for this book
            List<int> loanedDates = GetLoanedDatesForBookId(bookIdInt); // Fetch the dates from the DB
            return string.Join(",", loanedDates);
        }


        public List<int> GetLoanedDatesForBookId(int bookId)
        {
            List<int> loanedDates = new List<int>();

            // Query to retrieve loans for the specific BookCopyId with status 'loaning' or 'preloaning'
            string query = @"
        SELECT StartDate, EndDate
        FROM Loan
        WHERE BookCopyId = @bookId
        AND (Status = 'loaning' OR Status = 'preloaning')
    ";

            // Execute the query and get results
            DataTable loanDt = DBHelper.ExecuteQuery(query, new string[] { "bookId", bookId.ToString() });

            // Iterate over each loan record
            foreach (DataRow row in loanDt.Rows)
            {
                DateTime startDate = Convert.ToDateTime(row["StartDate"]);
                DateTime endDate = Convert.ToDateTime(row["EndDate"]);

                // Iterate from StartDate to EndDate, adding each day of the month to loanedDates
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    if (date.Month == currentMonth || date.Month == currentMonth + 1 || date.Month == currentMonth - 1)
                    {
                        loanedDates.Add(date.Day);
                    }
                }
            }

            // Return the list of loaned dates as integers
            return loanedDates.Distinct().ToList();  // Use Distinct() to remove duplicate days if needed
        }

        [System.Web.Services.WebMethod(Description = "Check Trustworthy")]
        public static string checkTrustLevel()
        {
            try
            {
                string trustQuery = @"SELECT TrustScore
  FROM Trustworthy
  WHERE PatronId = @userId
";

                string checkLoanQuery = @"SELECT COUNT(LoanId)
  FROM Loan
  WHERE PatronId = @userId AND Status = 'loaning'

";
                object getTrustScore = DBHelper.ExecuteScalar(trustQuery, new string[]
                {
                    "userId", userid.ToString()
                });

                object getLoanNum = DBHelper.ExecuteScalar(checkLoanQuery, new string[]
                {
                    "userId", userid.ToString()
                });

                int userScore = Convert.ToInt32(getTrustScore);

                int userLoanNum = Convert.ToInt32(getLoanNum);

                if (userScore >= 80 && userScore < 90)
                {
                    if (userLoanNum < 1)
                    {
                        return "SUCCESS";
                    }
                    else
                    {
                        return "You cannot borrow more than 1 books on " + userScore.ToString() + " trust credit";
                    }

                }
                else if (userScore >= 90 && userScore < 100)
                {
                    if (userLoanNum < 2)
                    {
                        return "SUCCESS";
                    }
                    else
                    {
                        return "You cannot borrow more than 2 books on " + userScore.ToString() + " trust credit";
                    }
                }
                else if (userScore == 100)
                {
                    if (userLoanNum < 3)
                    {
                        return "SUCCESS";
                    }
                    else
                    {
                        return "You cannot borrow more than 3 books on " + userScore.ToString() + " trust credit";
                    }
                }
                else
                {
                    return userScore.ToString() + " is not in the range of trust credit";
                }
            }
            catch (Exception ex)
            {
                return "Error retrieving trust data";
            }
        }

        [System.Web.Services.WebMethod(Description = "Check Trustworthy")]
        public static string InsertLoan(string EndDate, string ISBN)
        {
            try
            {

                string isbnQuery = @"
            SELECT BookCopyId
  FROM BookCopy
WHERE ISBN = @isbn;";
                int BookCopyId = Convert.ToInt32(DBHelper.ExecuteScalar(isbnQuery, new string[]{
                    "isbn", ISBN
                }));

                if(BookCopyId > 0)
                {
                    string avoidString = @"
SELECT COUNT(l.LoanId)
  FROM Loan l
  JOIN BookCopy bc on l.BookCopyId = bc.BookCopyId
WHERE bc.ISBN = @isbn
AND l.PatronId = @userid
AND Status = 'loaning';
";

                    int avoidRepeatBook = Convert.ToInt32(DBHelper.ExecuteScalar(avoidString, new string[]{
                    "isbn", ISBN,
                    "userid", userid.ToString()
                }));


                    // Parse the date in 'YYYY-MM-DD' format (default for input[type="date"])
                    DateTime endDate = DateTime.Parse(EndDate);

                    /*        if (endDate < startDate)
                            {
                                return "End date cannot be earlier than start date.";
                            }*/


                    if(avoidRepeatBook > 0)
                    {
                        return "You already have the book";
                    }
                    else
                    {
                        string query = @"
            INSERT INTO Loan (StartDate, EndDate, IsApprove, PatronId, BookCopyId, Status)
            VALUES
            (CAST(GETDATE() AS DATE), CAST(@endDate AS DATE), 1, @userId, @bookCopyId, 'loaning')";

                        int insertLoan = DBHelper.ExecuteNonQuery(query, new string[]
                        {
                "endDate", endDate.ToString("yyyy-MM-dd"),
                "userId", userid.ToString(),
                "bookCopyId", BookCopyId.ToString()
                        });

                        if (insertLoan > 0)
                        {
                            return "SUCCESS";
                        }
                        else
                        {
                            return "Failed to loan the book";
                        }
                    }

                   
                }
                else
                {
                    return "This book source is not in this library";

                }


              


            }
            catch (FormatException)
            {
                return "Invalid date format. Please check your input.";
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }
    }

}