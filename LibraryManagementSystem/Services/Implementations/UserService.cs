using AutoMapper;
using LibraryManagementSystem.DTOs.Users;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Ropositories.Interfaces;
using LibraryManagementSystem.Services.Interfaces;

namespace LibraryAPI.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly IGenericRepository<UserRole> _userRoleRepo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepo, IGenericRepository<UserRole> userRoleRepo,
        IAuditLogRepository auditRepo, IMapper mapper)
    {
        _userRepo = userRepo;
        _userRoleRepo = userRoleRepo;
        _auditRepo = auditRepo;
        _mapper = mapper;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepo.GetAllWithRolesAsync();
        return _mapper.Map<List<UserDto>>(users);
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _userRepo.GetWithRolesAsync(id);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }

    // ← REPLACE THIS ENTIRE METHOD with the new code below
    public async Task<(UserDto? Result, string? Error)> UpdateUserAsync(int id, UpdateUserDto dto, int updatedByUserId)
    {
        var user = await _userRepo.GetWithRolesAsync(id);
        if (user == null) return (null, "User not found.");

        // Validate role IDs exist
        if (dto.RoleIds.Any(rid => rid <= 0))
            return (null, "Invalid role IDs provided.");

        var oldRoles = user.UserRoles.Select(ur => ur.RoleId).ToList();
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.IsActive = dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        // Remove old roles
        var existingRoles = user.UserRoles.ToList();
        foreach (var ur in existingRoles)
            _userRoleRepo.Remove(ur);
        await _userRepo.SaveChangesAsync();  // ← Save after removal

        // Add new roles
        foreach (var roleId in dto.RoleIds)
            await _userRoleRepo.AddAsync(new UserRole { UserId = id, RoleId = roleId });
        await _userRepo.SaveChangesAsync();  // ← Save after adding

        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();  // ← Final save for user updates

        await _auditRepo.LogAsync(updatedByUserId, "UPDATE", "User", id,
            $"Roles: {string.Join(",", oldRoles)}", $"Roles: {string.Join(",", dto.RoleIds)}");

        var updated = await _userRepo.GetWithRolesAsync(id);
        return (_mapper.Map<UserDto>(updated), null);
    }

    public async Task<bool> DeactivateUserAsync(int id, int adminUserId)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return false;
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();
        await _auditRepo.LogAsync(adminUserId, "DEACTIVATE", "User", id);
        return true;
    }
}