namespace LibraryManagementSystem.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime Expiry { get; set; }
    }
}
