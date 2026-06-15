
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
    private readonly ILogger<BorrowController> _logger;

    public BorrowController(IBorrowService borrowService, ILogger<BorrowController> logger)
    {
        _borrowService = borrowService;
        _logger = logger;
    }

    // POST api/borrow/borrow
    [HttpPost("borrow")]
    public async Task<IActionResult> BorrowBook([FromBody] BorrowRequestDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        _logger.LogInformation("BorrowBook called by userId={UserId}, bookId={BookId}", userId, dto.BookId);

        var (result, error) = await _borrowService.BorrowBookAsync(userId, dto);
        if (error != null)
            return BadRequest(ApiResponse<object>.ErrorResponse(error));

        return Ok(ApiResponse<BorrowDto>.SuccessResponse(result!, "Book borrowed successfully."));
    }

    // POST api/borrow/return/5
    [HttpPost("return/{borrowId:int}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> ReturnBook(int borrowId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        _logger.LogInformation("ReturnBook called by userId={UserId}, borrowId={BorrowId}", userId, borrowId);

        var (result, error) = await _borrowService.ReturnBookAsync(userId, borrowId);
        if (error != null)
            return BadRequest(ApiResponse<object>.ErrorResponse(error));

        return Ok(ApiResponse<BorrowDto>.SuccessResponse(result!, "Book returned successfully."));
    }

    // GET api/borrow/all
    [HttpGet("all")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> GetAllBorrows(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        _logger.LogInformation("GetAllBorrows: status={Status}, page={Page}", status, page);
        var result = await _borrowService.GetAllBorrowsAsync(status, page, size);
        return Ok(ApiResponse<PagedResponse<BorrowDto>>.SuccessResponse(result));
    }

    // GET api/borrow/my-history
    [HttpGet("my-history")]
    public async Task<IActionResult> GetMyHistory()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        _logger.LogInformation("GetMyHistory for userId={UserId}", userId);
        var history = await _borrowService.GetUserBorrowHistoryAsync(userId);
        return Ok(ApiResponse<List<BorrowDto>>.SuccessResponse(history));
    }

    // GET api/borrow/user/5/history
    [HttpGet("user/{userId:int}/history")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> GetUserHistory(int userId)
    {
        _logger.LogInformation("GetUserHistory for userId={UserId}", userId);
        var history = await _borrowService.GetUserBorrowHistoryAsync(userId);
        return Ok(ApiResponse<List<BorrowDto>>.SuccessResponse(history));
    }

    // GET api/borrow/5
    [HttpGet("{borrowId:int}")]
    public async Task<IActionResult> GetBorrowById(int borrowId)
    {
        var borrow = await _borrowService.GetBorrowByIdAsync(borrowId);
        if (borrow == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Borrow record not found.", 404));
        return Ok(ApiResponse<BorrowDto>.SuccessResponse(borrow));
    }

    // GET api/borrow/overdue
    [HttpGet("overdue")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> GetOverdue()
    {
        await _borrowService.UpdateOverdueStatusAsync();
        var result = await _borrowService.GetAllBorrowsAsync("Overdue", 1, 100);
        return Ok(ApiResponse<PagedResponse<BorrowDto>>.SuccessResponse(result));
    }
}