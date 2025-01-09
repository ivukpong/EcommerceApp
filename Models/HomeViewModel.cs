using RobustEcommerceApp.Models;
using System.Collections.Generic;

namespace RobustEcommerceApp.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Product> FeaturedProducts { get; set; } = new List<Product>();
    }
}
