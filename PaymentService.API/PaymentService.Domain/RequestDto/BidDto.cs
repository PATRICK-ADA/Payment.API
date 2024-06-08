

namespace BidService.API.BidService.Domain.RequestDto
{
    public class BidDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserName { get; set; } = null!;
        public List<string> Cars { get; set; } = null!;
        public decimal AmountPaid { get; set; }

    }
}
