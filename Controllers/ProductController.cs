using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RobustEcommerceApp.Data;
using RobustEcommerceApp.Models;

namespace RobustEcommerceApp.Controllers
{
    [Route("[controller]")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all products (Read)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            return View(products); // Return list of products to the view
        }

        // Display specific product details (Read)
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product); // Return product details to the view
        }

        // Display form for creating a new product (Create - GET)
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(); // Return the create product form
        }

        // Handle form submission for creating a new product (Create - POST)
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,ImageUrl")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Redirect to the list of products
            }
            return View(product); // Return the form with validation errors
        }

        // Display form for editing a product (Update - GET)
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product); // Return the edit product form
        }

        // Handle form submission for updating a product (Update - POST)
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,ImageUrl")] Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index)); // Redirect to the list of products
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(product); // Return the form with validation errors
        }

        // Display confirmation for deleting a product (Delete - GET)
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product); // Return the delete confirmation page
        }

        // Handle product deletion (Delete - POST)
        [HttpPost("delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index)); // Redirect to the list of products
        }

        // Check if a product exists by ID
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
