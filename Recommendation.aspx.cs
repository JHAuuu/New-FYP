﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class Recommendation : System.Web.UI.Page
    {
        static int userid = 0;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            
            if (!IsPostBack)
            {
                if (Session["PatronId"] != null)
                {

                    userid = Convert.ToInt32(Session["PatronId"].ToString());
                    DataTable BooksDt = getPreferencesList();


                    if (BooksDt.Rows.Count > 0)
                    {
                        rptBooks.DataSource = BooksDt;
                        rptBooks.DataBind();
                    }

                    DataTable categoryDt = getAllCategoryName();
                    DataTable filteredCategoryDt = getSelectPreferencesbyUser(categoryDt);

                    if (filteredCategoryDt.Rows.Count > 0)
                    {
                        rptCategory.DataSource = filteredCategoryDt;
                        rptCategory.DataBind();
                        rptChoosenCat.DataSource = filteredCategoryDt;
                        rptChoosenCat.DataBind();
                    }
                }
                else
                {
                    
                    Response.Redirect("Logout.aspx");
                }
            }
        }

        public DataTable getRecommendationList(int userid){
            var ratings = Rating.LoadRatingsFromDatabase();
            var model = Rating.TrainModel(ratings);
            IEnumerable<uint> allBookIds = Rating.GetAllBookIdsFromDatabase();
            var recommendations = Rating.GetRecommendationsForUser(model, (uint)userid, allBookIds);

            DataTable dt = new DataTable();
            dt.Columns.Add("BookID", typeof(int));
            dt.Columns.Add("BookTitle", typeof(string));
            dt.Columns.Add("BookDesc", typeof(string));
            dt.Columns.Add("BookSeries", typeof(string));
            dt.Columns.Add("CategoryName", typeof(string));
            dt.Columns.Add("BookImage", typeof(string));

            foreach (var recommendation in recommendations)
            {

                string query = @"
            SELECT b.BookTitle, b.BookDesc, b.BookDesc, b.BookSeries, c.CategoryName, b.BookImage
            FROM Book AS b
            INNER JOIN Category AS c ON b.CategoryId = c.CategoryId
            WHERE b.BookId = @bookId";

                DataTable tempDt = DBHelper.ExecuteQuery(query, new string[] {
                    "@bookId", recommendation.bookId.ToString() }
                );

                foreach (DataRow originalRow in tempDt.Rows)
                {
                    DataRow newRow = dt.NewRow();
                    foreach (DataColumn column in tempDt.Columns)
                    {
                        if (column.ColumnName == "BookImage" && originalRow[column] != DBNull.Value)
                        {

                            newRow[column.ColumnName] = ImageHandler.GetImage((byte[])originalRow[column]);
                        }
                        else if (column.ColumnName != "BookImage" && originalRow[column] != DBNull.Value)
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
            }

                return dt;

        }

        public DataTable getPreferencesList()
        {
            string query = @"SELECT 
    b.BookId,
    b.BookTitle,
    b.BookDesc,
    b.BookSeries,
   (SELECT STRING_AGG(c.CategoryName, ', ') 
     FROM BookCategory bcg 
     JOIN Category c ON bcg.CategoryId = c.CategoryId 
     WHERE bcg.BookId = b.BookId) AS CategoryNames,  -- Concatenate all categories for each book
    CAST(SUM(r.RateStarts) * 1.0 / COUNT(r.RateStarts) AS DECIMAL(3,1)) AS AverageRating,
    COUNT(r.RateStarts) AS RatingCount,
    b.BookImage
FROM 
    Book AS b
INNER JOIN 
    BookCategory AS bc ON b.BookId = bc.BookId
INNER JOIN 
    Category AS c ON bc.CategoryId = c.CategoryId  -- Get all categories for each book
INNER JOIN 
    Rating AS r ON r.BookId = b.BookId
WHERE 
    b.BookId IN (
        SELECT DISTINCT b.BookId
        FROM Book b
        INNER JOIN BookCategory bc ON b.BookId = bc.BookId
        INNER JOIN Preferences p ON p.CategoryId = bc.CategoryId
        WHERE p.PatronId = @userid
    )  -- Subquery to filter books based on user preferences
GROUP BY 
    b.BookId, b.BookTitle, b.BookDesc, b.BookSeries, b.BookImage
ORDER BY 
    AverageRating DESC, RatingCount DESC;         
                                    ";


            DataTable originalDt = DBHelper.ExecuteQuery(query, new string[]{
                    "@userid",userid.ToString()
                });

            DataTable dt = new DataTable();
            dt.Columns.Add("BookID", typeof(string));
            dt.Columns.Add("BookTitle", typeof(string));
            dt.Columns.Add("BookDesc", typeof(string));
            dt.Columns.Add("BookSeries", typeof(string));
            dt.Columns.Add("CategoryNames", typeof(string));
            dt.Columns.Add("RatingCount", typeof(string));
            dt.Columns.Add("AverageRating", typeof(string));
            dt.Columns.Add("BookImage", typeof(string));


            foreach (DataRow originalRow in originalDt.Rows)
            {
                DataRow newRow = dt.NewRow();
                foreach (DataColumn column in originalDt.Columns)
                {
                    if (column.ColumnName == "BookImage" && originalRow[column] != DBNull.Value)
                    {

                        newRow[column.ColumnName] = ImageHandler.GetImage((byte[])originalRow[column]);
                    }
                    else if (column.ColumnName != "BookImage" && originalRow[column] != DBNull.Value)
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

        private DataTable GetAuthorsForBook(string bookId)
        {
            string authorQuery = @"SELECT a.AuthorName FROM Author AS a
                           INNER JOIN BookAuthor AS ba ON a.AuthorId = ba.AuthorId
                           WHERE ba.BookId = @bookId";
            DataTable originalDt = DBHelper.ExecuteQuery(authorQuery, new string[] { "@bookId", bookId });
            DataTable dt = new DataTable();
            dt.Columns.Add("AuthorName", typeof(string));
            for (int i = 0; i < originalDt.Rows.Count; i++)
            {
                DataRow row = originalDt.Rows[i];
                // Check for DBNull and add the author name to the new DataTable
                if (row["AuthorName"] != DBNull.Value)
                {
                    DataRow newRow = dt.NewRow();
                    string authorName = row["AuthorName"].ToString();
                    newRow["AuthorName"] = authorName + (i < originalDt.Rows.Count - 1 ? "," : "");
                    dt.Rows.Add(newRow);
                }
            }
            return dt;
        }

        private DataTable getAllCategoryName()
        {
            string query = @"SELECT CategoryId, CategoryName, CategoryDesc FROM Category";
            DataTable originalDt = DBHelper.ExecuteQuery(query);
            DataTable dt = new DataTable();
            dt.Columns.Add("CategoryId", typeof(string));
            dt.Columns.Add("CategoryName", typeof(string));
            dt.Columns.Add("CategoryDesc", typeof(string));

            foreach (DataRow originalRow in originalDt.Rows)
            {
                DataRow newRow = dt.NewRow();
                foreach (DataColumn column in originalDt.Columns)
                {
                    
                        newRow[column.ColumnName] = originalRow[column].ToString();


                }
                dt.Rows.Add(newRow);
            }
            return dt;
        }

        protected void rptBooks_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                // Get the current book's data
                DataRowView rowView = (DataRowView)e.Item.DataItem;
                string bookId = rowView["BookID"].ToString();

                // Find the nested Repeater control for authors
                Repeater rptAuthors = (Repeater)e.Item.FindControl("rptAuthors");

                // Query authors for the current book
                DataTable authorsDataTable = GetAuthorsForBook(bookId); // Fetch authors for this book
                rptAuthors.DataSource = authorsDataTable;
                rptAuthors.DataBind();
            }
        }

        public DataTable getSelectPreferencesbyUser(DataTable categoryDt)
        {
            //add a column for marking data
            if (!categoryDt.Columns.Contains("Active"))
            {
                categoryDt.Columns.Add("Active", typeof(string));
            }

            string query = @"SELECT PatronId, CategoryId FROM Preferences WHERE PatronId = @userid";
            DataTable originalDt = DBHelper.ExecuteQuery(query, new String[]
            {
                "userid",userid.ToString()
            });


            foreach (DataRow categoryRow in categoryDt.Rows)
            {
                // Find if the current category exists in user preferences
                bool isActive = originalDt.AsEnumerable()
                    .Any(originalRow => originalRow["CategoryId"].ToString() == categoryRow["CategoryId"].ToString());

                // Set "Active" column based on the match
                if (isActive)
                {
                    categoryRow["Active"] = "active";
                }
                else
                {
                    categoryRow["Active"] = DBNull.Value;
                }
            }


            return categoryDt;
        }

        [System.Web.Services.WebMethod(Description = "Insert To Database")]
        public static string SaveChanges(List<String> categoryIds)
        {
            try
            {


                //int user = 0;
                if (userid != 0)
                {
                    bool removed = RemoveAllPreferences(userid);


                    if (removed)
                    {
                        if (categoryIds.Count >= 1)
                        {


                            foreach (var item in categoryIds)
                            {
                                string query = "INSERT INTO Preferences (PatronId, CategoryId) VALUES (@userid, @categoryid)";

                                DBHelper.ExecuteNonQuery(query, new string[]{
                            "userid", userid.ToString(),
                            "categoryid",item
                        });


                            }
                        }
                        else
                        {
                            return "At least add one preferences";
                        }
                    }

                }

                return "SUCCESS";
            }catch(Exception ex)
            {
                return "ex";
            }
        }

        public static bool RemoveAllPreferences(int userid)
        {
            
            //int user = 0;
            if(userid != 0)
            {
                //user = Convert.ToInt32(Session["user"].ToString());
                try
                {
                    string query = @"DELETE FROM Preferences WHERE PatronId = @userId";
                    DBHelper.ExecuteNonQuery(query, new String[]
                    {
                    "userId",userid.ToString()
                    });
                }catch(Exception ex)
                {
                    return false;
                }
                
            }

            return true;
        }


        protected void rptBooks_PreRender(object sender, EventArgs e)
        {
            // Check if the repeater has data
            bool hasData = rptBooks.Items.Count > 0;

            // Show or hide the empty message based on the data count
            emptyMessage.Visible = !hasData;
            pnlBooks.Visible = hasData;
        }


        [System.Web.Services.WebMethod(Description = "Reset")]
        public static string reset()
        {
            bool removed = RemoveAllPreferences(userid);
            if (removed)
            {
                return "SUCCESS";
            }
            else
            {
                return "Failed to remove";
            }
        }

    }
}