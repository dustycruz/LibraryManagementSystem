using FluentValidation;
using LibraryManagementSystem.DTOs.Books;

namespace LibraryAPI.Validators;

public class CreateBookValidator : AbstractValidator<CreateBookDto>
{
    public CreateBookValidator()
    {
        RuleFor(x => x.ISBN).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Author).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Valid category is required.");
        RuleFor(x => x.TotalCopies).GreaterThan(0).WithMessage("Must have at least 1 copy.");
        RuleFor(x => x.PublishedYear).InclusiveBetween(1000, DateTime.Now.Year + 1)
            .When(x => x.PublishedYear.HasValue);
    }
}