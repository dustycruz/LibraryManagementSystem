
using LibraryAPI.Helpers;
using LibraryAPI.Services.Interfaces;
using LibraryManagementSystem.DTOs.Categories;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cats = await _categoryService.GetAllAsync();
        return Ok(ApiResponse<List<CategoryDto>>.SuccessResponse(cats));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var cat = await _categoryService.GetByIdAsync(id);
        if (cat == null) return NotFound(ApiResponse<object>.ErrorResponse("Category not found.", 404));
        return Ok(ApiResponse<CategoryDto>.SuccessResponse(cat));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var (result, error) = await _categoryService.CreateAsync(dto);
        if (error != null) return BadRequest(ApiResponse<object>.ErrorResponse(error));
        return CreatedAtAction(nameof(GetById), new { id = result!.CategoryId },
            ApiResponse<CategoryDto>.SuccessResponse(result!, "Category created.", 201));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDto dto)
    {
        var (result, error) = await _categoryService.UpdateAsync(id, dto);
        if (error != null) return BadRequest(ApiResponse<object>.ErrorResponse(error));
        return Ok(ApiResponse<CategoryDto>.SuccessResponse(result!, "Category updated."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _categoryService.DeleteAsync(id);
        if (!deleted) return NotFound(ApiResponse<object>.ErrorResponse("Category not found.", 404));
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Category deleted."));
    }
}