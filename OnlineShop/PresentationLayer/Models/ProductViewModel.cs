namespace PresentationLayer.Models
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public int? ProductCategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int NumberOfOrders { get; set; }
    }


}
