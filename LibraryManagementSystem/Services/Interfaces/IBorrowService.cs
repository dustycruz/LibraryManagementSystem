
using LibraryAPI.Helpers;
using LibraryManagementSystem.DTOs.Borrow;

namespace LibraryAPI.Services.Interfaces;

public interface IBorrowService
{
    Task<(BorrowDto? Result, string? Error)> BorrowBookAsync(int requestingUserId, BorrowRequestDto dto);
    Task<(BorrowDto? Result, string? Error)> ReturnBookAsync(int processedByUserId, int borrowId);
    Task<PagedResponse<BorrowDto>> GetAllBorrowsAsync(string? status, int page, int size);
    Task<List<BorrowDto>> GetUserBorrowHistoryAsync(int userId);
    Task<BorrowDto?> GetBorrowByIdAsync(int borrowId);
    Task UpdateOverdueStatusAsync();
}