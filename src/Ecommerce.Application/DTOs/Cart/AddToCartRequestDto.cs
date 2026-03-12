namespace Ecommerce.Application.DTOs.Cart
{
    public class AddToCartRequestDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
