using SoleKingECommerce.Models;

namespace SoleKingECommerce.Services.Interfaces
{
    public interface IColorService
    {
        Task<List<Color>> GetAllColorsAsync();
    }
}
