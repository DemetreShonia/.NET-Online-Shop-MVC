namespace PresentationLayer.Models
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }

        // Name of the product
        public string? Name { get; set; }

        // List price of the product
        public decimal ListPrice { get; set; }

        public string? ProductNumber { get; set; }

        // Size of the product (can be int or decimal depending on your business logic)
        public string? Size { get; set; }

        // Product category Id
        public int? ProductCategoryId { get; set; }

        // Category Name (you may use this for display purposes in the view)
        public string? CategoryName { get; set; }

        // Number of orders for the product
        public int NumberOfOrders { get; set; }

        // The uploaded photo file (to save the image)
        public IFormFile? Photo { get; set; }
        public string? ThumbnailPhotoFileName { get; set; } 

        // Additional properties (optional)
        public int? ProductModelId { get; set; }
        public string? ProductModelName { get; set; }
    }
}
