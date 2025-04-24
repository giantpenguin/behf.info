namespace YeahWeb.PaymentsIntegration
{
    public class VNPayOptions : BasePayOptions
    {
        public override string PayName => "VNPay";
        public string TmnCode { get; set; }

        public string HashSecret { get; set; }

        public string PayEndpoint { get; set; }

        public string ReturnUrl { get; set; }

        public string Version { get; set; } = "2.1.0";
        
        public override string CurrencyCode { get; set; } = "VND";

        public override decimal MaximumAmount { get; set; } = 100000000;

        public override bool Disabled { get; set; } = false;
    }

    public class VNPayPayRequest
    {
        public required string OrderId { get; set; }
        public decimal Amount { get; set; }
        //public string Name { get; set; }
        //public string Email { get; set; }
        public required string Locale { get; set; } = "vn"; //"vn" | "en"
        public required string OrderType { get; set; } = "other";
        public required string BankCode { get; set; }
        public required string OrderDescription { get; set; }
    }

    public class VNPayResponse
    {
        public required string OrderId { get; set; }
        public required string PaymentId { get; set; }
        public required string TransactionId { get; set; }
        public required string ResponseCode { get; set; }
        public required string BankCode { get; set; }
        public decimal Amount { get; set; }
        public string CardType { get; set; }
        public DateTime PayDate { get; set; }
        public required string OrderInfo { get; set; }
        public bool Success => ResponseCode == "00";
    }
}
