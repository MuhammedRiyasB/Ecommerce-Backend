using AutoMapper;
using Ecommerce.Application.DTOs.Cart;
using Ecommerce.Application.Interfaces.Cart;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Services.Cart
{
    public class CartService : ICartService
    {
        private const int MaxQuantity = 10;
        private readonly IRepository<Domain.Entities.Cart> _cartRepo;
        private readonly IRepository<CartItem> _cartItemRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CartService> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="CartService"/> with its required dependencies.
        /// </summary>
        /// <param name="cartRepo">Repository for accessing and modifying cart aggregates.</param>
        /// <param name="cartItemRepo">Repository for accessing and modifying cart item entities.</param>
        /// <param name="productRepo">Repository for accessing product entities referenced by cart items.</param>
        /// <param name="unitOfWork">Unit of work used to persist repository changes atomically.</param>
        /// <param name="mapper">Mapper used to translate domain entities to DTOs and vice versa.</param>
        /// <param name="logger">Logger scoped to <see cref="CartService"/> for informational and error logging.</param>
        public CartService(
            IRepository<Domain.Entities.Cart> cartRepo,
            IRepository<CartItem> cartItemRepo,
            IRepository<Product> productRepo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CartService> logger)
        {
            _cartRepo = cartRepo;
            _cartItemRepo = cartItemRepo;
            _productRepo = productRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Adds the specified product and quantity to the user's cart, creating a cart if none exists.
        /// </summary>
        /// <param name="userId">The identifier of the user whose cart will be updated.</param>
        /// <param name="dto">The add-to-cart request containing the product identifier and desired quantity.</param>
        /// <returns>The user's updated cart mapped to <see cref="CartResponseDto"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="userId"/> is Guid.Empty or <paramref name="dto"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the product specified in <paramref name="dto"/> cannot be found.</exception>
        public async Task<CartResponseDto> AddToCartAsync(Guid userId, AddToCartRequestDto dto)
        {
            if (userId == Guid.Empty) throw new ArgumentNullException(nameof(userId), "Invalid user id");
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var product = await _productRepo.GetByIdAsync(dto.ProductId);
            if (product == null) throw new ArgumentException("Product not found", nameof(dto.ProductId));

            var cart = await _cartRepo.Query()
                .Include(c => c.CartItems).ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Domain.Entities.Cart { CartId = Guid.NewGuid(), UserId = userId };
                await _cartRepo.AddAsync(cart);
            }

            var existing = cart.CartItems.FirstOrDefault(ci => ci.ProductId == dto.ProductId);
            if (existing == null)
            {
                cart.CartItems.Add(new CartItem
                {
                    Id = Guid.NewGuid(), CartId = cart.CartId,
                    ProductId = dto.ProductId, Quantity = Math.Max(1, dto.Quantity)
                });
            }
            else
            {
                existing.Quantity = Math.Min(existing.Quantity + dto.Quantity, MaxQuantity);
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("User {UserId} added product {ProductId} to cart", userId, dto.ProductId);

            cart = await _cartRepo.Query()
                .Include(c => c.CartItems).ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return _mapper.Map<CartResponseDto>(cart!);
        }

        /// <summary>
        /// Retrieves the cart for the specified user, including its items and each item's product.
        /// </summary>
        /// <returns>A CartResponseDto representing the user's cart; if no cart exists, a CartResponseDto with CartId = Guid.Empty and an empty Items list.</returns>
        public async Task<CartResponseDto> GetUserCartAsync(Guid userId)
        {
            var cart = await _cartRepo.Query()
                .Include(c => c.CartItems).ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return new CartResponseDto { CartId = Guid.Empty, Items = new List<CartItemResponseDto>() };

            return _mapper.Map<CartResponseDto>(cart);
        }

        /// <summary>
        /// Removes the specified product from the user's cart if it exists.
        /// </summary>
        /// <param name="userId">The identifier of the cart owner.</param>
        /// <param name="productId">The identifier of the product to remove from the cart.</param>
        /// <returns>`true` if the product was found and removed; `false` if the cart or the item was not found.</returns>
        public async Task<bool> RemoveFromCartAsync(Guid userId, Guid productId)
        {
            var cart = await _cartRepo.Query().Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return false;

            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (item == null) return false;

            _cartItemRepo.Remove(item);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Increases the quantity of the specified product in the user's cart up to the service maximum.
        /// </summary>
        /// <param name="userId">The identifier of the user who owns the cart.</param>
        /// <param name="productId">The identifier of the product whose quantity should be increased.</param>
        /// <param name="delta">The amount to increase the item's quantity by.</param>
        /// <returns>`true` if the item's quantity was increased; `false` if the item was not found or already at the maximum allowed quantity.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the user's cart does not exist.</exception>
        public async Task<bool> IncreaseQuantityAsync(Guid userId, Guid productId, int delta = 1)
        {
            var cart = await _cartRepo.Query().Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) throw new InvalidOperationException("Cart not found");

            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (item == null || item.Quantity >= MaxQuantity) return false;

            item.Quantity = Math.Min(item.Quantity + delta, MaxQuantity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Decreases the quantity of a product in the user's cart by a given amount.
        /// </summary>
        /// <param name="delta">Amount to decrease the item's quantity; result is floored at 0. Defaults to 1.</param>
        /// <returns>`true` if the item's quantity was updated or the item was removed, `false` if the product was not found in the cart.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the user's cart does not exist.</exception>
        public async Task<bool> DecreaseQuantityAsync(Guid userId, Guid productId, int delta = 1)
        {
            var cart = await _cartRepo.Query().Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) throw new InvalidOperationException("Cart not found");

            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (item == null) return false;

            item.Quantity = Math.Max(0, item.Quantity - delta);
            if (item.Quantity == 0) _cartItemRepo.Remove(item);

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
