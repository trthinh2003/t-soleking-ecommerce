using SoleKingECommerce.ViewModels.Payment;

namespace SoleKingECommerce.Services.Interfaces
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(VnPaymentRequest request, HttpContext context);
        VnPaymentResponse PaymentExecute(IQueryCollection collections);
    }
}
