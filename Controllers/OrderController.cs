using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RobustEcommerceApp.Data;
using RobustEcommerceApp.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;

namespace RobustEcommerceApp.Controllers
{
    [Route("[controller]")]
    [Authorize] // Ensure that all actions in the controller require authentication
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Render orders view
        [HttpGet]
        [SwaggerOperation(Summary = "Renders the orders view for the authenticated user.")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.Id == null)
                return Unauthorized("User is not authenticated.");

            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.UserId == user.Id)
                .ToListAsync();

            return View(orders);
        }

        // Render order details view
        [HttpGet("Details/{id}")]
        [SwaggerOperation(Summary = "Renders the view for details of a specific order.")]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.Id == null)
                return Unauthorized("User is not authenticated.");

            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null)
                return NotFound("Order not found.");

            return View(order);
        }

        // Render checkout view with cart items
        [HttpGet("Checkout")]
        [SwaggerOperation(Summary = "Renders the checkout view with cart items.")]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (string.IsNullOrEmpty(user?.Id))
                return Unauthorized("User is not authenticated.");

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index", "Product");

            var order = new Order
            {
                Items = cart.Items.Select(i => new OrderItem
                {
                    Product = i.Product,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Product.Price
                }).ToList(),
                ShippingAddress = new ShippingAddress()
            };

            return View(order);
        }

        [HttpPost("Details")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details([Bind("Id,UserId,User,OrderDate,Items,ShippingAddressId,ShippingAddress")] Order order)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.Id == null)
                return Unauthorized("User is not authenticated.");

            order.User = user;
            order.UserId = user.Id;
            DateTime localDateTime = DateTime.Now; // Local time
            DateTime utcDateTime = localDateTime.ToUniversalTime();
            order.OrderDate = utcDateTime;
           
            // Retrieve and validate Product for each OrderItem
            foreach (var item in order.Items)
            {
                if (item.ProductId == 0)
                {
                    ModelState.AddModelError(string.Empty, "Invalid product information.");
                    return View("Checkout", order);
                }

                Product? product = await _context.Products.FindAsync(item.ProductId);
                item.Product = product;
                if (item.Product == null)
                {
                    ModelState.AddModelError(string.Empty, $"Product with ID {item.ProductId} not found.");
                    return View("Checkout", order);
                }
            }

            //if (!ModelState.IsValid)
            //    return View("Checkout", order);

            _context.Orders.Add(order);

            // Remove cart items
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = order.Id });
        }

    }
}
