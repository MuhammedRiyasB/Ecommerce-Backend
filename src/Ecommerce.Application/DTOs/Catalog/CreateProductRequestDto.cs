namespace Ecommerce.Application.DTOs.Catalog
{
    public class CreateProductRequestDto
    {
        public string Brand { get; set; } = null!;
        public int Quantity { get; set; }
        public int Price { get; set; }
        public int Discount { get; set; }
        public string Description { get; set; } = null!;
        public int? Size { get; set; }
        public string CategoryName { get; set; } = null!;
    }
}
