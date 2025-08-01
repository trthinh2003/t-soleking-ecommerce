using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoleKingECommerce.Data;
using System.Linq;

namespace SoleKingECommerce.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("admin/[controller]")]
    public class DashboardController : Controller
    {
        private readonly SoleKingDbContext _context;

        public DashboardController(SoleKingDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Get stats for dashboard
            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalCategories = _context.Categories.Count();
            //ViewBag.TodayOrders = _context.Orders
            //    .Where(o => o.CreatedAt.Date == DateTime.Today)
            //    .Count();
            ViewBag.TodayOrders = 0;

            // Recent orders
            //ViewBag.RecentOrders = _context.Orders
            //    .OrderByDescending(o => o.CreatedAt)
            //    .Take(5)
            //    .ToList();
            ViewBag.RecentOrders = new {
                OrderId = 0,
                CustomerName = "Test Customer",
                TotalAmount = 0,
                CreatedAt = DateTime.Now,
                Name = "Test Order",
                SoldQuantity = 0
            };

            // Top selling products
            //ViewBag.TopProducts = _context.Products
            //    .OrderByDescending(p => p.SoldQuantity)
            //    .Take(5)
            //    .ToList();
            ViewBag.TopProducts = 0;

            return View("~/Views/Admin/Dashboard/Index.cshtml");
        }
    }
}