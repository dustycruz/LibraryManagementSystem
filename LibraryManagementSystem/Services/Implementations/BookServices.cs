using AutoMapper;
using LibraryAPI.Helpers;
using LibraryAPI.Services.Interfaces;
using LibraryManagementSystem.DTOs.Books;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Ropositories.Interfaces;

namespace LibraryAPI.Services.Implementations;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IMapper _mapper;

    public BookService(IBookRepository bookRepo, IAuditLogRepository auditRepo, IMapper mapper)
    {
        _bookRepo = bookRepo;
        _auditRepo = auditRepo;
        _mapper = mapper;
    }

    public async Task<PagedResponse<BookDto>> GetBooksAsync(string? search, int? categoryId, bool? isActive, int page, int size)
    {
        return new PagedResponse<BookDto>
        {
            Data = new List<BookDto>(),
            PageNumber = page,
            PageSize = size,
            TotalRecords = 0
        };
    }

    public async Task<BookDto?> GetByIdAsync(int id)
    {
        var book = await _bookRepo.GetWithCategoryAsync(id);
        return book == null ? null : _mapper.Map<BookDto>(book);
    }

    public async Task<BookDto> CreateAsync(CreateBookDto dto, int createdByUserId)
    {
        var book = _mapper.Map<Book>(dto);
        book.CreatedAt = DateTime.UtcNow;
        await _bookRepo.AddAsync(book);
        await _bookRepo.SaveChangesAsync();

        // Reload with category
        var created = await _bookRepo.GetWithCategoryAsync(book.BookId);
        await _auditRepo.LogAsync(createdByUserId, "CREATE", "Book", book.BookId,
            null, $"Title: {book.Title}, ISBN: {book.ISBN}");

        return _mapper.Map<BookDto>(created!);
    }

    public async Task<BookDto?> UpdateAsync(int id, UpdateBookDto dto, int updatedByUserId)
    {
        var book = await _bookRepo.GetWithCategoryAsync(id);
        if (book == null) return null;

        var oldValues = $"Title: {book.Title}, Author: {book.Author}";
        _mapper.Map(dto, book);
        book.UpdatedAt = DateTime.UtcNow;

        _bookRepo.Update(book);
        await _bookRepo.SaveChangesAsync();

        var updated = await _bookRepo.GetWithCategoryAsync(id);
        await _auditRepo.LogAsync(updatedByUserId, "UPDATE", "Book", id,
            oldValues, $"Title: {dto.Title}, Author: {dto.Author}");

        return _mapper.Map<BookDto>(updated!);
    }

    public async Task<bool> DeleteAsync(int id, int deletedByUserId)
    {
        var book = await _bookRepo.GetByIdAsync(id);
        if (book == null) return false;

        if (await _bookRepo.HasActiveBorrowsAsync(id))
            throw new InvalidOperationException("Cannot delete a book with active borrows.");

        book.IsActive = false;
        book.UpdatedAt = DateTime.UtcNow;
        _bookRepo.Update(book);
        await _bookRepo.SaveChangesAsync();

        await _auditRepo.LogAsync(deletedByUserId, "DELETE", "Book", id, $"Title: {book.Title}");
        return true;
    }
}