
using PaymentService.API.PaymentService.Domain.Entities;

namespace Invoice.Core.Abstraction
{
    public interface IKafKaPublisherService
    {
        Task ProduceAsync(Guid key, string value);
        Task PublishPaymentResult(string key, PaymentResult paymentResult);

    }
}
