using PaymentService.API.PaymentService.Domain.RequestDto;

namespace PaymentService.API.PaymentService.Core.ApiResponse
{
    public class PaystackResponse
        {
            public bool status { get; set; }
            public string message { get; set; }
            public PaystackData data { get; set; }
        }
    
}
