using Ecommerce.Application.DTOs.Cart;
using FluentValidation;

namespace Ecommerce.Application.Validators.Cart
{
    public class AddToCartRequestValidator : AbstractValidator<AddToCartRequestDto>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AddToCartRequestValidator"/> and registers validation rules for <c>AddToCartRequestDto</c>.
        /// </summary>
        /// <remarks>
        /// Enforced rules:
        /// - <c>ProductId</c> must not be empty (message: "ProductId is required").
        /// - <c>Quantity</c> must be between 1 and 10 inclusive (message: "Quantity must be between 1 and 10").
        /// </remarks>
        public AddToCartRequestValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required");
            RuleFor(x => x.Quantity).InclusiveBetween(1, 10).WithMessage("Quantity must be between 1 and 10");
        }
    }
}
