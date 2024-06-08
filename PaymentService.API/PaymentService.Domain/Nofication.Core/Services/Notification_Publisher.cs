
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using  ILogger = Serilog.ILogger;
using Newtonsoft.Json;
using RoomService.Infrastructure.Data;
using Notification.API.Nofication.Core.Abstraction;
using BidService.API.BidService.Domain.RequestDto;
using BidService.API.BidService.Core.ApiResponse;

public class Notification_Publisher : INotification_Publisher
{
    private readonly AppDbContext _context;
    private readonly ILogger _logger;

    public Notification_Publisher(AppDbContext context, ILogger logger)
    {
        _context = context;

        _logger = logger;
    }
    public async Task<ApiResponse<object>> GetHighestBidderAsync()
    {
        var maxAmountPaid = await _context.NotifyBidders.MaxAsync(b => b.AmountPaid);
        var highestBidder = await _context.NotifyBidders
            .Where(b => b.AmountPaid == maxAmountPaid)
            .FirstOrDefaultAsync();
      
        if (highestBidder == null)
        {
            _logger.Warning("No bids found in the database.");
            return new FailureApiResponse("","No bids found in the database");
        }

        return new SuccessApiResponse<BidDto>("retrieved the highest bidder successfully", highestBidder);
    }
}
