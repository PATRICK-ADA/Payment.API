﻿namespace PaymentService.API.PaymentService.Domain.Entities
{
    public class PaymentResult
    {
        public string UserName { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string Reference { get; set; }
    }
}
