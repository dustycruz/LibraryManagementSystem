using LibraryAPI.Helpers;
using LibraryAPI.Services.Interfaces;
using LibraryManagementSystem.DTOs.Reports;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            var stats = await _reportService.GetDashboardStatsAsync();
            return Ok(ApiResponse<DashboardStatsDto>.SuccessResponse(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard stats");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Failed to load dashboard stats."));
        }
    }

    [HttpGet("available-books")]
    public async Task<IActionResult> AvailableBooks()
    {
        try
        {
            var data = await _reportService.GetAvailableBooksReportAsync();
            return Ok(ApiResponse<List<BookReportDto>>.SuccessResponse(data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading available books report");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Failed to load report."));
        }
    }

    [HttpGet("most-borrowed")]
    public async Task<IActionResult> MostBorrowed([FromQuery] int top = 10)
    {
        try
        {
            var data = await _reportService.GetMostBorrowedBooksReportAsync(top);
            return Ok(ApiResponse<List<BookReportDto>>.SuccessResponse(data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading most borrowed report");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Failed to load report."));
        }
    }

    [HttpGet("user-activity")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> UserActivity()
    {
        try
        {
            var data = await _reportService.GetUserActivityReportAsync();
            return Ok(ApiResponse<List<UserActivityReportDto>>.SuccessResponse(data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user activity report");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Failed to load report."));
        }
    }

    [HttpGet("overdue")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> Overdue()
    {
        try
        {
            var data = await _reportService.GetOverdueBooksReportAsync();
            return Ok(ApiResponse<List<OverdueReportDto>>.SuccessResponse(data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading overdue report");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Failed to load report."));
        }
    }
}