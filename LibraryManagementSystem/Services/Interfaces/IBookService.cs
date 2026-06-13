
using LibraryAPI.Helpers;
using LibraryManagementSystem.DTOs.Books;

namespace LibraryAPI.Services.Interfaces;

public interface IBookService
{
    Task<PagedResponse<BookDto>> GetBooksAsync(string? search, int? categoryId, bool? isActive, int page, int size);
    Task<BookDto?> GetByIdAsync(int id);
    Task<BookDto> CreateAsync(CreateBookDto dto, int createdByUserId);
    Task<BookDto?> UpdateAsync(int id, UpdateBookDto dto, int updatedByUserId);
    Task<bool> DeleteAsync(int id, int deletedByUserId);
}