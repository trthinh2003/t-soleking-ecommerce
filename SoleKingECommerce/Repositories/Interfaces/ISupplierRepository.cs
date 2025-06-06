using SoleKingECommerce.Models;

namespace SoleKingECommerce.Repositories.Interfaces
{
    public interface ISupplierRepository : IRepository<Supplier> 
    {
        IQueryable<Supplier> GetQueryable();
    }
}
