namespace LibraryManagementSystem.Models
{
    public class Fine
    {
        public int FineId { get; set; }
        public int BorrowId { get; set; }
        public decimal Amount { get; set; } = 0;
        public bool IsPaid { get; set; } = false;
        public DateTime? PaidDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public BorrowRecord BorrowRecord { get; set; } = null!;
    }
}
