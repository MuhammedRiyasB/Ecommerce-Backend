using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Domain.Common;

namespace Ecommerce.Application.Interfaces.Orders
{
    /// <summary>
    /// Manages order lifecycle — creation, retrieval, status updates, and revenue reporting.
    /// </summary>
    public interface IOrderService
    {
        Task<bool> CreateOrderAsync(Guid userId, CreateOrderRequestDto createOrderDto);
        Task<PagedResult<OrderDetailsResponseDto>> GetOrdersByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10);
        Task<OrderDetailsResponseDto> GetOrderByIdAsync(Guid orderId, Guid requestingUserId, bool isAdmin);
        Task<UpdateOrderStatusResponseDto> ChangeOrderStatusAsync(Guid orderId, string status);
        Task<RevenueResponseDto> GetRevenueAsync();
    }
}
