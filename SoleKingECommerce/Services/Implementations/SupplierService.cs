using SoleKingECommerce.Helpers;
using SoleKingECommerce.Models;
using SoleKingECommerce.Repositories.Interfaces;
using SoleKingECommerce.Services.Interfaces;
using SoleKingECommerce.ViewModels.Supplier;

namespace SoleKingECommerce.Services.Implementations
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;

        public SupplierService(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<IEnumerable<Supplier>> GetAllSuppliersAsync()
        {
            return await _supplierRepository.GetAllAsync();
        }
        public async Task<Supplier?> GetSupplierByIdAsync(int id) 
        {
            return await _supplierRepository.GetByIdAsync(id);
        }

        public async Task<PaginatedList<Supplier>> GetPaginatedSuppliersAsync(string searchString, int pageIndex = 1, int pageSize = 10) 
        {
            var query = _supplierRepository.GetQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c => c.Name.Contains(searchString));
            }
            return await PaginatedList<Supplier>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task CreateSupplierAsync(SupplierViewModel model)
        {
            var supplier = new Supplier
            {
                Name = model.Name,
                Address = model.Address,
                Email = model.Email,
                Phone = model.Phone
            };
            await _supplierRepository.AddAsync(supplier);
        }

        public async Task UpdateSupplierAsync(SupplierViewModel model)
        {
            var supplier = await _supplierRepository.GetByIdAsync(model.Id);
            if (supplier == null)
            {
                throw new ArgumentException("Supplier not found");
            }
            supplier.Name = model.Name;
            supplier.Address = model.Address;
            supplier.Email = model.Email;
            supplier.Phone = model.Phone;
            await _supplierRepository.UpdateAsync(supplier);
        }

        public async Task DeleteSupplierAsync(int id) 
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);
            if (supplier == null)
            {
                throw new ArgumentException("Supplier not found");
            }
            await _supplierRepository.DeleteAsync(supplier);
        }

        public async Task<bool> SupplierExistsAsync(int id) 
        {
            return await _supplierRepository.ExistsAsync(id);
        }
    }
}
