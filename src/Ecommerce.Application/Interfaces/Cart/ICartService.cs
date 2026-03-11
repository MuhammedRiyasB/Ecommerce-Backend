using Ecommerce.Application.DTOs.Cart;

namespace Ecommerce.Application.Interfaces.Cart
{
    /// <summary>
    /// Manages shopping cart operations for authenticated users.
    /// </summary>
    public interface ICartService
    {
        /// <summary>
/// Retrieves the shopping cart for the specified user.
/// </summary>
/// <param name="userId">The unique identifier of the user whose cart to retrieve.</param>
/// <returns>The user's cart represented as a <see cref="CartResponseDto"/>.</returns>
Task<CartResponseDto> GetUserCartAsync(Guid userId);
        /// <summary>
/// Adds an item to the specified user's shopping cart and returns the updated cart.
/// </summary>
/// <param name="userId">The identifier of the user whose cart will be modified.</param>
/// <param name="dto">The request containing product and quantity information to add to the cart.</param>
/// <returns>The updated cart as a <see cref="CartResponseDto"/>.</returns>
Task<CartResponseDto> AddToCartAsync(Guid userId, AddToCartRequestDto dto);
        /// <summary>
/// Removes the specified product from the user's shopping cart.
/// </summary>
/// <param name="userId">The unique identifier of the user whose cart will be modified.</param>
/// <param name="productId">The unique identifier of the product to remove from the cart.</param>
/// <returns>`true` if the product existed in the cart and was removed; `false` otherwise.</returns>
Task<bool> RemoveFromCartAsync(Guid userId, Guid productId);
        /// <summary>
/// Increases the quantity of the specified product in the user's cart by the given amount.
/// </summary>
/// <param name="delta">The amount to increase the product's quantity by; must be greater than zero. Defaults to 1.</param>
/// <returns>`true` if the cart was updated successfully, `false` otherwise.</returns>
Task<bool> IncreaseQuantityAsync(Guid userId, Guid productId, int delta = 1);
        /// <summary>
/// Decreases the quantity of a product in the specified user's cart by a given amount.
/// </summary>
/// <param name="userId">The identifier of the user who owns the cart.</param>
/// <param name="productId">The identifier of the product whose quantity will be decreased.</param>
/// <param name="delta">The amount to decrease the product quantity by; defaults to 1.</param>
/// <returns>`true` if the quantity was decreased successfully, `false` otherwise.</returns>
Task<bool> DecreaseQuantityAsync(Guid userId, Guid productId, int delta = 1);
    }
}
