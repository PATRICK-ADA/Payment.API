

namespace InvoiceService.API.InvoiceService.Domain.Entities
{
    public class InvoiceModel
    {

        public Guid Id { get; set; } = Guid.NewGuid();   
        public string UserName { get; set; } = null!;
        public List<string> Cars { get; set; } = new();
        public decimal Amount { get; set; }
        public DateTime InvoiceDate { get; set; }


    }


}
