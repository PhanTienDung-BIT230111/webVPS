using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHangOnline.Data;
using WebBanHangOnline.Models;
using System.Linq;
using System.Threading.Tasks;

namespace WebBanHangOnline.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> List(int? categoryId)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            var products = await query.ToListAsync();
            var categories = await _context.Categories.ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;

            return View(products);
        }

        // Action để hiển thị ảnh sản phẩm từ database
        public async Task<IActionResult> GetImage(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product?.ImageData != null)
            {
                return File(product.ImageData, product.ImageContentType ?? "image/jpeg");
            }
            return NotFound();
        }

        public IActionResult Details(int id)
        {
            TempData["SuccessMessage"] = "Tính năng xem chi tiết sản phẩm đang được phát triển. Sản phẩm đã được thêm vào giỏ hàng!";
            return RedirectToAction("AddToCart", "Cart", new { id = id, quantity = 1 });
        }
    }
}
