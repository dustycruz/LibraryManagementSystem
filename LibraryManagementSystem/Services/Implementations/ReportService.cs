using LibraryManagementSystem.Data;
using LibraryManagementSystem.DTOs.Reports;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Services.Implementations;

public class ReportService : IReportService
{
    private readonly LibraryDbContext _context;

    public ReportService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var totalBooks = await _context.Books.CountAsync(b => b.IsActive);
        var availableCopies = await _context.Books.Where(b => b.IsActive).SumAsync(b => b.AvailableCopies);
        var activeBorrows = await _context.BorrowRecords.CountAsync(br => br.Status != "Returned");
        var overdue = await _context.BorrowRecords.CountAsync(br => br.DueDate < DateTime.UtcNow && br.Status != "Returned");
        var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
        var unpaidFines = await _context.Fines.Where(f => !f.IsPaid).SumAsync(f => (decimal?)f.Amount) ?? 0;

        return new DashboardStatsDto
        {
            TotalBooks = totalBooks,
            TotalAvailableCopies = availableCopies,
            ActiveBorrows = activeBorrows,
            OverdueCount = overdue,
            TotalActiveUsers = activeUsers,
            TotalUnpaidFines = unpaidFines
        };
    }

    public async Task<List<BookReportDto>> GetAvailableBooksReportAsync()
    {
        return await _context.Books
            .Include(b => b.Category)
            .Where(b => b.IsActive && b.AvailableCopies > 0)
            .Select(b => new BookReportDto
            {
                BookId = b.BookId,
                Title = b.Title,
                Author = b.Author,
                ISBN = b.ISBN,
                CategoryName = b.Category.CategoryName,
                TotalCopies = b.TotalCopies,
                AvailableCopies = b.AvailableCopies,
                BorrowedCopies = b.TotalCopies - b.AvailableCopies,
                TotalBorrows = b.BorrowRecords.Count
            })
            .OrderBy(b => b.Title)
            .ToListAsync();
    }

    public async Task<List<BookReportDto>> GetMostBorrowedBooksReportAsync(int top = 10)
    {
        return await _context.Books
            .Include(b => b.Category)
            .Include(b => b.BorrowRecords)
            .Where(b => b.IsActive)
            .Select(b => new BookReportDto
            {
                BookId = b.BookId,
                Title = b.Title,
                Author = b.Author,
                ISBN = b.ISBN,
                CategoryName = b.Category.CategoryName,
                TotalCopies = b.TotalCopies,
                AvailableCopies = b.AvailableCopies,
                BorrowedCopies = b.TotalCopies - b.AvailableCopies,
                TotalBorrows = b.BorrowRecords.Count
            })
            .OrderByDescending(b => b.TotalBorrows)
            .Take(top)
            .ToListAsync();
    }

    // FIXED: materialize from DB first, then project in memory
    // EF Core cannot translate TimeSpan math or (int) casts to SQL
    public async Task<List<OverdueReportDto>> GetOverdueBooksReportAsync()
    {
        var records = await _context.BorrowRecords
            .Include(br => br.User)
            .Include(br => br.Book)
            .Include(br => br.Fine)
            .Where(br => br.DueDate < DateTime.UtcNow && br.Status != "Returned")
            .ToListAsync();

        return records
            .Select(br =>
            {
                var daysOverdue = (int)(DateTime.UtcNow - br.DueDate).TotalDays;
                return new OverdueReportDto
                {
                    BorrowId = br.BorrowId,
                    MemberName = br.User.FirstName + " " + br.User.LastName,
                    Email = br.User.Email,
                    BookTitle = br.Book.Title,
                    ISBN = br.Book.ISBN,
                    BorrowDate = br.BorrowDate,
                    DueDate = br.DueDate,
                    DaysOverdue = daysOverdue,
                    FineAmount = br.Fine != null
                                     ? br.Fine.Amount
                                     : daysOverdue * 5m,
                    FineIsPaid = br.Fine != null && br.Fine.IsPaid
                };
            })
            .OrderByDescending(r => r.DaysOverdue)
            .ToList();
    }

    // FIXED: materialize from DB first, then project in memory
    // EF Core cannot translate .Sum() on nested navigation properties to SQL
    public async Task<List<UserActivityReportDto>> GetUserActivityReportAsync()
    {
        var users = await _context.Users
            .Include(u => u.BorrowRecords)
                .ThenInclude(br => br.Fine)
            .Where(u => u.IsActive)
            .ToListAsync();

        return users
            .Select(u => new UserActivityReportDto
            {
                UserId = u.UserId,
                MemberName = u.FirstName + " " + u.LastName,
                Email = u.Email,
                TotalBorrows = u.BorrowRecords.Count,
                ActiveBorrows = u.BorrowRecords.Count(br => br.Status != "Returned"),
                ReturnedBooks = u.BorrowRecords.Count(br => br.Status == "Returned"),
                TotalFines = u.BorrowRecords
                                 .Where(br => br.Fine != null)
                                 .Sum(br => br.Fine!.Amount),
                PaidFines = u.BorrowRecords
                                 .Where(br => br.Fine != null && br.Fine.IsPaid)
                                 .Sum(br => br.Fine!.Amount),
                UnpaidFines = u.BorrowRecords
                                 .Where(br => br.Fine != null && !br.Fine.IsPaid)
                                 .Sum(br => br.Fine!.Amount)
            })
            .OrderByDescending(u => u.TotalBorrows)
            .ToList();
    }
}