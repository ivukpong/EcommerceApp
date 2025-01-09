using Microsoft.AspNetCore.Mvc;
using RobustEcommerceApp.Data;
using RobustEcommerceApp.Models;

namespace RobustEcommerceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Example: Taking the first 3 products as featured
            var featuredProducts = _context.Products.Take(3).ToList();
            var viewModel = new HomeViewModel
            {
                FeaturedProducts = featuredProducts
            };

            return View(viewModel);
        }
    }
}
