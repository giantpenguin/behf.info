using System.ComponentModel.DataAnnotations;

namespace YeahWeb.Donation
{
    public class DonationRequest
    {
        public /*decimal*/double Amount { get; set; }

        public required string DonorEmail { get; set; }

        public required string DonorName { get; set; }

        public required string Message { get; set; }

        public required string PayGate { get; set; }
        public required string Locale { get; set; }

        public Guid FormRecordId { get; set; } = Guid.Empty;
        public string SubmitReferrer { get; set; }
    }
}