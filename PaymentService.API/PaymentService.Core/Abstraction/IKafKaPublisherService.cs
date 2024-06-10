
namespace Invoice.Core.Abstraction
{
    public interface IKafKaPublisherService
    {
        Task ProduceAsync(Guid key, string value);

    }
}
