namespace LibraryManagementSystem.DTOs.Books
{
    public class CreateBookDto
    {
        public string ISBN { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Publisher { get; set; }
        public int? PublishedYear { get; set; }
        public int CategoryId { get; set; }
        public string? Description { get; set; }
        public int TotalCopies { get; set; } = 1;
        public string? CoverImageUrl { get; set; }
    }
}
