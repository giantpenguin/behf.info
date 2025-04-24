using System.ComponentModel.DataAnnotations;

namespace YeahWeb.Donation
{
    public class DonationRequest
    {
        public decimal Amount { get; set; }

        public required string DonorEmail { get; set; }

        public required string DonorName { get; set; }

        public required string Message { get; set; }

        public required string PayGate { get; set; }
    }
}