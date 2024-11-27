using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp.text; // For iTextSharp specific types like Font, Rectangle, Paragraph
using iTextSharp.text.pdf; // For PdfPTable, PdfPCell, etc.
using System.Drawing; // For System.Drawing.Image
using System.IO;      // For MemoryStream

namespace fyp
{
    public class PdfUtility
    {
        public class PageEvents : iTextSharp.text.pdf.PdfPageEventHelper
        {
            public override void OnEndPage(PdfWriter writer, Document document)
            {
                base.OnEndPage(writer, document);

                // Define font for page number
                iTextSharp.text.Font font = iTextSharp.text.FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

                // Get current page number
                int pageNumber = writer.PageNumber;

                // Define the position (bottom-center)
                float x = document.PageSize.GetLeft(0) + (document.PageSize.Width / 2);
                float y = document.PageSize.GetBottom(30); // 30 units from the bottom

                // Create the page number text and add it to the document
                string pageText = $"Page {pageNumber}";
                ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_CENTER, new Phrase(pageText, font), x, y, 0);
            }
        }

        public static byte[] GenerateLoanPdfReport(string title, IEnumerable<LoanReport> reportData, DateTime startDate, DateTime endDate)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4.Rotate(), 50, 50, 25, 25); // Rotate for more columns
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

                // Assign the page event handler to the PdfWriter
                writer.PageEvent = new PageEvents();

                document.Open();

                // Create the title with background and text color
                iTextSharp.text.Font titleFont = iTextSharp.text.FontFactory.GetFont("Arial", 18, iTextSharp.text.Font.BOLD, BaseColor.WHITE); // White text
                Chunk titleChunk = new Chunk(title, titleFont);

                // Set background color for the title
                PdfPCell titleCell = new PdfPCell(new Phrase(titleChunk))
                {
                    BackgroundColor = new BaseColor(0, 61, 142), // RGB value for #003d8e
                    Border = iTextSharp.text.Rectangle.NO_BORDER, // Use iTextSharp's Rectangle
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                // Add title to the document in a table format (with no borders)
                PdfPTable titleTable = new PdfPTable(1) { WidthPercentage = 100 };
                titleTable.AddCell(titleCell);
                document.Add(titleTable);

                document.Add(new Paragraph(" ")); // Add some space after the title

                // Add date range under the title
                iTextSharp.text.Font dateRangeFont = iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK); // Date range font
                string dateRangeText = $"Date: {startDate.ToString("d/M/yyyy")} - {endDate.ToString("d/M/yyyy")}";
                Paragraph dateRangeParagraph = new Paragraph(dateRangeText, dateRangeFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(dateRangeParagraph);

                document.Add(new Paragraph(" ")); // Add some space after the date range

                // Create a dictionary of category percentages
                var categoryPercentages = new Dictionary<string, float>();
                foreach (var item in reportData)
                {
                    foreach (var category in item.CategoryNames.Split(','))
                    {
                        var trimmedCategory = category.Trim(); // Trim spaces if any
                        if (categoryPercentages.ContainsKey(trimmedCategory))
                            categoryPercentages[trimmedCategory]++;
                        else
                            categoryPercentages.Add(trimmedCategory, 1);
                    }
                }

                // Normalize the category counts to percentages
                float totalCategories = categoryPercentages.Values.Sum();
                foreach (var category in categoryPercentages.Keys.ToList())
                {
                    categoryPercentages[category] = (categoryPercentages[category] / totalCategories) * 100;
                }

                // Generate the pie chart image using System.Drawing (with labels for categories)
                System.Drawing.Image chartImage = ChartUtility.GeneratePieChartWithLabels(categoryPercentages);

                // Convert System.Drawing.Image to iTextSharp.text.Image
                using (MemoryStream chartStream = new MemoryStream())
                {
                    chartImage.Save(chartStream, System.Drawing.Imaging.ImageFormat.Png);  // Save the image as PNG

                    byte[] chartBytes = chartStream.ToArray();  // Convert to byte array

                    // Create iTextSharp Image from byte array
                    iTextSharp.text.Image pdfChartImage = iTextSharp.text.Image.GetInstance(chartBytes);
                    pdfChartImage.ScaleToFit(200f, 200f);  // Optionally scale the image
                    pdfChartImage.Alignment = Element.ALIGN_CENTER;
                    document.Add(pdfChartImage);  // Add the chart image to the document
                }

                // Add the text: Percentage of loan book categories under the pie chart
                iTextSharp.text.Font categoryTextFont = iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                Paragraph categoryTextParagraph = new Paragraph("Percentage of loan book categories", categoryTextFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(categoryTextParagraph);

                document.Add(new Paragraph(" ")); // Add some space after the text

                foreach (var item in reportData)
                {
                    // Add Book Copy ID and Book Title as a header
                    iTextSharp.text.Font headerFont = iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    Paragraph bookHeader = new Paragraph($"Book Copy ID: {item.BookCopyId}        Book Name: {item.BookTitle}", headerFont)
                    {
                        Alignment = Element.ALIGN_LEFT
                    };
                    document.Add(bookHeader);

                    document.Add(new Paragraph(" ")); // Add some space after the header

                    // Define the table for this record
                    PdfPTable table = new PdfPTable(6) { WidthPercentage = 100 }; // Adjusted to 6 columns
                    table.SetWidths(new float[] { 10f, 20f, 30f, 25f, 20f, 20f }); // Adjusted column widths

                    iTextSharp.text.Font tableHeaderFont = iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    PdfPCell headerCell;
                    // Add table headers
                    headerCell = new PdfPCell(new Phrase("User ID", tableHeaderFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(headerCell);
                    headerCell = new PdfPCell(new Phrase("User Name", tableHeaderFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(headerCell);
                    headerCell = new PdfPCell(new Phrase("Book Title", tableHeaderFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(headerCell);
                    headerCell = new PdfPCell(new Phrase("Categories", tableHeaderFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(headerCell);
                    headerCell = new PdfPCell(new Phrase("Start Date", tableHeaderFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(headerCell);
                    headerCell = new PdfPCell(new Phrase("Latest Return", tableHeaderFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(headerCell);

                    // Add horizontal line after headers
                    PdfPCell hrCell = new PdfPCell(new Phrase(" ")) // Empty cell
                    {
                        Colspan = 6, // Span across all columns
                        BorderWidthTop = 1f, // Line thickness
                        BorderWidthBottom = 0f, // No bottom border
                        BorderWidthLeft = 0f,
                        BorderWidthRight = 0f,
                        BorderColorTop = BaseColor.GRAY // Line color
                    };
                    table.AddCell(hrCell);

                    // Add data for this record
                    iTextSharp.text.Font cellFont = iTextSharp.text.FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    PdfPCell dataCell;

                    dataCell = new PdfPCell(new Phrase(item.UserId.ToString(), cellFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(dataCell);
                    dataCell = new PdfPCell(new Phrase(item.UserName, cellFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(dataCell);
                    dataCell = new PdfPCell(new Phrase(item.BookTitle, cellFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(dataCell);
                    dataCell = new PdfPCell(new Phrase(item.CategoryNames, cellFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(dataCell);
                    dataCell = new PdfPCell(new Phrase(item.StartDate.ToShortDateString(), cellFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(dataCell);
                    dataCell = new PdfPCell(new Phrase(item.LatestReturn.HasValue ? item.LatestReturn.Value.ToShortDateString() : "N/A", cellFont)){ Border = iTextSharp.text.Rectangle.NO_BORDER};
                    table.AddCell(dataCell);

                    // Add the table to the document
                    document.Add(table);

                    document.Add(new Paragraph(" ")); // Add space after each table
                }

                // Add Total Book Loan count at the bottom of the table
                iTextSharp.text.Font totalFont = iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                float totalLoanCount = reportData.Count();
                Paragraph totalLoanParagraph = new Paragraph($"Total Book Loan: {totalLoanCount}", totalFont)
                {
                    Alignment = Element.ALIGN_RIGHT
                };
                document.Add(totalLoanParagraph);

                document.Close();
                writer.Close();

                return memoryStream.ToArray();
            }
        }



        //add book reports
        public static byte[] GenerateBookPdfReport(string title, IEnumerable<BookReport> reportData, DateTime startDate, DateTime endDate)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4.Rotate(), 50, 50, 25, 25); // Rotate for more columns
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

                // Assign the page event handler to the PdfWriter
                writer.PageEvent = new PageEvents();

                document.Open();

                // Create the title with background and text color
                iTextSharp.text.Font titleFont = iTextSharp.text.FontFactory.GetFont("Arial", 18, iTextSharp.text.Font.BOLD, BaseColor.WHITE); // White text
                Chunk titleChunk = new Chunk(title, titleFont);

                // Set background color for the title
                PdfPCell titleCell = new PdfPCell(new Phrase(titleChunk))
                {
                    BackgroundColor = new BaseColor(0, 61, 142), // RGB value for #003d8e
                    Border = iTextSharp.text.Rectangle.NO_BORDER, // Use iTextSharp's Rectangle
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                // Add title to the document in a table format (with no borders)
                PdfPTable titleTable = new PdfPTable(1) { WidthPercentage = 100 };
                titleTable.AddCell(titleCell);
                document.Add(titleTable);

                document.Add(new Paragraph(" ")); // Add some space after the title

                // Add date range under the title
                iTextSharp.text.Font dateRangeFont = iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK); // Date range font
                string dateRangeText = $"Date: {startDate.ToString("d/M/yyyy")} - {endDate.ToString("d/M/yyyy")}";
                Paragraph dateRangeParagraph = new Paragraph(dateRangeText, dateRangeFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(dateRangeParagraph);

                document.Add(new Paragraph(" ")); // Add some space after the date range

                // Create a dictionary of category percentages
                var categoryPercentages = new Dictionary<string, float>();
                foreach (var item in reportData)
                {
                    foreach (var category in item.CategoryNames.Split(','))
                    {
                        var trimmedCategory = category.Trim(); // Trim spaces if any
                        if (categoryPercentages.ContainsKey(trimmedCategory))
                            categoryPercentages[trimmedCategory]++;
                        else
                            categoryPercentages.Add(trimmedCategory, 1);
                    }
                }

                // Normalize the category counts to percentages
                float totalCategories = categoryPercentages.Values.Sum();
                foreach (var category in categoryPercentages.Keys.ToList())
                {
                    categoryPercentages[category] = (categoryPercentages[category] / totalCategories) * 100;
                }

                // Generate the pie chart image using System.Drawing (with labels for categories)
                System.Drawing.Image chartImage = ChartUtility.GeneratePieChartWithLabels(categoryPercentages);

                // Convert System.Drawing.Image to iTextSharp.text.Image
                using (MemoryStream chartStream = new MemoryStream())
                {
                    chartImage.Save(chartStream, System.Drawing.Imaging.ImageFormat.Png);  // Save the image as PNG

                    byte[] chartBytes = chartStream.ToArray();  // Convert to byte array

                    // Create iTextSharp Image from byte array
                    iTextSharp.text.Image pdfChartImage = iTextSharp.text.Image.GetInstance(chartBytes);
                    pdfChartImage.ScaleToFit(200f, 200f);  // Optionally scale the image
                    pdfChartImage.Alignment = Element.ALIGN_CENTER;
                    document.Add(pdfChartImage);  // Add the chart image to the document
                }

                // Add the text: Percentage of loan book categories under the pie chart
                iTextSharp.text.Font categoryTextFont = iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                Paragraph categoryTextParagraph = new Paragraph("Percentage of Add book categories", categoryTextFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(categoryTextParagraph);

                document.Add(new Paragraph(" ")); // Add some space after the text

                // Loop through each book
                iTextSharp.text.Font headerFont = iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                iTextSharp.text.Font cellFont = iTextSharp.text.FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

                // Define table with more columns for additional data
                foreach (var item in reportData)
                {
                    // Add Book Header
                    Paragraph bookHeader = new Paragraph($"Book ID: {item.BookId}        Book Name: {item.BookTitle}", headerFont)
                    {
                        Alignment = Element.ALIGN_LEFT
                    };
                    document.Add(bookHeader);

                    document.Add(new Paragraph(" ")); // Add spacing

                    // Define a table for book details
                    PdfPTable table = new PdfPTable(4) { WidthPercentage = 100 }; // Adjusted for 4 columns
                    table.SetWidths(new float[] { 25f, 25f, 25f, 25f });

                    // Add headers
                    PdfPCell headerCell;

                    headerCell = new PdfPCell(new Phrase("Book Description", headerFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(headerCell);

                    headerCell = new PdfPCell(new Phrase("Book Series", headerFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(headerCell);

                    headerCell = new PdfPCell(new Phrase("CreatedAt", headerFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(headerCell);

                    headerCell = new PdfPCell(new Phrase("Categories", headerFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(headerCell);

                    // Add horizontal line after headers
                    PdfPCell hrCell = new PdfPCell(new Phrase(" ")) // Empty cell
                    {
                        Colspan = 6, // Span across all columns
                        BorderWidthTop = 1f, // Line thickness
                        BorderWidthBottom = 0f, // No bottom border
                        BorderWidthLeft = 0f,
                        BorderWidthRight = 0f,
                        BorderColorTop = BaseColor.GRAY // Line color
                    };
                    table.AddCell(hrCell);

                    // Add data
                    PdfPCell dataCell;

                    dataCell = new PdfPCell(new Phrase(item.BookDesc, cellFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(dataCell);

                    dataCell = new PdfPCell(new Phrase(item.BookSeries, cellFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(dataCell);

                    dataCell = new PdfPCell(new Phrase(item.CreatedAt.ToShortDateString(), cellFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(dataCell);

                    dataCell = new PdfPCell(new Phrase(item.CategoryNames, cellFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER };
                    table.AddCell(dataCell);

                    document.Add(table); // Add table to document

                    document.Add(new Paragraph(" ")); // Add space after each book
                }

                // Add Total Book Loan count at the bottom of the table
                iTextSharp.text.Font totalFont = iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                float totalBookCount = reportData.Count();
                Paragraph totalLoanParagraph = new Paragraph($"Total Book Add: {totalBookCount}", totalFont)
                {
                    Alignment = Element.ALIGN_RIGHT
                };
                document.Add(totalLoanParagraph);

                document.Close();
                writer.Close();

                return memoryStream.ToArray();
            }
        }





    }


}