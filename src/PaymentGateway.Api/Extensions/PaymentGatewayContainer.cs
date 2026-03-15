using PaymentGateway.Api.Application.UseCases.ProcessPayment;
using PaymentGateway.Api.Domain.Aggregates.PaymentAggregate;
using PaymentGateway.Api.Infra.Repositories.Payments;
using PaymentGateway.Api.Interfaces;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Settings;

using Polly;
using Polly.Extensions.Http;

namespace PaymentGateway.Api.Extensions;

public static class PaymentGatewayContainer
{
    public static void AddPaymentGatewayServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(opt =>
            opt.RegisterServicesFromAssemblies(typeof(ProcessPaymentCommand).Assembly));

        var bankSimulatorOptions = configuration
            .GetSection("BankSimulator")
            .Get<BankSimulatorOptions>() ?? new BankSimulatorOptions();

        services.AddSingleton<IPaymentsRepository, PaymentsRepository>();
        services.AddScoped<IBankSimulatorClient, BankSimulatorClient>();
        
        services.AddHttpClient("BankSimulator", client =>
            {
                if (!string.IsNullOrEmpty(bankSimulatorOptions.BaseUrl))
                    client.BaseAddress = new Uri(bankSimulatorOptions.BaseUrl);
            })
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    bankSimulatorOptions.Retry.RetryCount,
                    retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * bankSimulatorOptions.Retry.DelayInSeconds)));
    }
}