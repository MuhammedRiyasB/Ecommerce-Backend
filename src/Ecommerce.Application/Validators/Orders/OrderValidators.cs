using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Domain.Enums;
using FluentValidation;

namespace Ecommerce.Application.Validators.Orders
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequestDto>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.AddressId).NotEmpty().WithMessage("AddressId is required");
            RuleFor(x => x.TransactionId).NotEmpty().MaximumLength(100)
                .WithMessage("TransactionId is required and must be under 100 characters");
        }
    }

    public class ChangeOrderStatusRequestValidator : AbstractValidator<ChangeOrderStatusRequestDto>
    {
        public ChangeOrderStatusRequestValidator()
        {
            RuleFor(x => x.Status).NotEmpty()
                .Must(s => Enum.TryParse<OrderStatus>(s, true, out _))
                .WithMessage("Status must be one of: Pending, Processing, Shipped, Delivered, Cancelled, Returned");
        }
    }
}
