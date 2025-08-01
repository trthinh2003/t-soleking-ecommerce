using Microsoft.EntityFrameworkCore;
using SoleKingECommerce.Data;
using SoleKingECommerce.Models;
using SoleKingECommerce.Services.Interfaces;

namespace SoleKingECommerce.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly SoleKingDbContext _context;

        public OrderService(SoleKingDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(o => o.Items)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(pv => pv.Color)
                .Include(o => o.Transactions)
                .Include(o => o.VoucherUsages)
                    .ThenInclude(vu => vu.Voucher)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            try
            {
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
