using Ecommerce.Application.DTOs.Catalog;
using FluentValidation;

namespace Ecommerce.Application.Validators.Catalog
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequestDto>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Brand).NotEmpty().MaximumLength(100)
                .WithMessage("Brand must be between 1 and 100 characters");
            RuleFor(x => x.Quantity).InclusiveBetween(1, 100)
                .WithMessage("Quantity must be between 1 and 100");
            RuleFor(x => x.Price).InclusiveBetween(1, 500000)
                .WithMessage("Price must be between 1 and 500000");
            RuleFor(x => x.Discount).InclusiveBetween(0, 100)
                .WithMessage("Discount must be between 0 and 100");
            RuleFor(x => x.Description).NotEmpty().MaximumLength(500)
                .WithMessage("Description must be between 1 and 500 characters");
            RuleFor(x => x.CategoryName).NotEmpty().MaximumLength(50);
        }
    }
}
