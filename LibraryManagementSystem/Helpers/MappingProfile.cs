using AutoMapper;
using LibraryManagementSystem.DTOs.Books;
using LibraryManagementSystem.DTOs.Borrow;
using LibraryManagementSystem.DTOs.Categories;
using LibraryManagementSystem.DTOs.Users;
using LibraryManagementSystem.Models;

namespace LibraryAPI.Helpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Book
        CreateMap<Book, BookDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.CategoryName));
        CreateMap<CreateBookDto, Book>()
            .ForMember(d => d.AvailableCopies, o => o.MapFrom(s => s.TotalCopies));
        CreateMap<UpdateBookDto, Book>();

        // Category
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.BookCount, o => o.MapFrom(s => s.Books.Count(b => b.IsActive)));
        CreateMap<CreateCategoryDto, Category>();

        // User
        CreateMap<User, UserDto>()
            .ForMember(d => d.Roles, o => o.MapFrom(s => s.UserRoles.Select(ur => ur.Role.RoleName).ToList()));

        // Borrow
        CreateMap<BorrowRecord, BorrowDto>()
            .ForMember(d => d.MemberName, o => o.MapFrom(s => s.User.FirstName + " " + s.User.LastName))
            .ForMember(d => d.MemberEmail, o => o.MapFrom(s => s.User.Email))
            .ForMember(d => d.BookTitle, o => o.MapFrom(s => s.Book.Title))
            .ForMember(d => d.BookISBN, o => o.MapFrom(s => s.Book.ISBN))
            .ForMember(d => d.BookAuthor, o => o.MapFrom(s => s.Book.Author))
            .ForMember(d => d.DaysOverdue, o => o.MapFrom(s =>
                s.DueDate < DateTime.UtcNow && s.Status != "Returned"
                    ? (int)(DateTime.UtcNow - s.DueDate).TotalDays
                    : 0))
            .ForMember(d => d.FineAmount, o => o.MapFrom(s => s.Fine != null ? s.Fine.Amount : (decimal?)null))
            .ForMember(d => d.FineIsPaid, o => o.MapFrom(s => s.Fine != null ? s.Fine.IsPaid : (bool?)null));
    }
}