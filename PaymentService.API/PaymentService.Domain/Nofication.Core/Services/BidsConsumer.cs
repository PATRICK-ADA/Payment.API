using BidService.API.BidService.Domain.Entities;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RoomService.Infrastructure.Data;
using ILogger = Serilog.ILogger;

namespace Invoice.API.KafkaConsumerService
{
    public class BidConsumer : BackgroundService
    {
        private readonly string _topic;
        private readonly IConsumer<string, string> _consumer;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly IProducer<string, string> _producer;

        public BidConsumer(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, ILogger logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _config = configuration;

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

            _topic =  "Bid-Topic";
            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            _producer = new ProducerBuilder<string, string>(consumerConfig).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);

            await Task.Run(async () =>
            {
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var consumeResult = _consumer.Consume(stoppingToken);
                        if (consumeResult != null)
                        {
                            await ProcessMessage(consumeResult.Message.Value);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.Warning("Kafka consumer operation was cancelled.");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "An error occurred while consuming Kafka messages.");
                }
                finally
                {
                    _consumer.Close();
                }
            }, stoppingToken);
        }

        private async Task ProcessMessage(string messageValue)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var bidMessage = JsonConvert.DeserializeObject<BidModel>(messageValue);

            if (bidMessage != null)
            {
                var existingBid = await context.NotifyBidders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == bidMessage.Id && c.UserName == bidMessage.UserName &&
                                              c.Cars == bidMessage.Cars && c.AmountPaid == bidMessage.AmountPaid);

                if (existingBid == null)
                {
                    await context.NotifyBidders.AddAsync(bidMessage);
                    await context.SaveChangesAsync();
                }
            }
            
            
            var maxAmountPaid = await context.NotifyBidders.MaxAsync(b => b.AmountPaid);
            var highestBidder = await context.NotifyBidders
                .Where(b => b.AmountPaid == maxAmountPaid)
                .FirstOrDefaultAsync();
        
                var message = JsonConvert.SerializeObject(highestBidder);
                await _producer.ProduceAsync("Notification-Topic", new Message<string, string> { Key = highestBidder.UserName, Value = message });
                _logger.Information("Published highest bidder to Kafka.");

          
        }

        public override void Dispose()
        {
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}
