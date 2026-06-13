
using LibraryAPI.Helpers;
using LibraryAPI.Services.Interfaces;
using LibraryManagementSystem.DTOs.Reports;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Librarian")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("dashboard")]
    [AllowAnonymous]
    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        var stats = await _reportService.GetDashboardStatsAsync();
        return Ok(ApiResponse<DashboardStatsDto>.SuccessResponse(stats));
    }

    [HttpGet("available-books")]
    public async Task<IActionResult> AvailableBooks()
    {
        var data = await _reportService.GetAvailableBooksReportAsync();
        return Ok(ApiResponse<List<BookReportDto>>.SuccessResponse(data));
    }

    [HttpGet("most-borrowed")]
    public async Task<IActionResult> MostBorrowed([FromQuery] int top = 10)
    {
        var data = await _reportService.GetMostBorrowedBooksReportAsync(top);
        return Ok(ApiResponse<List<BookReportDto>>.SuccessResponse(data));
    }

    [HttpGet("overdue")]
    public async Task<IActionResult> Overdue()
    {
        var data = await _reportService.GetOverdueBooksReportAsync();
        return Ok(ApiResponse<List<OverdueReportDto>>.SuccessResponse(data));
    }

    [HttpGet("user-activity")]
    public async Task<IActionResult> UserActivity()
    {
        var data = await _reportService.GetUserActivityReportAsync();
        return Ok(ApiResponse<List<UserActivityReportDto>>.SuccessResponse(data));
    }
}