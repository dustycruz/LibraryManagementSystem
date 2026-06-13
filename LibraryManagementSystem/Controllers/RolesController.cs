using LibraryAPI.Helpers;

using LibraryManagementSystem.Models;
using LibraryManagementSystem.Ropositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
    private readonly IGenericRepository<Role> _roleRepo;

    public RolesController(IGenericRepository<Role> roleRepo)
    {
        _roleRepo = roleRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _roleRepo.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<Role>>.SuccessResponse(roles));
    }
}