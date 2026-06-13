using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Ropositories.Interfaces
{
    public interface IBorrowRepository : IGenericRepository<BorrowRecord>
    {
        Task<List<BorrowRecord>> GetUserBorrowsAsync(int userId);
        Task<List<BorrowRecord>> GetAllBorrowsPagedAsync(string? status, int pageNumber, int pageSize);
        Task<int> GetAllBorrowsCountAsync(string? status);
        Task<BorrowRecord?> GetBorrowWithDetailsAsync(int borrowId);
        Task<int> GetActiveBorrowCountAsync(int userId);
        Task<List<BorrowRecord>> GetOverdueBorrowsAsync();
    }
}
