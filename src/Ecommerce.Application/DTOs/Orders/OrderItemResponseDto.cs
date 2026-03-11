namespace Ecommerce.Application.DTOs.Orders
{
    public class OrderItemResponseDto
    {
        public Guid OrderItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public int Price { get; set; }
        public int Quantity { get; set; }
        public int TotalAmount { get; set; }
    }
}
