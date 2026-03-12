using Ecommerce.Application.DTOs.Payment;
using FluentValidation;

namespace Ecommerce.Application.Validators.Payment
{
    public class RazorPayVerificationValidator : AbstractValidator<RazorPayVerificationDto>
    {
        public RazorPayVerificationValidator()
        {
            RuleFor(x => x.razorpay_payment_id).NotEmpty();
            RuleFor(x => x.razorpay_order_id).NotEmpty();
            RuleFor(x => x.razorpay_signature).NotEmpty();
        }
    }
}
