using Ecommerce.Application.DTOs.Payment;
using Ecommerce.Domain.Common;

namespace Ecommerce.Application.Interfaces.Payment
{
    /// <summary>
    /// Abstracts payment gateway operations (creation and verification).
    /// </summary>
    public interface IPaymentGatewayService
    {
        Task<ApiResponse<string>> CreatePaymentOrderAsync(int price);
        Task<ApiResponse<bool>> VerifyPaymentAsync(RazorPayVerificationDto payment);
    }
}
