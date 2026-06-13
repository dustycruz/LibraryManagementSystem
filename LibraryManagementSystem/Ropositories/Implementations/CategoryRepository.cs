
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Ropositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Repositories.Implementations;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(LibraryDbContext context) : base(context) { }

    public async Task<List<Category>> GetAllWithBookCountAsync()
        => await _context.Categories.Include(c => c.Books).OrderBy(c => c.CategoryName).ToListAsync();

    public async Task<bool> NameExistsAsync(string name)
        => await _context.Categories.AnyAsync(c => c.CategoryName == name);
}