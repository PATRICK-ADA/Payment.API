using Confluent.Kafka;
using InvoiceService.API.InvoiceService.Domain.Entities;
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
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = _consumer.Consume(stoppingToken);

                    if (consumeResult != null)
                    {

                       await ProcessPayment(consumeResult.Message.Value);
                        

                       // var jsonPaymentResult = JsonConvert.SerializeObject(paymentResult);
                       // await _producer.ProduceAsync("Payment-Topic", new Message<string, string> { Value = jsonPaymentResult });

                      //  Log.Information($"Published payment result: {jsonPaymentResult}");
                    }
                }
            }
                 catch (Exception ex)
            {
                 Log.Information($"{ex.Message}");
            }
             
         }

        private async Task ProcessPayment(string paymentResult)
        {

            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var invoice = JsonConvert.DeserializeObject<InvoiceModel>(paymentResult);

            var result = await context.Invoices.AddAsync(invoice);
            await context.SaveChangesAsync();
        }
        public override void Dispose()
        {
            _consumer?.Dispose();
            _producer?.Dispose();
            base.Dispose();
        }
    }
}