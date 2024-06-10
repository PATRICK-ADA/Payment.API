using API.BidService.Core.ApiResponse;

namespace Notification.API.Nofication.Core.Abstraction
{
    public interface IPaymentService
    {
        Task<ApiResponse<object>> InitializePayment(string UserName);
        Task<ApiResponse<object>> VerifyPayment(string reference);
    }
}
