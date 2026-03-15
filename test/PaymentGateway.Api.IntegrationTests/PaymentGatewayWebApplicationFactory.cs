using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using PaymentGateway.Api.Interfaces;

namespace PaymentGateway.Api.IntegrationTests;

internal class PaymentGatewayWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IBankSimulatorClient> BankSimulatorClientMock { get; } = new(MockBehavior.Loose);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBankSimulatorClient));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddScoped(_ => BankSimulatorClientMock.Object);
        });
    }
}
