﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class BookDetail : System.Web.UI.Page
    {
        static int userid = 0;
        public static int bookid = 0;

        public bool isFavorite = false;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!String.IsNullOrEmpty(Request.QueryString["bookid"]))
                {
                    bookid = Convert.ToInt32(Request.QueryString["bookid"]);
                    if (Session["PatronId"] != null)
                    {
                        userid = Convert.ToInt32(Session["PatronId"].ToString());

                        storeHistory();
                    }

                    GetBookDetail();

                    CurrentUserPatronId = userid.ToString();

                    DataTable cmtDt = getComment();
                    if (cmtDt.Rows.Count > 0)
                    {
                        rptComment.DataSource = cmtDt;
                        rptComment.DataBind();
                    }

                    DataTable relatedBookDt = getRelatedBooks();
                    if (relatedBookDt.Rows.Count > 0)
                    {
                        rptRelatedBook.DataSource = relatedBookDt;
                        rptRelatedBook.DataBind();
                    }

                    isFavorite = getFavBook();

                    if (getAllFavGroup().Rows.Count > 0)
                    {
                        rptGroup.DataSource = getAllFavGroup();
                        rptGroup.DataBind();
                    }
                }
            }

        }

        public void GetBookDetail()
        {
            try
            {
                string query = @"SELECT 
    b.BookId, 
    b.BookTitle, 
    b.BookDesc, 
    b.BookSeries, 
    b.BookImage, 
    COUNT(CASE WHEN 
                     (l.LoanId IS NULL OR l.Status = 'returned' OR l.StartDate > GETDATE() OR l.EndDate < GETDATE() ) THEN 1 END) AS AvailableCopies,
    COUNT(bc.BookCopyId) AS TotalCopies,
    -- Nested subquery for Authors
    (SELECT STRING_AGG(a.AuthorName, ', ') 
     FROM BookAuthor ba 
     JOIN Author a ON ba.AuthorId = a.AuthorId 
     WHERE ba.BookId = b.BookId) AS Authors,
    -- Nested subquery for Categories
    (SELECT STRING_AGG(c.CategoryName, ', ') 
     FROM BookCategory bcg 
     JOIN Category c ON bcg.CategoryId = c.CategoryId 
     WHERE bcg.BookId = b.BookId) AS Categories
FROM 
    Book AS b
JOIN 
    BookCopy AS bc ON b.BookId = bc.BookId
LEFT JOIN 
    Loan l ON bc.BookCopyId = l.BookCopyId AND (l.Status = 'loaning') 
WHERE 
    b.BookId = @bookId
GROUP BY 
    b.BookId, b.BookTitle, b.BookDesc, b.BookSeries, b.BookImage;
";
                DataTable originalDt = DBHelper.ExecuteQuery(query, new string[]{
                    "bookId", bookid.ToString()
                });


                if (originalDt.Rows.Count > 0)
                {
                    DataRow row = originalDt.Rows[0];

                    lblTitle.Text = row["BookTitle"].ToString();
                    lblDesc.Text = row["BookDesc"].ToString();

                    lblSeries.Text = row["BookSeries"].ToString();
                    pnlSeries.Visible = !string.IsNullOrEmpty(lblSeries.Text);
                    

                    lblAuthor.Text = row["Authors"].ToString();
                    lblCategory.Text = row["Categories"].ToString();
                    if (row["BookImage"] != DBNull.Value)
                    {
                        
                        imgBook.ImageUrl = ImageHandler.GetImage((byte[])row["BookImage"]);  // Directly set Base64 string as ImageUrl
                    }
                    else
                    {
                        imgBook.ImageUrl = DBNull.Value.ToString();
                    }
                }

            }
            catch (Exception ex)
            {
               // return new DataTable();
            }
        }


        public DataTable getComment()
        {
            try
            {
                string query = @"SELECT 
    r.PatronId, 
    r.BookId, 
    r.RateComment, 
    r.RateStarts, 
    r.RateDate,
    u.UserName
FROM 
    Rating r
JOIN 
    Patron p ON r.PatronId = p.PatronId
JOIN 
    [User] u ON p.UserId = u.UserId
WHERE 
    r.BookId = @bookId
ORDER BY 
    CASE 
        WHEN r.PatronId = @userId THEN 1 
        ELSE 2 
    END, 
    r.RateDate DESC;
";

                DataTable originalDt = DBHelper.ExecuteQuery(query, new string[]{
                    "bookId", bookid.ToString(),
                    "userId", userid.ToString()
                }) ;


                return originalDt;

            }
            catch (Exception ex)
            {
                 return new DataTable();
            }
        }


        public DataTable getRelatedBooks()
        {
            try
            {
                string query = @"SELECT TOP 3 b.BookId, b.BookTitle, b.BookImage
FROM Book b
LEFT JOIN BookCategory bc ON b.BookId = bc.BookId
LEFT JOIN BookAuthor ba ON b.BookId = ba.BookId
WHERE 
    (bc.CategoryId IN (SELECT CategoryId FROM BookCategory WHERE BookId = @bookId)
    OR ba.AuthorId IN (SELECT AuthorId FROM BookAuthor WHERE BookId = @bookId))
    AND b.BookId != @bookId
GROUP BY b.BookId, b.BookTitle, b.BookDesc, b.BookSeries, b.BookImage
ORDER BY NEWID(); 
";

                DataTable originalDt = DBHelper.ExecuteQuery(query, new string[]{
                    "bookId", bookid.ToString(),
                });


                return originalDt;

            }
            catch (Exception ex)
            {
                return new DataTable();
            }
        }


        public bool getFavBook()
        {
            try
            {
                string query = @"SELECT FavId
      ,BookId
      ,PatronId
      ,Date
  FROM Favourite
  WHERE PatronId = @userId AND BookId = @bookId;";

                DataTable originalDt = DBHelper.ExecuteQuery(query, new string[]{
                    "userId", userid.ToString(),
                    "bookId", bookid.ToString()
                });

                return originalDt.Rows.Count > 0;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public DataTable getAllFavGroup()
        {
            try
            {
                string query = @"SELECT FavGrpId, FavGrpName FROM FavGroup WHERE PatronId = @userid";
                DataTable originalDt = DBHelper.ExecuteQuery(query, new string[]
                {
                    "userid",userid.ToString()
                });

                return originalDt;
            }
            catch (Exception ex)
            {
                return new DataTable();
            }
        }

        [System.Web.Services.WebMethod(Description = "Remove From Favourite")]
        public static string RemoveFromAll()
        {
            try
            {

                string queryFavId = @"SELECT FavId FROM Favourite WHERE BookId = @bookId AND PatronId = @userId";
                object findFavId = DBHelper.ExecuteScalar(queryFavId, new string[]
                {
                    "bookId", bookid.ToString(),
                    "userId", userid.ToString()
                });
                int favId = Convert.ToInt32(findFavId);

                string queryGrouping = @"DELETE FROM FavouriteGrouping WHERE FavId = @favId";
                int removeGrouping = DBHelper.ExecuteNonQuery(queryGrouping, new string[]
                {
                    "favId", favId.ToString()
                });


                string queryFav = @"DELETE FROM Favourite WHERE FavId = @favId";
                int removeFav = DBHelper.ExecuteNonQuery(queryFav, new string[]
                {
                    "favId", favId.ToString()
                });

                if (removeFav > 0)
                {
                    return "SUCCESS";
                }
                else
                {
                    return "Cannot Remove Favourite";
                }



            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [System.Web.Services.WebMethod(Description = "Add to Favourite only")]
        public static string AddToDefaultGroup()
        {
            try
            {
                // Replace with your database logic to add the favorite item to the default group
                // Example (pseudo-code):
                // Database.AddToGroup(favId, "defaultGroupId");

                string query = @"INSERT INTO Favourite (BookId, PatronId, Date) VALUES (@bookId, @userId, CAST(GETDATE() AS DATE));";
                int insertFav = DBHelper.ExecuteNonQuery(query, new string[]
                {
                    "bookId", bookid.ToString(),
                    "userId", userid.ToString()
                });

                if (insertFav > 0)
                {
                    return "SUCCESS";
                }
                else
                {
                    return "Error Adding as favourite";
                }

            }
            catch (Exception ex)
            {
                // Log the exception and return an error message
                return "ERROR: " + ex.ToString();
            }
        }

        [System.Web.Services.WebMethod(Description = "Add to Groups")]
        public static string AddToGroups( List<string> groupIds)
        {
            try
            {
                string query = @"INSERT INTO Favourite (BookId, PatronId, Date) VALUES (@bookId, @userId, CAST(GETDATE() AS DATE));  SELECT SCOPE_IDENTITY();";
                object insertFav = DBHelper.ExecuteScalar(query, new string[]
                {
                    "bookId", bookid.ToString(),
                    "userId", userid.ToString()
                });
                int FavId = Convert.ToInt32(insertFav);
                int insertGrouping = 0;
                if (FavId != 0)
                {
                    foreach (string groupId in groupIds)
                    {
                        string groupingQuery = @"INSERT INTO FavouriteGrouping (FavId, FavGrpId) VALUES (@favId, @groupId);";
                        insertGrouping = DBHelper.ExecuteNonQuery(groupingQuery, new string[]
                        {
                            "favId", FavId.ToString(),
                            "groupId", groupId
                        });
                    }
                    if (insertGrouping > 0)
                    {
                        return "SUCCESS";
                    }
                    else
                    {
                        return "Error Adding as Grouping";
                    }

                }
                else
                {
                    return "Error Adding as favourite";
                }
                // Assuming you have a database connection and logic to insert the book into groups


            }
            catch (Exception ex)
            {
                // Log the error and return a failure message
                return "ERROR: " + ex.Message;
            }
        }


        public void storeHistory()
        {
            try
            {
                string findSameBook = @"
SELECT COUNT(*)
  FROM History
  WHERE PatronId = @userId AND BookId = @bookId;
";

                int findBook = Convert.ToInt32(DBHelper.ExecuteScalar(findSameBook, new string[]
                {
                    "userId",userid.ToString(),
                    "bookId",bookid.ToString()
                }));

                if(findBook > 0)
                {
                    string updateQuery = @"UPDATE History
SET HistoryDate = GETDATE()
 WHERE PatronId = @userId AND BookId = @bookId;";
                    int update = DBHelper.ExecuteNonQuery(updateQuery, new string[]
                {
                    "userId",userid.ToString(),
                    "bookId",bookid.ToString()
                });
                }
                else
                {
                    string insertQuery = @"INSERT INTO History (HistoryDate, PatronId, BookId)
VALUES
(GETDATE(), @userId, @bookId)";
                    int insert = DBHelper.ExecuteNonQuery(insertQuery, new string[]
                {
                    "userId",userid.ToString(),
                    "bookId",bookid.ToString()
                });



                }
            }

                
            catch (Exception ex)
            {

            }
        }


        public string CurrentUserPatronId { get; set; }
    }
}