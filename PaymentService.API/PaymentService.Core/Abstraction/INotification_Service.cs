using BidService.API.BidService.Core.ApiResponse;

namespace Notification.API.Nofication.Core.Abstraction
{
    public interface INotification_Publisher
    {

        Task<ApiResponse<object>> GetHighestBidderAsync();
    }
}
