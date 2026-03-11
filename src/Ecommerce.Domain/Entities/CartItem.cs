namespace Ecommerce.Domain.Entities
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }

        // Navigation Properties
        public Cart Cart { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
