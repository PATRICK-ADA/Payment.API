using Microsoft.AspNetCore.Mvc;
using Notification.API.Nofication.Core.Abstraction;

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
        public async Task<IActionResult> InitializePayment([FromQuery] string UserName)
        {
            var authorizationUrl = await _paymentService.InitializePayment(UserName);
            return Ok(new { url = authorizationUrl.Data });
        }

       
       
        
        [HttpGet("verify-payment")]
        public async Task<IActionResult> VerifyPayment([FromQuery] string reference)
        {
            var result = await _paymentService.VerifyPayment(reference);
            return Ok(result);  
        }


    }
}
