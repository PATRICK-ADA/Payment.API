using API.BidService.Core.ApiResponse;
using PaymentService.API.PaymentService.Domain.RequestDto;

namespace Notification.API.Nofication.Core.Abstraction
{
    public interface IPaymentService
    {
        Task<ApiResponse<object>> InitializePayment(InvoiceDto invoice);
    }
}
