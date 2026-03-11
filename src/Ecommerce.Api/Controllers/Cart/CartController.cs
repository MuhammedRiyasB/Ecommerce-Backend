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
        /// <summary>
/// Initializes a new instance of <see cref="CartController"/> with the provided cart service.
/// </summary>
/// <param name="cartService">Service used to perform cart operations for the controller.</param>
public CartController(ICartService cartService) => _cartService = cartService;

        /// <summary>
        /// Retrieve the current user's GUID from the authentication claims.
        /// </summary>
        /// <returns>The user's GUID extracted from the authentication token.</returns>
        /// <exception cref="System.UnauthorizedAccessException">Thrown when the user id claim is missing or is not a valid GUID.</exception>
        private Guid GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");
            if (claim == null || !Guid.TryParse(claim.Value, out var userId))
                throw new UnauthorizedAccessException("User id not found in token");
            return userId;
        }

        /// <summary>
        /// Adds the specified item to the authenticated user's cart.
        /// </summary>
        /// <param name="dto">Details of the product and quantity to add to the cart.</param>
        /// <returns>An OkObjectResult containing the service's add-to-cart result, or a BadRequestObjectResult with validation errors if the request model is invalid.</returns>
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(await _cartService.AddToCartAsync(GetUserId(), dto));
        }

        /// <summary>
        /// Retrieves the current user's shopping cart.
        /// </summary>
        /// <returns>An OkObjectResult containing the user's cart data.</returns>
        [HttpGet]
        public async Task<IActionResult> GetCart() => Ok(await _cartService.GetUserCartAsync(GetUserId()));

        /// <summary>
        /// Removes the specified product from the current user's cart.
        /// </summary>
        /// <param name="productId">The identifier of the product to remove from the cart.</param>
        /// <returns>`200 OK` with `{ message = "Item removed" }` if the product was removed; `404 Not Found` with `{ message = "Item not found in cart" }` if the product was not present.</returns>
        [HttpDelete("{productId:guid}")]
        public async Task<IActionResult> RemoveItem([FromRoute] Guid productId)
        {
            var success = await _cartService.RemoveFromCartAsync(GetUserId(), productId);
            return success ? Ok(new { message = "Item removed" }) : NotFound(new { message = "Item not found in cart" });
        }

        /// <summary>
        /// Decrease the quantity of a product in the current user's cart.
        /// </summary>
        /// <param name="productId">The product identifier to decrease the quantity for.</param>
        /// <param name="delta">The number of units to decrease; defaults to 1.</param>
        /// <returns>An <see cref="IActionResult"/> that is 200 OK with { message = "Quantity decreased" } when the decrease succeeds, or 400 Bad Request with { message = "Cannot decrease quantity" } when it fails.</returns>
        [HttpPut("decrease/{productId:guid}")]
        public async Task<IActionResult> DecreaseQuantity([FromRoute] Guid productId, [FromQuery] int delta = 1)
        {
            var success = await _cartService.DecreaseQuantityAsync(GetUserId(), productId, delta);
            return success ? Ok(new { message = "Quantity decreased" }) : BadRequest(new { message = "Cannot decrease quantity" });
        }

        /// <summary>
        /// Increases the quantity of a product in the current user's cart by the specified delta.
        /// </summary>
        /// <param name="productId">The identifier of the product to increase in the cart.</param>
        /// <param name="delta">The amount to increase the product's quantity by; defaults to 1.</param>
        /// <returns>200 OK with { message = "Quantity increased" } if the quantity was increased, 400 Bad Request with { message = "Cannot increase quantity" } otherwise.</returns>
        [HttpPut("increase/{productId:guid}")]
        public async Task<IActionResult> IncreaseQuantity([FromRoute] Guid productId, [FromQuery] int delta = 1)
        {
            var success = await _cartService.IncreaseQuantityAsync(GetUserId(), productId, delta);
            return success ? Ok(new { message = "Quantity increased" }) : BadRequest(new { message = "Cannot increase quantity" });
        }
    }
}
