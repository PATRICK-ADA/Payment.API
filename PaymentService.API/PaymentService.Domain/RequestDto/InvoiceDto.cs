namespace PaymentService.API.PaymentService.Domain.RequestDto
{
    public class InvoiceDto
    {
        public string UserName { get; set; } = null!;
        public List<string> Cars { get; set; } = new();
        public decimal Amount { get; set; }
        public DateTime InvoiceDate { get; set; }
    }
}
