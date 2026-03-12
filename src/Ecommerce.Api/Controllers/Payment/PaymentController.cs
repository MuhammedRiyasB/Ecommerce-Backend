using Ecommerce.Application.DTOs.Payment;
using Ecommerce.Application.Interfaces.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace Ecommerce.Api.Controllers.Payment
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentGatewayService _paymentGatewayService;
        public PaymentController(IPaymentGatewayService paymentGatewayService) => _paymentGatewayService = paymentGatewayService;

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] int price)
        {
            var result = await _paymentGatewayService.CreatePaymentOrderAsync(price);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyPayment([FromBody] RazorPayVerificationDto paymentDto)
        {
            var result = await _paymentGatewayService.VerifyPaymentAsync(paymentDto);
            return StatusCode(result.StatusCode, result);
        }
    }
}
