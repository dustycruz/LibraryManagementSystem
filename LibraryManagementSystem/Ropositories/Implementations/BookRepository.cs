// BookRepository.cs
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Ropositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Repositories.Implementations;

public class BookRepository : GenericRepository<Book>, IBookRepository
{
    public BookRepository(LibraryDbContext context) : base(context) { }

    public async Task<(List<Book> Books, int TotalCount)> GetBooksPagedAsync(
        string? searchTerm, int? categoryId, bool? isActive,
        int pageNumber, int pageSize)
    {
        var query = _context.Books.Include(b => b.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(term) ||
                b.Author.ToLower().Contains(term) ||
                b.ISBN.ToLower().Contains(term) ||
                (b.Publisher != null && b.Publisher.ToLower().Contains(term)));
        }

        if (categoryId.HasValue)
            query = query.Where(b => b.CategoryId == categoryId.Value);

        if (isActive.HasValue)
            query = query.Where(b => b.IsActive == isActive.Value);

        var totalCount = await query.CountAsync();
        var books = await query
            .OrderBy(b => b.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (books, totalCount);
    }

    public async Task<Book?> GetByISBNAsync(string isbn)
        => await _context.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);

    public async Task<Book?> GetWithCategoryAsync(int bookId)
        => await _context.Books.Include(b => b.Category).FirstOrDefaultAsync(b => b.BookId == bookId);

    public async Task<bool> HasActiveBorrowsAsync(int bookId)
        => await _context.BorrowRecords.AnyAsync(br => br.BookId == bookId && br.Status != "Returned");
}