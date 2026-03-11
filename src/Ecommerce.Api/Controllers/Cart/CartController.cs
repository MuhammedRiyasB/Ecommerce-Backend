using Ecommerce.Application.DTOs.Cart;
using Ecommerce.Application.Interfaces.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Asp.Versioning;

namespace Ecommerce.Api.Controllers.Cart
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService) => _cartService = cartService;

        private Guid GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");
            if (claim == null || !Guid.TryParse(claim.Value, out var userId))
                throw new UnauthorizedAccessException("User id not found in token");
            return userId;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(await _cartService.AddToCartAsync(GetUserId(), dto));
        }

        [HttpGet]
        public async Task<IActionResult> GetCart() => Ok(await _cartService.GetUserCartAsync(GetUserId()));

        [HttpDelete("{productId:guid}")]
        public async Task<IActionResult> RemoveItem([FromRoute] Guid productId)
        {
            var success = await _cartService.RemoveFromCartAsync(GetUserId(), productId);
            return success ? Ok(new { message = "Item removed" }) : NotFound(new { message = "Item not found in cart" });
        }

        [HttpPut("decrease/{productId:guid}")]
        public async Task<IActionResult> DecreaseQuantity([FromRoute] Guid productId, [FromQuery] int delta = 1)
        {
            var success = await _cartService.DecreaseQuantityAsync(GetUserId(), productId, delta);
            return success ? Ok(new { message = "Quantity decreased" }) : BadRequest(new { message = "Cannot decrease quantity" });
        }

        [HttpPut("increase/{productId:guid}")]
        public async Task<IActionResult> IncreaseQuantity([FromRoute] Guid productId, [FromQuery] int delta = 1)
        {
            var success = await _cartService.IncreaseQuantityAsync(GetUserId(), productId, delta);
            return success ? Ok(new { message = "Quantity increased" }) : BadRequest(new { message = "Cannot increase quantity" });
        }
    }
}
