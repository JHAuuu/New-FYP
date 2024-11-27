using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace fyp
{
    public partial class AdvancedSearch : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadBooks(); // Default: Load all books
            }
        }


        protected void btnSearch_Click(object sender, EventArgs e)
        {
            List<SearchCriteria> searchCriteriaList = new List<SearchCriteria>();

            string searchField = Request.Form["searchField"];
            string contains = Request.Form["contains"];
            string searchTerm = Request.Form["searchTerm"];

            if (!string.IsNullOrEmpty(searchField))
            {
                searchCriteriaList.Add(new SearchCriteria
                {
                    Field = searchField,
                    Contains = contains,
                    Term = searchTerm
                });
            }

            string timePeriod = Request.Form["timePeriod"];
            DateTime? fromDate = !string.IsNullOrEmpty(Request.Form["fromDate"]) ? DateTime.Parse(Request.Form["fromDate"]) : (DateTime?)null;
            DateTime? toDate = !string.IsNullOrEmpty(Request.Form["toDate"]) ? DateTime.Parse(Request.Form["toDate"]) : (DateTime?)null;

            LoadBooks(searchCriteriaList, timePeriod, fromDate, toDate);
        }



        private void LoadBooks(List<SearchCriteria> searchCriteriaList = null, string timePeriod = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            string query = @"
                WITH DistinctCategories AS (
                    SELECT 
                        b.BookId,
                        COALESCE(c.CategoryName, '') AS CategoryName
                    FROM Book b
                    LEFT JOIN BookCategory bc ON b.BookId = bc.BookId
                    LEFT JOIN Category c ON bc.CategoryId = c.CategoryId
                ),
                DistinctAuthors AS (
                    SELECT 
                        b.BookId,
                        COALESCE(a.AuthorName, '') AS AuthorName
                    FROM Book b
                    LEFT JOIN BookAuthor ba ON b.BookId = ba.BookId
                    LEFT JOIN Author a ON ba.AuthorId = a.AuthorId
                )
                SELECT 
                    b.BookId, 
                    b.BookTitle, 
                    b.BookSeries, 
                    b.CreatedAt, 
                    b.BookImage,
                    (SELECT STRING_AGG(CategoryName, ', ') FROM DistinctCategories dc WHERE dc.BookId = b.BookId) AS CategoryNames,
                    (SELECT STRING_AGG(AuthorName, ', ') FROM DistinctAuthors da WHERE da.BookId = b.BookId) AS AuthorNames
                FROM Book b
                WHERE b.IsDeleted = 0";

            // Apply filters
            if (searchCriteriaList != null && searchCriteriaList.Count > 0)
            {
                foreach (var criteria in searchCriteriaList)
                {
                    string parameterName = "@param" + parameters.Count;
                    switch (criteria.Field)
                    {
                        case "allFields":
                            query += $" AND (b.BookTitle LIKE {parameterName} OR EXISTS (SELECT 1 FROM DistinctCategories dc WHERE dc.CategoryName LIKE {parameterName} AND dc.BookId = b.BookId) OR EXISTS (SELECT 1 FROM DistinctAuthors da WHERE da.AuthorName LIKE {parameterName} AND da.BookId = b.BookId))";
                            parameters.Add(new SqlParameter(parameterName, $"%{criteria.Term}%"));
                            break;
                        case "title":
                            query += $" AND b.BookTitle LIKE {parameterName}";
                            parameters.Add(new SqlParameter(parameterName, $"%{criteria.Term}%"));
                            break;
                        case "author":
                            query += $" AND EXISTS (SELECT 1 FROM DistinctAuthors da WHERE da.AuthorName LIKE {parameterName} AND da.BookId = b.BookId)";
                            parameters.Add(new SqlParameter(parameterName, $"%{criteria.Term}%"));
                            break;
                        case "category":
                            query += $" AND EXISTS (SELECT 1 FROM DistinctCategories dc WHERE dc.CategoryName LIKE {parameterName} AND dc.BookId = b.BookId)";
                            parameters.Add(new SqlParameter(parameterName, $"%{criteria.Term}%"));
                            break;
                        default:
                            break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(timePeriod))
            {
                if (timePeriod == "lastYear")
                {
                    //query += " AND b.CreatedAt >= DATEADD(YEAR, -1, GETDATE())";

                    //in this if 2024 will deduct to 2023 then search and focus on 2023 1 1 to 2023 12 31
                    int currentYear = DateTime.Now.Year;  // Get the current year
                    int lastYear = currentYear - 1;       // Deduct one year for last year

                    // Construct the date range for last year dynamically
                    string startOfLastYear = new DateTime(lastYear, 1, 1).ToString("yyyy-MM-dd");
                    string endOfLastYear = new DateTime(lastYear, 12, 31).ToString("yyyy-MM-dd");

                    // Add the dynamic range to the query
                    query += $" AND b.CreatedAt >= '{startOfLastYear}' AND b.CreatedAt <= '{endOfLastYear}'";
                }
                else if (timePeriod == "lastMonth")
                {
                    query += " AND b.CreatedAt >= DATEADD(MONTH, -1, GETDATE())";
                }
            }

            if (timePeriod == "allTime")
            {
                if (fromDate.HasValue)
                {
                    query += " AND b.CreatedAt >= @FromDate";
                    parameters.Add(new SqlParameter("@FromDate", fromDate.Value));
                }

                if (toDate.HasValue)
                {
                    query += " AND b.CreatedAt <= @ToDate";
                    parameters.Add(new SqlParameter("@ToDate", toDate.Value));
                }
            }

            query += " GROUP BY b.BookId, b.BookTitle, b.BookSeries, b.CreatedAt, b.BookImage";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        rptBooks.DataSource = dt;
                        rptBooks.DataBind();

                        pnlBooks.Visible = dt.Rows.Count > 0;
                        emptyMessage.Visible = dt.Rows.Count == 0;
                    }
                }
            }
        }

        public class SearchCriteria
        {
            public string Field { get; set; }
            public string Contains { get; set; }
            public string Term { get; set; }
        }
    }
}