namespace YeahWeb.PaymentsIntegration
{
    public class PayGatesOptions
    {
        public const string SectionName = "PayGates";

        public VNPayOptions VNPaySettings { get; set; }
    }

    public abstract class BasePayOptions
    {
        public abstract string PayName { get; }

        public abstract string CurrencyCode { get; set; }

        public virtual decimal MaximumAmount { get; set; }

        public virtual int ExpireOffset { get; set; } = 900; // 15mins expire time

        public virtual bool Disabled { get; set; } = true;
    }

}