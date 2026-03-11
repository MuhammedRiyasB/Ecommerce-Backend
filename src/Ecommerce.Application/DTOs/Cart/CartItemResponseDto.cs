namespace Ecommerce.Application.DTOs.Cart
{
    public class CartItemResponseDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Price { get; set; }
        public int Quantity { get; set; }
        public int TotalPrice { get; set; }
        public string Image { get; set; } = null!;
    }
}
