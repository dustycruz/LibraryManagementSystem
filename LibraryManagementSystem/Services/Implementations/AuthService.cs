
using LibraryAPI.Helpers;
using LibraryManagementSystem.DTOs.Auth;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Ropositories.Interfaces;
using LibraryManagementSystem.Services.Interfaces;

namespace LibraryAPI.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IGenericRepository<Role> _roleRepo;
    private readonly IGenericRepository<UserRole> _userRoleRepo;
    private readonly JwtHelper _jwtHelper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepo,
        IGenericRepository<Role> roleRepo,
        IGenericRepository<UserRole> userRoleRepo,
        JwtHelper jwtHelper,
        ILogger<AuthService> logger)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _userRoleRepo = userRoleRepo;
        _jwtHelper = jwtHelper;
        _logger = logger;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Login failed for email: {Email}", dto.Email);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid password for email: {Email}", dto.Email);
            return null;
        }

        var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();
        var token = _jwtHelper.GenerateToken(user, roles);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}",
            UserId = user.UserId,
            Roles = roles,
            Expiry = DateTime.UtcNow.AddHours(24)
        };
    }

    public async Task<bool> RegisterAsync(RegisterDto dto)
    {
        if (await _userRepo.EmailExistsAsync(dto.Email))
            return false;

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        // Assign Member role by default
        var memberRole = (await _roleRepo.FindAsync(r => r.RoleName == "Member")).FirstOrDefault();
        if (memberRole != null)
        {
            var userRole = new UserRole { UserId = user.UserId, RoleId = memberRole.RoleId };
            await _userRoleRepo.AddAsync(userRole);
            await _userRoleRepo.SaveChangesAsync();
        }

        return true;
    }
}