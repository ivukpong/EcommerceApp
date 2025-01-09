using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RobustEcommerceApp.Data;
using RobustEcommerceApp.Models;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;

namespace RobustEcommerceApp.Controllers
{
     [Route("[controller]")]
     [Authorize] // Ensure that all actions in the controller require authentication
     public class CartController : Controller
     {
          private readonly ApplicationDbContext _context;
          private readonly UserManager<IdentityUser> _userManager;

          public CartController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
          {
               _context = context;
               _userManager = userManager;
          }

          // Renders the cart view for the authenticated user
          [HttpGet]
          [SwaggerOperation(Summary = "Renders the cart view for the authenticated user.")]
          public async Task<IActionResult> Index()
          {
               var user = await _userManager.GetUserAsync(User);

               if (string.IsNullOrEmpty(user?.Id))
                    return Unauthorized("User is not authenticated.");

               var cart = await _context.Carts
                   .Include(c => c.Items)
                   .ThenInclude(i => i.Product)
                   .FirstOrDefaultAsync(c => c.UserId == user.Id);

               return View(cart);
          }

          // Adds a product to the cart
          [HttpPost("AddToCart/{productId}")]
          [SwaggerOperation(Summary = "Adds a product to the cart.")]
          public async Task<IActionResult> AddToCart(int productId)
          {
               if (!User.Identity.IsAuthenticated)
               {
                    return Challenge(); // Redirects to the login page if the user is not authenticated
               }

               var user = await _userManager.GetUserAsync(User);
               var product = await _context.Products.FindAsync(productId);
               if (product == null)
                    return NotFound("Product not found.");

               var cart = await _context.Carts
                   .Include(c => c.Items)
                   .FirstOrDefaultAsync(c => c.UserId == user.Id);

               if (cart == null)
               {
                    cart = new Cart { UserId = user.Id, Items = new List<CartItem>() };
                    _context.Carts.Add(cart);
               }

               var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
               if (cartItem == null)
               {
                    cart.Items.Add(new CartItem { ProductId = productId, Quantity = 1 });
               }
               else
               {
                    cartItem.Quantity++;
               }

               await _context.SaveChangesAsync();

               return RedirectToAction("Index");
          }

          // Removes a product from the cart
          [HttpPost("RemoveFromCart")]
          [SwaggerOperation(Summary = "Removes a product from the cart.")]
          public async Task<IActionResult> RemoveFromCart(int productId)
          {
               var user = await _userManager.GetUserAsync(User);
               if (string.IsNullOrEmpty(user?.Id))
                    return Unauthorized("User is not authenticated.");

               var cart = await _context.Carts
                   .Include(c => c.Items)
                   .FirstOrDefaultAsync(c => c.UserId == user.Id);

               if (cart == null)
                    return NotFound("Cart not found.");

               var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
               if (cartItem != null)
               {
                    cart.Items.Remove(cartItem);
                    await _context.SaveChangesAsync();
               }

               return RedirectToAction("Index");
          }

          // Redirects to the checkout page with cart information
          [HttpPost]
          [SwaggerOperation(Summary = "Redirects to the checkout page with cart information.")]
          public async Task<IActionResult> Checkout()
          {
               var user = await _userManager.GetUserAsync(User);
               if (string.IsNullOrEmpty(user?.Id))
                    return Unauthorized("User is not authenticated.");

               var cart = await _context.Carts
                   .Include(c => c.Items)
                   .ThenInclude(i => i.Product)
                   .FirstOrDefaultAsync(c => c.UserId == user.Id);

               if (cart == null)
                    return RedirectToAction("Index", "Product");

               var order = new Order
               {
                    Items = cart.Items.Select(i => new OrderItem
                    {
                         Product = i.Product,
                         ProductId = i.ProductId,
                         Quantity = i.Quantity,
                         Price = i.Product.Price
                    }).ToList()
               };

               return View(order);
          }
     }
}
