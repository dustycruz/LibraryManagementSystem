using AutoMapper;
using LibraryAPI.Services.Interfaces;
using LibraryManagementSystem.DTOs.Categories;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Ropositories.Interfaces;
using LibraryManagementSystem.Services.Interfaces;

namespace LibraryAPI.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository categoryRepo, IMapper mapper)
    {
        _categoryRepo = categoryRepo;
        _mapper = mapper;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var cats = await _categoryRepo.GetAllWithBookCountAsync();
        return _mapper.Map<List<CategoryDto>>(cats);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var cat = await _categoryRepo.GetByIdAsync(id);
        return cat == null ? null : _mapper.Map<CategoryDto>(cat);
    }

    public async Task<(CategoryDto? Result, string? Error)> CreateAsync(CreateCategoryDto dto)
    {
        if (await _categoryRepo.NameExistsAsync(dto.CategoryName))
            return (null, "Category name already exists.");

        var cat = _mapper.Map<Category>(dto);
        await _categoryRepo.AddAsync(cat);
        await _categoryRepo.SaveChangesAsync();
        return (_mapper.Map<CategoryDto>(cat), null);
    }

    public async Task<(CategoryDto? Result, string? Error)> UpdateAsync(int id, CreateCategoryDto dto)
    {
        var cat = await _categoryRepo.GetByIdAsync(id);
        if (cat == null) return (null, "Category not found.");

        cat.CategoryName = dto.CategoryName;
        cat.Description = dto.Description;
        _categoryRepo.Update(cat);
        await _categoryRepo.SaveChangesAsync();
        return (_mapper.Map<CategoryDto>(cat), null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var cat = await _categoryRepo.GetByIdAsync(id);
        if (cat == null) return false;
        _categoryRepo.Remove(cat);
        await _categoryRepo.SaveChangesAsync();
        return true;
    }
}