using System.ComponentModel.DataAnnotations;

namespace YeahWeb.Donation;

public class DonationViewModel
{
    [Required]
    [Display(Name = "Full Name")]
    public string DonorName { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string DonorEmail { get; set; }

    [Required]
    [Display(Name = "Amount (VND)")]
    [Range(1000, 100000000)] // 1000 VND to 100M VND
    public decimal Amount { get; set; }

    [Required]
    [Display(Name = "Message")]
    [MaxLength(150)]
    public string Message { get; set; }

    public string PayGate { get; set; } = "VNPay";

    public string? BankCode { get; set; }

    public string Locale { get; set; } = "vn";
}