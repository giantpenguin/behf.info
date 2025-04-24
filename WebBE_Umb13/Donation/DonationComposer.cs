using Umbraco.Cms.Core.Composing;
using YeahWeb.Donation.Services;
using YeahWeb.PaymentsIntegration;

namespace YeahWeb.Donation;

public class DonationComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddScoped<VNPayService>();
        builder.Services.AddSingleton<IDonationStoreService, DonationStoreService>();
    }
}