using Microsoft.EntityFrameworkCore;
using  ILogger = Serilog.ILogger;
using RoomService.Infrastructure.Data;
using Notification.API.Nofication.Core.Abstraction;
using BidService.API.BidService.Domain.RequestDto;
using InvoiceService.API.InvoiceService.Domain.Entities;
using PaymentService.API.PaymentService.Core.ApiResponse;
using RestSharp;
using API.BidService.Core.ApiResponse;
using PaymentService.API.PaymentService.Domain.RequestDto;

namespace Services.Notification_Publisher
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        public PaymentService(AppDbContext context, ILogger logger, IConfiguration configuration)
        {
            _context = context;

            _logger = logger;
            _config = configuration;
        }
        public async Task<ApiResponse<object>> InitializePayment(InvoiceDto invoice)
        {
            var client = new RestClient(_config["Paystack:RestClientUrl"]);
            var request = new RestRequest();
            request.Method = Method.Post;
            request.AddHeader("Authorization", $"Bearer {_config["Paystack:SecretKey"]}");
            request.AddHeader("Content-Type", "application/json");

            request.AddJsonBody(new
            {
                email = invoice.UserName,
                amount = invoice.Amount * 100,
                reference = Guid.NewGuid().ToString()
            });

            var response = await client.ExecuteAsync<PaystackResponse>(request);
            if (response.IsSuccessful)
            {
                return new SuccessApiResponse<string>(null, response.Data.data.authorization_url);
            }
            else
            {
                return new FailureApiResponse(null, "Payment Initialization failed. Try again Later", 400);
            }

        }
    }
}