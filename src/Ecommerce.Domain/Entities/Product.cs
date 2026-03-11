using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Domain.Entities
{
    public class Product : ISoftDeletable
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public int Price { get; set; }
        public int Discount { get; set; }
        public string Description { get; set; } = null!;
        public string Image { get; set; } = null!;
        public int Size { get; set; }
        public int CategoryId { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        public Category Category { get; set; } = null!;
        public List<CartItem> CartItems { get; set; } = new();
    }
}
