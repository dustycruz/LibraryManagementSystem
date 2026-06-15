// BorrowService.cs
using AutoMapper;
using LibraryAPI.Helpers;
using LibraryAPI.Services.Interfaces;
using LibraryManagementSystem.DTOs.Borrow;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Ropositories.Interfaces;

namespace LibraryAPI.Services.Implementations;

public class BorrowService : IBorrowService
{
    private readonly IBorrowRepository _borrowRepo;
    private readonly IBookRepository _bookRepo;
    private readonly IUserRepository _userRepo;
    private readonly IFineRepository _fineRepo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<BorrowService> _logger;

    private const decimal FinePerDay = 5.00m;
    private const int MaxBorrowLimit = 3;

    public BorrowService(
        IBorrowRepository borrowRepo,
        IBookRepository bookRepo,
        IUserRepository userRepo,
        IFineRepository fineRepo,
        IAuditLogRepository auditRepo,
        IMapper mapper,
        ILogger<BorrowService> logger)
    {
        _borrowRepo = borrowRepo;
        _bookRepo = bookRepo;
        _userRepo = userRepo;
        _fineRepo = fineRepo;
        _auditRepo = auditRepo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<(BorrowDto? Result, string? Error)> BorrowBookAsync(
        int requestingUserId, BorrowRequestDto dto)
    {
        var targetUserId = dto.UserId ?? requestingUserId;

        var user = await _userRepo.GetWithRolesAsync(targetUserId);
        if (user == null || !user.IsActive)
            return (null, "User not found or inactive.");

        var activeCount = await _borrowRepo.GetActiveBorrowCountAsync(targetUserId);
        if (activeCount >= MaxBorrowLimit)
            return (null, $"Member has reached the maximum borrow limit of {MaxBorrowLimit} books.");

        var book = await _bookRepo.GetByIdAsync(dto.BookId);
        if (book == null || !book.IsActive)
            return (null, "Book not found or inactive.");

        if (book.AvailableCopies <= 0)
            return (null, "No copies available for borrowing.");

        var unpaidFines = await _fineRepo.GetUnpaidFinesByUserAsync(targetUserId);
        if (unpaidFines.Any())
            return (null, $"Member has unpaid fines totaling ₱{unpaidFines.Sum(f => f.Amount):F2}. Please settle fines first.");

        var borrowDays = dto.BorrowDays > 0 ? dto.BorrowDays : 14;
        var record = new BorrowRecord
        {
            UserId = targetUserId,
            BookId = dto.BookId,
            BorrowDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(borrowDays),
            Status = "Borrowed"
        };

        await _borrowRepo.AddAsync(record);
        book.AvailableCopies--;
        _bookRepo.Update(book);
        await _borrowRepo.SaveChangesAsync(); // ← This saves BOTH BorrowRecord and Book changes

        await _auditRepo.LogAsync(requestingUserId, "BORROW", "BorrowRecord", record.BorrowId,
            null, $"UserId:{targetUserId}, BookId:{dto.BookId}, Due:{record.DueDate:yyyy-MM-dd}");

        var detail = await _borrowRepo.GetBorrowWithDetailsAsync(record.BorrowId);
        return (_mapper.Map<BorrowDto>(detail), null);
    }

    public async Task<(BorrowDto? Result, string? Error)> ReturnBookAsync(
        int processedByUserId, int borrowId)
    {
        _logger.LogInformation("ReturnBookAsync START: borrowId={BorrowId}, processedBy={UserId}",
            borrowId, processedByUserId);

        var record = await _borrowRepo.GetBorrowWithDetailsAsync(borrowId);

        if (record == null)
        {
            _logger.LogWarning("ReturnBookAsync: BorrowRecord {BorrowId} not found", borrowId);
            return (null, "Borrow record not found.");
        }

        if (record.Status == "Returned")
        {
            _logger.LogWarning("ReturnBookAsync: BorrowRecord {BorrowId} already returned", borrowId);
            return (null, "This book has already been returned.");
        }

        _logger.LogInformation("ReturnBookAsync: Found record, current status={Status}", record.Status);

        // Step 1 — update the borrow record fields
        record.ReturnDate = DateTime.UtcNow;
        record.Status = "Returned";
        record.ProcessedByUserId = processedByUserId;
        _borrowRepo.Update(record);

        // Step 2 — calculate and create fine if overdue
        decimal fineAmount = 0;
        if (DateTime.UtcNow > record.DueDate)
        {
            var daysOverdue = (int)(DateTime.UtcNow - record.DueDate).TotalDays;
            fineAmount = daysOverdue * FinePerDay;

            var existingFine = await _fineRepo.GetByBorrowIdAsync(borrowId);
            if (existingFine == null)
            {
                var fine = new Fine
                {
                    BorrowId = borrowId,
                    Amount = fineAmount,
                    IsPaid = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _fineRepo.AddAsync(fine);
                _logger.LogInformation("Fine created: ₱{Amount} for borrowId={BorrowId}", fineAmount, borrowId);
            }
        }

        // Step 3 — increment available copies
        var book = await _bookRepo.GetByIdAsync(record.BookId);
        if (book != null)
        {
            book.AvailableCopies++;
            _bookRepo.Update(book);
            _logger.LogInformation("Book {BookId} AvailableCopies incremented to {Count}",
                book.BookId, book.AvailableCopies);
        }

        // Step 4 — single SaveChangesAsync saves ALL changes across all repos
        // This works because all repos share the same DbContext (Scoped DI)
        try
        {
            await _borrowRepo.SaveChangesAsync();
            _logger.LogInformation("ReturnBookAsync: SaveChangesAsync SUCCESS for borrowId={BorrowId}", borrowId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ReturnBookAsync: SaveChangesAsync FAILED for borrowId={BorrowId}", borrowId);
            return (null, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
        }

        // Step 5 — audit log (has its own SaveChangesAsync internally)
        await _auditRepo.LogAsync(processedByUserId, "RETURN", "BorrowRecord", borrowId,
            "Status:Borrowed", $"Status:Returned, Fine:₱{fineAmount:F2}");

        var updated = await _borrowRepo.GetBorrowWithDetailsAsync(borrowId);
        _logger.LogInformation("ReturnBookAsync END: borrowId={BorrowId}, newStatus={Status}",
            borrowId, updated?.Status);

        return (_mapper.Map<BorrowDto>(updated), null);
    }

    public async Task<PagedResponse<BorrowDto>> GetAllBorrowsAsync(
        string? status, int page, int size)
    {
        var borrows = await _borrowRepo.GetAllBorrowsPagedAsync(status, page, size);
        var total = await _borrowRepo.GetAllBorrowsCountAsync(status);
        return new PagedResponse<BorrowDto>
        {
            Data = _mapper.Map<List<BorrowDto>>(borrows),
            PageNumber = page,
            PageSize = size,
            TotalRecords = total
        };
    }

    public async Task<List<BorrowDto>> GetUserBorrowHistoryAsync(int userId)
    {
        var borrows = await _borrowRepo.GetUserBorrowsAsync(userId);
        return _mapper.Map<List<BorrowDto>>(borrows);
    }

    public async Task<BorrowDto?> GetBorrowByIdAsync(int borrowId)
    {
        var record = await _borrowRepo.GetBorrowWithDetailsAsync(borrowId);
        return record == null ? null : _mapper.Map<BorrowDto>(record);
    }

    public async Task UpdateOverdueStatusAsync()
    {
        var overdue = await _borrowRepo.FindAsync(br =>
            br.DueDate < DateTime.UtcNow && br.Status == "Borrowed");
        foreach (var br in overdue)
        {
            br.Status = "Overdue";
            _borrowRepo.Update(br);
        }
        await _borrowRepo.SaveChangesAsync();
    }
}