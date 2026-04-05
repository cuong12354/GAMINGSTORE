using GAMINGSTORE.Models;

namespace GAMINGSTORE.ViewModels
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; } = default!;

        public List<string> ImageUrls { get; set; } = new();

        public List<string> Highlights { get; set; } = new();

        public List<ProductSpecificationItem> Specifications { get; set; } = new();

        public List<Product> RelatedProducts { get; set; } = new();

        public string PrimaryCategory { get; set; } = string.Empty;
    }

    public class ProductSpecificationItem
    {
        public string Label { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}