using Ecommerce.Application.DTOs.Payment;
using Ecommerce.Application.Interfaces.Payment;
using Ecommerce.Domain.Common;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;

namespace Ecommerce.Infrastructure.Services
{
    /// <summary>
    /// Razorpay implementation of IPaymentGatewayService — infrastructure concern.
    /// </summary>
    public class RazorPayGatewayService : IPaymentGatewayService
    {
        private readonly RazorpayClient _razorpayClient;
        private readonly string _keySecret;

        public RazorPayGatewayService(IConfiguration configuration)
        {
            var key = configuration["RazorPaySettings:Key"];
            _keySecret = configuration["RazorPaySettings:Secret"]!;

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(_keySecret))
                throw new InvalidOperationException("RazorPay configuration is missing.");

            _razorpayClient = new RazorpayClient(key, _keySecret);
        }

        public async Task<ApiResponse<string>> CreatePaymentOrderAsync(int price)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var options = new Dictionary<string, object>
                    {
                        { "amount", price * 100 },
                        { "currency", "INR" },
                        { "receipt", $"receipt_{Guid.NewGuid()}" }
                    };

                    var order = _razorpayClient.Order.Create(options);
                    return new ApiResponse<string>
                    {
                        StatusCode = 200,
                        Message = "Order created",
                        Data = order["id"].ToString()
                    };
                }
                catch (Exception ex)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = 500,
                        Message = $"Failed to create Razorpay order: {ex.Message}"
                    };
                }
            });
        }

        public async Task<ApiResponse<bool>> VerifyPaymentAsync(RazorPayVerificationDto payment)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var attributes = new Dictionary<string, string>
                    {
                        { "razorpay_payment_id", payment.razorpay_payment_id! },
                        { "razorpay_order_id", payment.razorpay_order_id! },
                        { "razorpay_signature", payment.razorpay_signature! }
                    };

                    Utils.verifyPaymentSignature(attributes);
                    return new ApiResponse<bool> { StatusCode = 200, Message = "Verified", Data = true };
                }
                catch
                {
                    return new ApiResponse<bool> { StatusCode = 400, Message = "Verification failed", Data = false };
                }
            });
        }
    }
}
