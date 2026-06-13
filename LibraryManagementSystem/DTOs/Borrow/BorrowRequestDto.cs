namespace LibraryManagementSystem.DTOs.Borrow
{
    public class BorrowRequestDto
    {
        public int BookId { get; set; }
        public int? UserId { get; set; } // Admin/Librarian can borrow on behalf of member
        public int BorrowDays { get; set; } = 14;
    }
}
