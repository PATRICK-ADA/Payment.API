using Microsoft.EntityFrameworkCore;
using RoomService.Infrastructure.Data;
using Notification.API.Nofication.Core.Abstraction;
using PaymentService.API.PaymentService.Core.ApiResponse;
using RestSharp;
using API.BidService.Core.ApiResponse;
using PaymentService.API.PaymentService.Domain.RequestDto;
using Invoice.Core.Abstraction;
using PaymentService.API.PaymentService.Domain.Entities;

namespace Services.Notification_Publisher
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IKafKaPublisherService _kafkapublisher;

        public PaymentService(AppDbContext context, IConfiguration configuration, IKafKaPublisherService kafkapublisher)
        {
            _context = context;

            _kafkapublisher = kafkapublisher;
            _config = configuration;
        }
        
        
        public async Task<ApiResponse<object>> InitializePayment(string UserName)
        {
            var client = new RestClient(_config["Paystack:RestClientUrl"]);
            var request = new RestRequest();
            request.Method = Method.Post;
            request.AddHeader("Authorization", $"Bearer {_config["Paystack:SecretKey"]}");
            request.AddHeader("Content-Type", "application/json");
           
            var result = await _context.Invoices.FirstOrDefaultAsync(x => x.UserName == UserName);
            if (result == null)
            {
                return new FailureApiResponse(null, "Invoice with the specified UserName doesn't exist", 400);
            }
           
            request.AddJsonBody(new
            {
                email = result.UserName,
                amount = result.Amount * 100,
                reference = Guid.NewGuid().ToString()
            });

            var response = await client.ExecuteAsync<PaystackResponse>(request);
            if (response.IsSuccessful)
            {
                return new SuccessApiResponse<string>("Success", response.Data.data.authorization_url?? "");
            }
            else
            {
                return new FailureApiResponse(null, "Payment Initialization failed. Try again Later", 400);
            }

        }


        public async Task<ApiResponse<object>> VerifyPayment(string reference)
        {
            var client = new RestClient($"https://api.paystack.co/transaction/verify/{reference}");
            var request = new RestRequest();
            request.Method = Method.Get;
            request.AddHeader("Authorization", $"Bearer {_config["Paystack:SecretKey"]}");

            var response = await client.ExecuteAsync<PaystackVerifyResponse>(request);
            if (response.IsSuccessful && response.Data.data.status == "success")
            {
                var paymentResult = new PaymentResult
                {
                    UserName = response.Data.data.customer.email,
                    Amount = response.Data.data.amount / 100, // Convert back from kobo
                    Status = "Success",
                    Reference = reference
                };

                await _kafkapublisher.PublishPaymentResult(reference, paymentResult); 
                await _context.PaymentResults.AddAsync(paymentResult);
                await _context.SaveChangesAsync();

                return new SuccessApiResponse<object>("Success", paymentResult, 200);

            }
            return new FailureApiResponse(null, "Failed to verify payment and send payment details to kafka", 400);
        }    
    }
}