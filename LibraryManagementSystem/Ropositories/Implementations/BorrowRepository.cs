
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Ropositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Repositories.Implementations;

public class BorrowRepository : GenericRepository<BorrowRecord>, IBorrowRepository
{
    public BorrowRepository(LibraryDbContext context) : base(context) { }

    public async Task<List<BorrowRecord>> GetUserBorrowsAsync(int userId)
        => await _context.BorrowRecords
            .Include(br => br.Book).ThenInclude(b => b.Category)
            .Include(br => br.Fine)
            .Where(br => br.UserId == userId)
            .OrderByDescending(br => br.BorrowDate)
            .ToListAsync();

    public async Task<List<BorrowRecord>> GetAllBorrowsPagedAsync(string? status, int pageNumber, int pageSize)
    {
        var query = _context.BorrowRecords
            .Include(br => br.User)
            .Include(br => br.Book)
            .Include(br => br.Fine)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(br => br.Status == status);

        return await query
            .OrderByDescending(br => br.BorrowDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetAllBorrowsCountAsync(string? status)
    {
        var query = _context.BorrowRecords.AsQueryable();
        if (!string.IsNullOrEmpty(status))
            query = query.Where(br => br.Status == status);
        return await query.CountAsync();
    }

    public async Task<BorrowRecord?> GetBorrowWithDetailsAsync(int borrowId)
        => await _context.BorrowRecords
            .Include(br => br.User)
            .Include(br => br.Book)
            .Include(br => br.Fine)
            .FirstOrDefaultAsync(br => br.BorrowId == borrowId);

    public async Task<int> GetActiveBorrowCountAsync(int userId)
        => await _context.BorrowRecords
            .CountAsync(br => br.UserId == userId && br.Status != "Returned");

    public async Task<List<BorrowRecord>> GetOverdueBorrowsAsync()
        => await _context.BorrowRecords
            .Include(br => br.User)
            .Include(br => br.Book)
            .Include(br => br.Fine)
            .Where(br => br.DueDate < DateTime.UtcNow && br.Status != "Returned")
            .ToListAsync();
}