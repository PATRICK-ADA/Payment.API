namespace PaymentService.API.PaymentService.Domain.RequestDto
{
    public class PaystackVerifyResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public Data data { get; set; }

        public class Data
        {
            public string status { get; set; }
            public int amount { get; set; }
            public Customer customer { get; set; }

            public class Customer
            {
                public string email { get; set; }
            }
        }
    }
}
