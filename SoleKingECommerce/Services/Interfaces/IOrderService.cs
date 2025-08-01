using SoleKingECommerce.Models;

namespace SoleKingECommerce.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> GetOrderByIdAsync(int id);
        Task<bool> UpdateOrderAsync(Order order);
    }
}
