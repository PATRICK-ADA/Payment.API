using Confluent.Kafka;
using InvoiceService.API.InvoiceService.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PaymentService.API.PaymentService.Core.ApiResponse;
using PaymentService.API.PaymentService.Domain.RequestDto;
using RestSharp;
using RoomService.Infrastructure.Data;
using Serilog;

namespace Services.Notification_Publisher
{
    public class InvoiceConsumer : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly IConsumer<string, string> _consumer;
        private readonly IProducer<string, string> _producer;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public InvoiceConsumer(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _config = configuration;
            _serviceScopeFactory = serviceScopeFactory;


            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _config["Kafka:BootstrapServers"],
                GroupId = "your-group-id",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                FetchMinBytes = 1,
                EnableAutoCommit = true,
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslUsername = _config["Kafka:SaslUsername"],
                SaslPassword = _config["Kafka:SaslPassword"],
            };
            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            _consumer.Subscribe("Invoice-Topic");

            var producerConfig = new ProducerConfig { BootstrapServers = configuration["Kafka:BootstrapServers"] };
            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = _consumer.Consume(stoppingToken);

                if (consumeResult != null)
                {
                   
                    var paymentResult = await ProcessPayment(consumeResult.Message.Value);

                    var jsonPaymentResult = JsonConvert.SerializeObject(paymentResult);
                    await _producer.ProduceAsync("Payment-Topic", new Message<string, string> { Value = jsonPaymentResult });

                    Log.Information($"Published payment result: {jsonPaymentResult}");
                }
            }
        }
        
        private async Task<PaymentResult> ProcessPayment(string message)
        {

            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var invoice = JsonConvert.DeserializeObject<InvoiceModel>(message);

            var result = await context.Invoices.AddAsync(invoice);
            await context.SaveChangesAsync();
            var client = new RestClient("https://api.paystack.co/transaction/initialize");

            var request = new RestRequest();
            request.Method = Method.Post; 

            request.AddHeader("Authorization", $"Bearer {_config["Paystack:SecretKey"]}");
            request.AddHeader("Content-Type", "application/json");

            
            request.AddJsonBody(new
            {
                email = invoice.UserName,
                amount = invoice.Amount * 100, // amount in kobo
                reference = Guid.NewGuid().ToString()
            });

            var response = await client.ExecuteAsync<PaystackResponse>(request);
            if (response.IsSuccessful)
            {
                return new PaymentResult
                {
                    UserName = invoice.UserName,
                    Amount = invoice.Amount,
                    Status = "Success",
                    Reference = response.Data.data.reference
                };
            }
            else
            {
                return new PaymentResult
                {
                    UserName = invoice.UserName,
                    Amount = invoice.Amount,
                    Status = "Failed",
                    Reference = null
                };
            }
        }

        public override void Dispose()
        {
            _consumer?.Dispose();
            _producer?.Dispose();
            base.Dispose();
        }
    }
}