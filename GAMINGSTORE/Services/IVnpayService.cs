namespace GAMINGSTORE.Services
{
    public interface IVnpayService
    {
        string CreatePaymentUrl(int orderId, decimal amount, string orderInfo, string returnUrl);
        (bool success, string message, int? orderId) ProcessPaymentReturn(IQueryCollection queryCollection);
    }
}
