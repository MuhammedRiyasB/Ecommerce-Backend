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

        public async Task<CartResponseDto> GetUserCartAsync(Guid userId)
        {
            var cart = await _cartRepo.Query()
                .Include(c => c.CartItems).ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return new CartResponseDto { CartId = Guid.Empty, Items = new List<CartItemResponseDto>() };

            return _mapper.Map<CartResponseDto>(cart);
        }

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
