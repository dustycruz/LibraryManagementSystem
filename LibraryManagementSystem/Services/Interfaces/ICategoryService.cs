using LibraryManagementSystem.DTOs.Categories;

namespace LibraryManagementSystem.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<(CategoryDto? Result, string? Error)> CreateAsync(CreateCategoryDto dto);
        Task<(CategoryDto? Result, string? Error)> UpdateAsync(int id, CreateCategoryDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
