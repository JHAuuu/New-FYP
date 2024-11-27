using System;
using fyp.Authentication;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Web.Services;

namespace fyp
{
    public partial class BookManagement : AdminPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            var master = this.Master as DashMasterPage;
            if (master != null)
            {
                master.titleText = "Book Management";
            }

            // This binds the DropDownList, make sure SqlDataSourceCategory is set correctly.
            cblCategoryIds.DataBind();

            if (!IsPostBack)
            {
                BooksRepeater.DataBind();
            }
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            // Get the button that was clicked  
            Button btnView = (Button)sender;

            // Retrieve the CommandArgument which contains the BookId  
            string bookId = btnView.CommandArgument;

            // Store BookId in session  
            Session["BookId"] = bookId;

            // Redirect to BookCopyManagement.aspx  
            Response.Redirect("~/BookCopyManagement.aspx");
        }



        protected void btnSubmitBook_Click(object sender, EventArgs e)
        {
            string bookTitle = txtBookTitle.Text;
            string bookDescription = txtBookDesc.Text;
            string bookSeries = txtBookSeries.Text;

            try
            {
                // Check if the book title already exists
                string checkQuery = "SELECT BookTitle FROM Book WHERE BookTitle = @BookTitle";
                object[] checkParams = { "@BookTitle", txtBookTitle.Text };

                DataTable resultTable = fyp.DBHelper.ExecuteQuery(checkQuery, checkParams);

                if (resultTable.Rows.Count > 0)
                {
                    string script = "alert('The book \"" + txtBookTitle.Text + "\" already exists.');";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "BookExistsAlert", script, true);
                }
                else
                {
                    // Optional: Update the book image if a new file is uploaded
                    byte[] bookImage = null;
                    bool hasImage = fileUploadBookImage.HasFile;

                    if (hasImage)
                    {
                        using (System.IO.Stream fs = fileUploadBookImage.PostedFile.InputStream)
                        using (System.IO.BinaryReader br = new System.IO.BinaryReader(fs))
                        {
                            bookImage = br.ReadBytes((int)fs.Length);
                        }
                    }

                    // Prepare the SQL query and parameters based on whether there's an image
                    string insertBookQuery;
                    object[] insertBookParams;

                    if (hasImage)
                    {
                        insertBookQuery = "INSERT INTO Book (BookTitle, BookDesc, BookSeries, BookImage) OUTPUT INSERTED.BookId VALUES (@BookTitle, @BookDesc, @BookSeries, @BookImage)";
                        insertBookParams = new object[] {
                    "@BookTitle", bookTitle,
                    "@BookDesc", bookDescription,
                    "@BookSeries", bookSeries,
                    "@BookImage", bookImage
                };
                    }
                    else
                    {
                        insertBookQuery = "INSERT INTO Book (BookTitle, BookDesc, BookSeries) OUTPUT INSERTED.BookId VALUES (@BookTitle, @BookDesc, @BookSeries)";
                        insertBookParams = new object[] {
                    "@BookTitle", bookTitle,
                    "@BookDesc", bookDescription,
                    "@BookSeries", bookSeries
                };
                    }

                    // Execute the query to insert the new book and get the new BookId
                    int newBookId = (int)fyp.DBHelper.ExecuteScalar(insertBookQuery, insertBookParams);

                    // Link selected categories to the new book
                    foreach (ListItem item in cblCategoryIds.Items)
                    {
                        if (item.Selected)
                        {
                            int categoryId = Convert.ToInt32(item.Value);
                            string insertCategoryQuery = "INSERT INTO BookCategory (BookId, CategoryId) VALUES (@BookId, @CategoryId)";
                            object[] categoryParams = { "@BookId", newBookId, "@CategoryId", categoryId };
                            fyp.DBHelper.ExecuteNonQuery(insertCategoryQuery, categoryParams);
                        }
                    }

                    // Refresh the book list and show a success alert
                    BooksRepeater.DataBind();
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "SuccessAlert", "alert('Book inserted successfully!');", true);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"An error occurred: {ex.Message}";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ErrorAlert", $"alert('{errorMessage}');", true);
            }


        }

            // Define response class to structure response from InsertBook
        public class InsertBookResponse
        {
            public bool success { get; set; }  // Change to lowercase 's'
            public string message { get; set; } // Change to lowercase 'm'
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = searchBar.Text.Trim();

            if (!string.IsNullOrEmpty(searchText))
            {
                // Update SelectCommand to filter based on BookTitle or BookDesc
                SqlDataSource1.SelectCommand = "SELECT BookId, BookTitle, BookDesc, BookSeries, BookImage FROM Book " +
                                               "WHERE IsDeleted = 0 AND (BookTitle LIKE '%' + @SearchText + '%' OR BookDesc LIKE '%' + @SearchText + '%')";
                SqlDataSource1.SelectParameters.Clear();
                SqlDataSource1.SelectParameters.Add("SearchText", searchText);
            }
            else
            {
                // Reset to show all books if search text is empty
                SqlDataSource1.SelectCommand = "SELECT BookId, BookTitle, BookDesc, BookSeries, BookImage FROM Book WHERE IsDeleted = 0";
            }

            // Rebind the Repeater to apply the updated filter
            BooksRepeater.DataBind();
        }

    }
}