using System;
using fyp.Authentication;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.IO;
using System.Data.SqlClient;
using ZXing; // Import ZXing.Net
using ZXing.Common;
using System.Drawing; // For image processing
using System.Drawing.Imaging; // For saving the barcode image

namespace fyp
{
    public partial class BookCopyManagement : AdminPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var master = this.Master as DashMasterPage;
            if (master != null)
            {
                master.titleText = "Book Management";
            }

            if (!IsPostBack)
            {
                PredefinedData();
            }
        }

        protected void PredefinedData()
        {
            // Get the BookId from the session
            if (Session["BookId"] != null)
            {
                string BookId = Session["BookId"].ToString();

                // Call the method to bind data (ensure BindBookData is implemented to work with BookId)
                BindBookData(BookId);

                // Set the BookId parameter for the SqlDataSource
                sqlDSBookCopy.SelectParameters["BookId"].DefaultValue = BookId;

                // Retrieve the count of BookCopy
                int copyCount = GetBookCopyCount(BookId);
                lblBookCopyCount.Text = $"Showing {copyCount} featured editions";
            }
            else
            {
                // Handle case where BookId is not provided
                lblMessage.Text = "Book ID is missing.";
            }

            // Retrieve BookData from session and populate UI elements
            if (Session["BookData"] is BookData bookData)
            {
                lblBookTitle.Text = bookData.Title;
                lblAuthorName.Text = bookData.Author;
                lblBookDesc.Text = bookData.Description;
                lblCategory.Text = bookData.Categories;
                lblRatingAverage.Text = new string('⭐', bookData.AverageRating) +
                                       $"{bookData.AverageRating} · {bookData.TotalRating} ratings";
                bookCoverImage.ImageUrl = bookData.CoverImageBase64;
            }
            else
            {
                lblMessage.Text = "No book data found in session.";
            }
        }

        public class BookData
        {
            public string Title { get; set; }
            public string Author { get; set; }
            public string Series { get; set; }
            public string Description { get; set; }
            public string Categories { get; set; }
            public int AverageRating { get; set; }
            public int TotalRating { get; set; }
            public int RatingCount { get; set; }
            public string CoverImageBase64 { get; set; }
        }

        private int GetBookCopyCount(string BookId)
        {
            int count = 0;
            string query = "SELECT COUNT(*) FROM BookCopy WHERE BookId = @BookId AND IsDeleted = 0";

            object[] checkParams = { "@BookId", BookId };
            DataTable rt = fyp.DBHelper.ExecuteQuery(query, checkParams);

            if (rt.Rows.Count > 0)
            {
                // Retrieve the count from the DataTable
                count = Convert.ToInt32(rt.Rows[0][0]);
            }

            return count;
        }

        private void BindBookData(string BookId)
        {
            string query = @"
        SELECT
            Book.BookImage,
            Book.BookTitle,
            Book.BookSeries,
            Author.AuthorName,
            Book.BookDesc,
            STRING_AGG(Category.CategoryName, ', ') AS CategoryNames,
            ISNULL(SUM(Rating.RateStarts), 0) AS TotalRating,
            COUNT(Rating.RateStarts) AS RatingCount
        FROM
            Book
        LEFT JOIN BookAuthor ON Book.BookId = BookAuthor.BookId
        LEFT JOIN Author ON BookAuthor.AuthorId = Author.AuthorId
        LEFT JOIN Rating ON Book.BookId = Rating.BookId
        LEFT JOIN BookCategory ON Book.BookId = BookCategory.BookId
        LEFT JOIN Category ON BookCategory.CategoryId = Category.CategoryId
        WHERE
            Book.BookId = @BookId
        GROUP BY
            Book.BookImage, Book.BookTitle, Book.BookSeries, Author.AuthorName, 
            Book.BookDesc";

            object[] checkParams = { "@BookId", BookId };
            DataTable rt = fyp.DBHelper.ExecuteQuery(query, checkParams);

            try
            {
                if (rt.Rows.Count > 0)
                {
                    DataRow row = rt.Rows[0];

                    // Create a new instance of BookData and populate it
                    var bookData = new BookData
                    {
                        Title = row["BookTitle"].ToString(),
                        Author = row["AuthorName"].ToString(),
                        Description = row["BookDesc"].ToString(),
                        Series = row["BookSeries"].ToString(),
                        Categories = row["CategoryNames"] != DBNull.Value ? row["CategoryNames"].ToString() : "N/A",
                        TotalRating = row["TotalRating"] != DBNull.Value ? Convert.ToInt32(row["TotalRating"]) : 0,
                        RatingCount = row["RatingCount"] != DBNull.Value ? Convert.ToInt32(row["RatingCount"]) : 0,
                    };

                    // Calculate the average rating and convert it to stars
                    bookData.AverageRating = bookData.RatingCount > 0 ?
                        (int)Math.Round((double)bookData.TotalRating / bookData.RatingCount) : 0;
                    bookData.CoverImageBase64 = row["BookImage"] != DBNull.Value ?
                        "data:image/png;base64," + Convert.ToBase64String((byte[])row["BookImage"]) : "~/images/defaultCoverBook.png";

                    // Save the BookData object in session
                    Session["BookData"] = bookData;
                }
                else
                {
                    lblMessage.Text = "Book not found.";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "An error occurred while loading book data: " + ex.Message;
            }
        }

        //update Book
        protected void btnSubmitUp_Click(object sender, EventArgs e)
        {
            try
            {
                // Retrieve book information from input fields
                string bookTitle = txtBookTitle.Text;
                string bookDescription = txtBookDesc.Text;
                string bookSeries = txtBookSeries.Text;
                string authorName = txtAuthorName.Text;
                int bookId = Convert.ToInt32(Session["BookId"]); // Retrieve BookId from session

                // Retrieve selected categories (by name)
                List<string> selectedCategories = new List<string>();
                foreach (ListItem item in cblCategoryIds.Items)
                {
                    if (item.Selected)
                    {
                        selectedCategories.Add(item.Value);
                    }
                }

                // Optional: Update the book image if a new file is uploaded
                byte[] bookImage = null;
                if (fileUploadBookImage.HasFile)
                {
                    using (System.IO.Stream fs = fileUploadBookImage.PostedFile.InputStream)
                    using (System.IO.BinaryReader br = new System.IO.BinaryReader(fs))
                    {
                        bookImage = br.ReadBytes((int)fs.Length);
                    }
                }

                // Step 1: Update Book details
                string updateBookQuery = @"
            UPDATE Book
            SET
                BookTitle = @BookTitle,
                BookSeries = @BookSeries,
                BookDesc = @BookDesc" +
                        (bookImage != null ? ", BookImage = @BookImage" : "") + @"
            WHERE BookId = @BookId;";

                List<object> updateParams = new List<object> {
            "@BookTitle", bookTitle,
            "@BookSeries", bookSeries,
            "@BookDesc", bookDescription,
            "@BookId", bookId
        };

                if (bookImage != null)
                {
                    updateParams.Add("@BookImage");
                    updateParams.Add(bookImage);
                }

                fyp.DBHelper.ExecuteNonQuery(updateBookQuery, updateParams.ToArray());

                // Step 2: Update or Insert Author and update BookAuthor relationship
                string checkAuthorQuery = "SELECT AuthorId FROM Author WHERE AuthorName = @AuthorName";
                object[] checkAuthorParams = { "@AuthorName", authorName };
                DataTable authorTable = fyp.DBHelper.ExecuteQuery(checkAuthorQuery, checkAuthorParams);

                int authorId;
                if (authorTable.Rows.Count > 0)
                {
                    // Author exists; retrieve AuthorId
                    authorId = Convert.ToInt32(authorTable.Rows[0]["AuthorId"]);
                }
                else
                {
                    // Author does not exist; insert new author and retrieve new AuthorId
                    string insertAuthorQuery = "INSERT INTO Author (AuthorName) VALUES (@AuthorName); SELECT SCOPE_IDENTITY();";
                    authorId = Convert.ToInt32(fyp.DBHelper.ExecuteScalar(insertAuthorQuery, checkAuthorParams));
                }

                // Update the BookAuthor table to associate the book with the correct author
                string updateBookAuthorQuery = @"
                    DELETE FROM BookAuthor WHERE BookId = @BookId;
                    INSERT INTO BookAuthor (BookId, AuthorId) VALUES (@BookId, @AuthorId);";

                object[] bookAuthorParams = { "@BookId", bookId, "@AuthorId", authorId };
                fyp.DBHelper.ExecuteNonQuery(updateBookAuthorQuery, bookAuthorParams);

                // Step 3: Update Categories - Delete existing and add new associations
                string deleteCategoriesQuery = "DELETE FROM BookCategory WHERE BookId = @BookId";
                fyp.DBHelper.ExecuteNonQuery(deleteCategoriesQuery, new object[] { "@BookId", bookId });

                // Retrieve CategoryIds for selected category names
                string selectCategoryIdsQuery = "SELECT CategoryId FROM Category WHERE CategoryName = @CategoryName";
                List<int> categoryIds = new List<int>();

                foreach (string categoryName in selectedCategories)
                {
                    object[] categoryParams = { "@CategoryName", categoryName };
                    DataTable dt = fyp.DBHelper.ExecuteQuery(selectCategoryIdsQuery, categoryParams);

                    // Collect each CategoryId for selected category names
                    foreach (DataRow row in dt.Rows)
                    {
                        categoryIds.Add(Convert.ToInt32(row["CategoryId"]));
                    }
                }

                // Insert new category associations into BookCategory
                string insertCategoryQuery = "INSERT INTO BookCategory (BookId, CategoryId) VALUES (@BookId, @CategoryId)";
                foreach (int categoryId in categoryIds)
                {
                    object[] insertParams = { "@BookId", bookId, "@CategoryId", categoryId };
                    fyp.DBHelper.ExecuteNonQuery(insertCategoryQuery, insertParams);
                }

                // Display success message
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Book information updated successfully.');", true);
                PredefinedData();
            }
            catch (Exception ex)
            {
                string errorMessage = "An error occurred: " + ex.Message;
                ScriptManager.RegisterStartupScript(this, this.GetType(), "errorAlert", "alert('" + errorMessage.Replace("'", "\\'") + "');", true);
            }
        }

        [System.Web.Services.WebMethod]
        public static object GetBookCopyDetails(int bookCopyId)
        {
            string query = @"
            SELECT bc.BookCopyId, bc.PublishDate, bc.PublishOwner, bc.IsAvailable, bc.BookCopyImage, 
                   bc.BookId, b.BookTitle
            FROM BookCopy AS bc 
            INNER JOIN Book AS b ON bc.BookId = b.BookId
            WHERE bc.BookCopyId = @BookCopyId AND bc.IsDeleted = 0";

            object[] checkParams = { "@BookCopyId", bookCopyId };
            DataTable rt = fyp.DBHelper.ExecuteQuery(query, checkParams);

            if (rt.Rows.Count != 0)
            {

                string publishDate = rt.Rows[0]["PublishDate"] != DBNull.Value ? Convert.ToDateTime(rt.Rows[0]["PublishDate"]).ToString("yyyy-MM-dd") : string.Empty;

                return new
                {
                    BookCopyId = rt.Rows[0]["BookCopyId"],
                    PublishDate = publishDate,
                    PublishOwner = rt.Rows[0]["PublishOwner"].ToString(),
                    BookTitle = rt.Rows[0]["BookTitle"],
                    BookCopyImage = rt.Rows[0]["BookCopyImage"] != DBNull.Value
                                    ? Convert.ToBase64String((byte[])rt.Rows[0]["BookCopyImage"])
                                    : null // Return null if no image
                };
            }
            else
            {
                return null;
            }


        }


        //delete book
        [WebMethod]
        public static string DeleteBook(int bookId)
        {
            try
            {
                if (bookId > 0)
                {
                    string updateBookQuery = "UPDATE Book SET IsDeleted = 1 WHERE BookId = @BookId";
                    fyp.DBHelper.ExecuteNonQuery(updateBookQuery, new object[] { "@BookId", bookId });

                    return "Success";  // Return success without redirecting
                }
                else
                {
                    return "Error: Invalid BookId";
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        //delete book Copy
        [WebMethod]
        public static string DeleteBookCopy(int bookId)
        {
            try
            {
                if (bookId > 0)
                {
                    string updateBookQuery = "UPDATE BookCopy SET IsDeleted = 1 WHERE BookId = @BookId";
                    fyp.DBHelper.ExecuteNonQuery(updateBookQuery, new object[] { "@BookId", bookId });

                    return "Success";  // Return success without redirecting
                }
                else
                {
                    return "Error: Invalid BookId";
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        protected void GridView1_RowEditing(object sender, GridViewEditEventArgs e)
        {
            GridView1.EditIndex = e.NewEditIndex;
            GridView1.DataBind(); // Rebind data to refresh GridView with editable row
        }

        protected void GridView1_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            GridView1.EditIndex = -1;
            GridView1.DataBind(); // Exit edit mode
        }

        protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                // Get the row being edited
                GridViewRow row = GridView1.Rows[e.RowIndex];

                // Retrieve values from controls
                int bookCopyId = Convert.ToInt32(GridView1.DataKeys[e.RowIndex].Value);
                DateTime publishDate = ((Calendar)row.FindControl("calendarEditPublishDate")).SelectedDate;
                string publishOwner = ((TextBox)row.FindControl("txtEditPublishOwner")).Text;

                // Initialize query and parameters
                string updateQuery;
                var parameters = new List<SqlParameter>
        {
            new SqlParameter("@PublishDate", SqlDbType.Date) { Value = publishDate },
            new SqlParameter("@PublishOwner", SqlDbType.NVarChar, 100) { Value = publishOwner ?? (object)DBNull.Value },
            new SqlParameter("@BookCopyId", SqlDbType.Int) { Value = bookCopyId }
        };

                // Handle file upload
                FileUpload fileUpload = (FileUpload)row.FindControl("fileUploadBookCopyImage");
                if (fileUpload.HasFile)
                {
                    using (Stream fs = fileUpload.PostedFile.InputStream)
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        byte[] bookCopyImage = br.ReadBytes((int)fs.Length);
                        parameters.Add(new SqlParameter("@BookCopyImage", SqlDbType.VarBinary, -1) { Value = bookCopyImage });
                    }

                    // Include BookCopyImage in the query
                    updateQuery = @"
                UPDATE BookCopy
                SET PublishDate = @PublishDate,
                    PublishOwner = @PublishOwner,
                    BookCopyImage = @BookCopyImage
                WHERE BookCopyId = @BookCopyId";
                }
                else
                {
                    // Exclude BookCopyImage from the query
                    updateQuery = @"
                UPDATE BookCopy
                SET PublishDate = @PublishDate,
                    PublishOwner = @PublishOwner
                WHERE BookCopyId = @BookCopyId";
                }

                // Execute the query
                fyp.DBHelper.NonQuery(updateQuery, parameters);

                // Exit edit mode
                GridView1.EditIndex = -1;
                GridView1.DataBind();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                throw;
            }
        }

        protected void btnSubmitAddBC_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the BookId from the session
                string bookId = Session["BookId"]?.ToString();
                if (string.IsNullOrEmpty(bookId))
                {
                    lblAddErrorMessage.Text = "Book ID is missing. Please try again.";
                    return;
                }

                // Retrieve values from the modal
                string publishDateInput = txtAddPublishDate.Text;
                string publishOwner = txtAddPublishOwner.Text;
                byte[] bookImage = null;

                // Validate and parse Publish Date
                DateTime publishDate;
                if (!DateTime.TryParse(publishDateInput, out publishDate))
                {
                    lblAddErrorMessage.Text = "Invalid Publish Date. Please use the format YYYY-MM-DD.";
                    return;
                }

                // Handle file upload
                if (fileUploadAddBookImage.HasFile)
                {
                    using (Stream fs = fileUploadAddBookImage.PostedFile.InputStream)
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        bookImage = br.ReadBytes((int)fs.Length);
                    }
                }

                // Prepare the SQL query and parameters
                string insertQuery = @"
            INSERT INTO BookCopy (PublishDate, PublishOwner, IsAvailable, BookCopyImage, BookId)
            VALUES (@PublishDate, @PublishOwner, @IsAvailable, @BookCopyImage, @BookId)";

                var parameters = new List<SqlParameter>
        {
            new SqlParameter("@PublishDate", SqlDbType.Date) { Value = publishDate },
            new SqlParameter("@PublishOwner", SqlDbType.NVarChar, 100) { Value = publishOwner },
            new SqlParameter("@IsAvailable", SqlDbType.Bit) { Value = true }, // Default IsAvailable to true
            new SqlParameter("@BookCopyImage", SqlDbType.VarBinary) { Value = (object)bookImage ?? DBNull.Value },
            new SqlParameter("@BookId", SqlDbType.Int) { Value = int.Parse(bookId) }
        };

                // Execute the query
                fyp.DBHelper.NonQuery(insertQuery, parameters);

                // Refresh the GridView
                GridView1.DataBind();

                // Close the modal and reset the form
                txtAddPublishDate.Text = string.Empty;
                txtAddPublishOwner.Text = string.Empty;
                lblAddErrorMessage.Text = string.Empty;

                // Hide modal using JavaScript
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal", "hideAddBookCopyModal();", true);
            }
            catch (Exception ex)
            {
                lblAddErrorMessage.Text = $"Error: {ex.Message}";
            }
        }

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DownloadBarcode")
            {
                string isbn = e.CommandArgument.ToString();

                if (!string.IsNullOrEmpty(isbn))
                {
                    try
                    {
                        // Use ZXing.Net to generate a barcode
                        var barcodeWriter = new BarcodeWriter
                        {
                            Format = BarcodeFormat.EAN_13, // Format suitable for ISBN
                            Options = new EncodingOptions
                            {
                                Width = 300, // Width of the barcode
                                Height = 150, // Height of the barcode
                                Margin = 2 // Margin around the barcode
                            }
                        };

                        // Generate the barcode as a Bitmap
                        Bitmap barcodeBitmap = barcodeWriter.Write(isbn);

                        // Convert the Bitmap to a byte array for download
                        using (MemoryStream ms = new MemoryStream())
                        {
                            barcodeBitmap.Save(ms, ImageFormat.Png);
                            byte[] byteImage = ms.ToArray();

                            // Trigger file download
                            Response.Clear();
                            Response.ContentType = "image/png";
                            Response.AddHeader("Content-Disposition", $"attachment; filename=Barcode_{isbn}.png");
                            Response.BinaryWrite(byteImage);
                            Response.End();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle errors gracefully
                        ClientScript.RegisterStartupScript(this.GetType(), "alert", $"alert('Error generating barcode: {ex.Message}');", true);
                    }
                }
                else
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Invalid ISBN value. Cannot generate barcode.');", true);
                }
            }
        }






    }



}
