namespace LibraryManagementSystem.DTOs.Categories
{
    public class CreateCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
