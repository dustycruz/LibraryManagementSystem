namespace LibraryManagementSystem.DTOs.Reports
{
    public class UserActivityReportDto
    {
        public int UserId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalBorrows { get; set; }
        public int ActiveBorrows { get; set; }
        public int ReturnedBooks { get; set; }
        public decimal TotalFines { get; set; }
        public decimal PaidFines { get; set; }
        public decimal UnpaidFines { get; set; }
    }
}
