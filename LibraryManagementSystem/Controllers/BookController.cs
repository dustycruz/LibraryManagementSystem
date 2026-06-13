
using LibraryAPI.Helpers;
using LibraryAPI.Services.Interfaces;
using LibraryManagementSystem.DTOs.Books;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] bool? isActive = true,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        var result = await _bookService.GetBooksAsync(search, categoryId, isActive, page, size);
        return Ok(ApiResponse<PagedResponse<BookDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBook(int id)
    {
        var book = await _bookService.GetByIdAsync(id);
        if (book == null) return NotFound(ApiResponse<object>.ErrorResponse("Book not found.", 404));
        return Ok(ApiResponse<BookDto>.SuccessResponse(book));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> CreateBook([FromBody] CreateBookDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var book = await _bookService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetBook), new { id = book.BookId },
            ApiResponse<BookDto>.SuccessResponse(book, "Book created successfully.", 201));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var book = await _bookService.UpdateAsync(id, dto, userId);
        if (book == null) return NotFound(ApiResponse<object>.ErrorResponse("Book not found.", 404));
        return Ok(ApiResponse<BookDto>.SuccessResponse(book, "Book updated successfully."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var deleted = await _bookService.DeleteAsync(id, userId);
        if (!deleted) return NotFound(ApiResponse<object>.ErrorResponse("Book not found.", 404));
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Book deleted successfully."));
    }
}