using LibraryAPI.Helpers;
using LibraryManagementSystem.DTOs.Auth;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Ropositories.Interfaces;
using LibraryManagementSystem.Services.Interfaces;

namespace LibraryAPI.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IGenericRepository<UserRole> _userRoleRepo;
    private readonly IGenericRepository<Role> _roleRepo;
    private readonly JwtHelper _jwtHelper;

    public AuthService(IUserRepository userRepo, IGenericRepository<UserRole> userRoleRepo,
        IGenericRepository<Role> roleRepo, JwtHelper jwtHelper)
    {
        _userRepo = userRepo;
        _userRoleRepo = userRoleRepo;
        _roleRepo = roleRepo;
        _jwtHelper = jwtHelper;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var users = await _userRepo.FindAsync(u => u.Email == dto.Email);
        var user = users.FirstOrDefault();
        if (user == null) return null;

        // Reload with roles
        var userWithRoles = await _userRepo.GetWithRolesAsync(user.UserId);
        if (userWithRoles == null) return null;

        // Verify password using BCrypt
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, userWithRoles.PasswordHash))
            return null;

        var roles = userWithRoles.UserRoles.Select(ur => ur.Role.RoleName).ToList();
        var token = _jwtHelper.GenerateToken(userWithRoles, roles);
        return new AuthResponseDto
        {
            UserId = userWithRoles.UserId,
            Email = userWithRoles.Email,
            FullName = $"{userWithRoles.FirstName} {userWithRoles.LastName}",
            Token = token,
            Roles = roles
        };
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        // Check if email already exists
        var existingUsers = await _userRepo.FindAsync(u => u.Email == dto.Email);
        if (existingUsers.Any()) return null;

        // Create new user
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Hash password using BCrypt
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // Save user
        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        // Assign default "Member" role
        var roles = await _roleRepo.FindAsync(r => r.RoleName == "Member");
        var memberRole = roles.FirstOrDefault();
        if (memberRole != null)
        {
            var userRole = new UserRole { UserId = user.UserId, RoleId = memberRole.RoleId };
            await _userRoleRepo.AddAsync(userRole);
            await _userRoleRepo.SaveChangesAsync();
        }

        // Reload user with roles
        var registeredUser = await _userRepo.GetWithRolesAsync(user.UserId);
        if (registeredUser == null) return null;

        // Generate token
        var roleNames = registeredUser.UserRoles.Select(ur => ur.Role.RoleName).ToList();
        var token = _jwtHelper.GenerateToken(registeredUser, roleNames);
        return new AuthResponseDto
        {
            UserId = registeredUser.UserId,
            Email = registeredUser.Email,
            FullName = $"{registeredUser.FirstName} {registeredUser.LastName}",
            Token = token,
            Roles = roleNames
        };
    }
}