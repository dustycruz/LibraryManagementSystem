using static System.Net.WebRequestMethods;

namespace LibraryManagementSystem.Models
{
    public class BorrowRecord
    {
        public int BorrowId { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "Borrowed";
        public int? ProcessedByUserId { get; set; }

        public User User { get; set; } = null!;
        public Book Book { get; set; } = null!;
        public User? ProcessedByUser { get; set; }
        public Fine? Fine { get; set; }
    }
}
