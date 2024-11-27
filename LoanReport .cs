using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace fyp
{
    public class LoanReport
    {
        public int LoanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int BookCopyId { get; set; }
        public string BookTitle { get; set; }
        public string CategoryNames { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime? LatestReturn { get; set; } // Nullable in case there's no punishment record
    }

    public class BookReport
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string BookDesc { get; set; }
        public string BookSeries { get; set; }
        public DateTime CreatedAt { get; set; } // Corresponding to the CreatedAt date
        public string CategoryNames { get; set; } // Updated to match query alias
        public string AuthorName { get; set; }
    }
}