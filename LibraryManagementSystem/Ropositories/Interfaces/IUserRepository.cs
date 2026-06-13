using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Ropositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetWithRolesAsync(int userId);
        Task<List<User>> GetAllWithRolesAsync();
        Task<bool> EmailExistsAsync(string email);
    }
}
