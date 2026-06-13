
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Ropositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Repositories.Implementations;

public class FineRepository : GenericRepository<Fine>, IFineRepository
{
    public FineRepository(LibraryDbContext context) : base(context) { }

    public async Task<Fine?> GetByBorrowIdAsync(int borrowId)
        => await _context.Fines.FirstOrDefaultAsync(f => f.BorrowId == borrowId);

    public async Task<List<Fine>> GetUnpaidFinesByUserAsync(int userId)
        => await _context.Fines
            .Include(f => f.BorrowRecord)
            .Where(f => f.BorrowRecord.UserId == userId && !f.IsPaid)
            .ToListAsync();
}