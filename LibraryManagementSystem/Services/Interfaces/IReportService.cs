using LibraryManagementSystem.DTOs.Reports;

namespace LibraryManagementSystem.Services.Interfaces
{
    public interface IReportService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<List<BookReportDto>> GetAvailableBooksReportAsync();
        Task<List<BookReportDto>> GetMostBorrowedBooksReportAsync(int top = 10);
        Task<List<OverdueReportDto>> GetOverdueBooksReportAsync();
        Task<List<UserActivityReportDto>> GetUserActivityReportAsync();
    }
}
