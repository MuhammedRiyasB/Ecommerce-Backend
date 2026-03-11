using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public Guid AddressId { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
        public DateTime OrderDate { get; set; }
        public string TransactionId { get; set; } = null!;

        // Navigation Properties
        public List<OrderItem> OrderItems { get; set; } = new();
        public User User { get; set; } = null!;
        public Address Address { get; set; } = null!;
    }
}
