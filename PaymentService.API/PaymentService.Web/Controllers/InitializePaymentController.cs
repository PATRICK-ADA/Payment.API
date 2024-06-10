using Microsoft.AspNetCore.Mvc;
using Notification.API.Nofication.Core.Abstraction;
using PaymentService.API.PaymentService.Domain.RequestDto;

namespace PaymentService.API.PaymentService.Web.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    public class InitializePaymentController : ControllerBase
    {
        
        private readonly IPaymentService _paymentService;   
        public InitializePaymentController(IPaymentService paymentService) 
        {
        
        _paymentService = paymentService;   
        }


        [HttpPost("initialize-payment")]
        public async Task<IActionResult> InitializePayment([FromBody] InvoiceDto invoice)
        {
            var authorizationUrl = await _paymentService.InitializePayment(invoice);
            return Ok(new { url = authorizationUrl.Data });
        }

    }
}
