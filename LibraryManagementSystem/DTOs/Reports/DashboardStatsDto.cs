namespace LibraryManagementSystem.DTOs.Reports
{
    public class DashboardStatsDto
    {
        public int TotalBooks { get; set; }
        public int TotalAvailableCopies { get; set; }
        public int ActiveBorrows { get; set; }
        public int OverdueCount { get; set; }
        public int TotalActiveUsers { get; set; }
        public decimal TotalUnpaidFines { get; set; }
    }
}
