using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Ropositories.Interfaces
{
    public interface IFineRepository : IGenericRepository<Fine>
    {
        Task<Fine?> GetByBorrowIdAsync(int borrowId);
        Task<List<Fine>> GetUnpaidFinesByUserAsync(int userId);
    }
}
