
using LibraryAPI.Helpers;
using LibraryAPI.Services.Interfaces;
using LibraryManagementSystem.DTOs.Borrow;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BorrowController : ControllerBase
{
    private readonly IBorrowService _borrowService;

    public BorrowController(IBorrowService borrowService)
    {
        _borrowService = borrowService;
    }

    [HttpPost("borrow")]
    public async Task<IActionResult> BorrowBook([FromBody] BorrowRequestDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (result, error) = await _borrowService.BorrowBookAsync(userId, dto);
        if (error != null)
            return BadRequest(ApiResponse<object>.ErrorResponse(error));
        return Ok(ApiResponse<BorrowDto>.SuccessResponse(result!, "Book borrowed successfully."));
    }

    [HttpPost("return/{borrowId}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> ReturnBook(int borrowId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (result, error) = await _borrowService.ReturnBookAsync(userId, borrowId);
        if (error != null)
            return BadRequest(ApiResponse<object>.ErrorResponse(error));
        return Ok(ApiResponse<BorrowDto>.SuccessResponse(result!, "Book returned successfully."));
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> GetAllBorrows(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        var result = await _borrowService.GetAllBorrowsAsync(status, page, size);
        return Ok(ApiResponse<PagedResponse<BorrowDto>>.SuccessResponse(result));
    }

    [HttpGet("my-history")]
    public async Task<IActionResult> GetMyHistory()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var history = await _borrowService.GetUserBorrowHistoryAsync(userId);
        return Ok(ApiResponse<List<BorrowDto>>.SuccessResponse(history));
    }

    [HttpGet("user/{userId}/history")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> GetUserHistory(int userId)
    {
        var history = await _borrowService.GetUserBorrowHistoryAsync(userId);
        return Ok(ApiResponse<List<BorrowDto>>.SuccessResponse(history));
    }

    [HttpGet("{borrowId}")]
    public async Task<IActionResult> GetBorrowById(int borrowId)
    {
        var borrow = await _borrowService.GetBorrowByIdAsync(borrowId);
        if (borrow == null) return NotFound(ApiResponse<object>.ErrorResponse("Borrow record not found.", 404));
        return Ok(ApiResponse<BorrowDto>.SuccessResponse(borrow));
    }
}