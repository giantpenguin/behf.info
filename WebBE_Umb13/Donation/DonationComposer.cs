using Umbraco.Cms.Core.Composing;
using Umbraco.Forms.Core.Providers;
using YeahWeb.Donation.Services;
using YeahWeb.PaymentsIntegration;

namespace YeahWeb.Donation;

public class DonationComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<VNPayService>();
        builder.Services.AddSingleton<IDonationTransRepository, DonationTransRepository>();
        builder.WithCollectionBuilder<WorkflowCollectionBuilder>()
                .Add<DonationTransHandleWorkflow>();
    }
}