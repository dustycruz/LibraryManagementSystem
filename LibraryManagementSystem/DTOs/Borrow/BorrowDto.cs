namespace LibraryManagementSystem.DTOs.Borrow
{
    public class BorrowDto
    {
        public int BorrowId { get; set; }
        public int UserId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string MemberEmail { get; set; } = string.Empty;
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookISBN { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int DaysOverdue { get; set; }
        public decimal? FineAmount { get; set; }
        public bool? FineIsPaid { get; set; }
    }
}
