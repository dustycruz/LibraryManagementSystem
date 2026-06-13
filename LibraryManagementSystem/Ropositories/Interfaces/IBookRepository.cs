using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Ropositories.Interfaces
{
    public interface IBookRepository : IGenericRepository<Book>
    {
        Task<(List<Book> Books, int TotalCount)> GetBooksPagedAsync(
            string? searchTerm, int? categoryId, bool? isActive,
            int pageNumber, int pageSize);
        Task<Book?> GetByISBNAsync(string isbn);
        Task<Book?> GetWithCategoryAsync(int bookId);
        Task<bool> HasActiveBorrowsAsync(int bookId);
    }
}
