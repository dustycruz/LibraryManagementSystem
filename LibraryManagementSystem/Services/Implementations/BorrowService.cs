using AutoMapper;
using LibraryAPI.Helpers;
using LibraryManagementSystem.DTOs.Borrow;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Ropositories.Interfaces;
using LibraryManagementSystem.Services.Interfaces;

namespace LibraryAPI.Services.Implementations;

public class BorrowService : IBorrowService
{
    private readonly IBorrowRepository _borrowRepo;
    private readonly IBookRepository _bookRepo;
    private readonly IUserRepository _userRepo;
    private readonly IFineRepository _fineRepo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IMapper _mapper;
    private const decimal FinePerDay = 5.00m;
    private const int MaxBorrowLimit = 3;

    public BorrowService(IBorrowRepository borrowRepo, IBookRepository bookRepo,
        IUserRepository userRepo, IFineRepository fineRepo,
        IAuditLogRepository auditRepo, IMapper mapper)
    {
        _borrowRepo = borrowRepo;
        _bookRepo = bookRepo;
        _userRepo = userRepo;
        _fineRepo = fineRepo;
        _auditRepo = auditRepo;
        _mapper = mapper;
    }

    public async Task<(BorrowDto? Result, string? Error)> BorrowBookAsync(int requestingUserId, BorrowRequestDto dto)
    {
        // Determine target user (admin/librarian can borrow on behalf)
        var targetUserId = dto.UserId ?? requestingUserId;
        var user = await _userRepo.GetWithRolesAsync(targetUserId);
        if (user == null || !user.IsActive)
            return (null, "User not found or inactive.");

        // Check borrow limit
        var activeCount = await _borrowRepo.GetActiveBorrowCountAsync(targetUserId);
        if (activeCount >= MaxBorrowLimit)
            return (null, $"Member has reached the maximum borrow limit of {MaxBorrowLimit} books.");

        // Check book availability
        var book = await _bookRepo.GetByIdAsync(dto.BookId);
        if (book == null || !book.IsActive)
            return (null, "Book not found or inactive.");
        if (book.AvailableCopies <= 0)
            return (null, "No copies available for borrowing.");

        // Check unpaid fines
        var unpaidFines = await _fineRepo.GetUnpaidFinesByUserAsync(targetUserId);
        if (unpaidFines.Any())
            return (null, $"Member has unpaid fines totaling ₱{unpaidFines.Sum(f => f.Amount):F2}. Please settle fines first.");

        // Create borrow record
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
        await _borrowRepo.SaveChangesAsync();

        await _auditRepo.LogAsync(requestingUserId, "BORROW", "BorrowRecord", record.BorrowId,
            null, $"User: {targetUserId}, Book: {dto.BookId}, Due: {record.DueDate:yyyy-MM-dd}");

        var detail = await _borrowRepo.GetBorrowWithDetailsAsync(record.BorrowId);
        return (_mapper.Map<BorrowDto>(detail), null);
    }

    public async Task<(BorrowDto? Result, string? Error)> ReturnBookAsync(int processedByUserId, int borrowId)
    {
        var record = await _borrowRepo.GetBorrowWithDetailsAsync(borrowId);
        if (record == null) return (null, "Borrow record not found.");
        if (record.Status == "Returned") return (null, "This book has already been returned.");

        record.ReturnDate = DateTime.UtcNow;
        record.Status = "Returned";
        record.ProcessedByUserId = processedByUserId;

        // Calculate fine
        decimal fineAmount = 0;
        if (DateTime.UtcNow > record.DueDate)
        {
            var daysOverdue = (int)(DateTime.UtcNow - record.DueDate).TotalDays;
            fineAmount = daysOverdue * FinePerDay;

            var fine = new Fine
            {
                BorrowId = borrowId,
                Amount = fineAmount,
                IsPaid = false,
                CreatedAt = DateTime.UtcNow
            };
            await _fineRepo.AddAsync(fine);
        }

        // Increment available copies
        var book = await _bookRepo.GetByIdAsync(record.BookId);
        if (book != null)
        {
            book.AvailableCopies++;
            _bookRepo.Update(book);
        }

        _borrowRepo.Update(record);
        await _borrowRepo.SaveChangesAsync();

        await _auditRepo.LogAsync(processedByUserId, "RETURN", "BorrowRecord", borrowId,
            "Status: Borrowed", $"Status: Returned, Fine: ₱{fineAmount:F2}");

        var updated = await _borrowRepo.GetBorrowWithDetailsAsync(borrowId);
        return (_mapper.Map<BorrowDto>(updated), null);
    }

    public async Task<PagedResponse<BorrowDto>> GetAllBorrowsAsync(string? status, int page, int size)
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