using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Rewrite;
using YeahWeb.PaymentsIntegration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

builder.Services.AddOptions<VNPayOptions>().Bind(builder.Configuration.GetSection("PayGates:VNPayConfig"));
//OR builder.Services.Configure<VNPayOptions>(builder.Configuration.GetSection("PayGates:VNPayConfig"));

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

var rewriteOptions = new RewriteOptions()
    .AddIISUrlRewrite(builder.Environment.ContentRootFileProvider, "IISUrlRewrite.xml");
app.UseRewriter(rewriteOptions);

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseInstallerEndpoints();
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

//https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-8.0&viewFallbackFrom=aspnetcore-2.2
app.UseForwardedHeaders(new ForwardedHeadersOptions() { ForwardedHeaders = ForwardedHeaders.XForwardedFor });

// needed for the rewrites to take effect
app.UseStaticFiles();

await app.RunAsync();
