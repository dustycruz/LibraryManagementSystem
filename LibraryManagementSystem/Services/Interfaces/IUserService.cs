using LibraryManagementSystem.DTOs.Users;

namespace LibraryManagementSystem.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetByIdAsync(int id);
        Task<(UserDto? Result, string? Error)> UpdateUserAsync(int id, UpdateUserDto dto, int updatedByUserId);
        Task<bool> DeactivateUserAsync(int id, int adminUserId);
    }
}
