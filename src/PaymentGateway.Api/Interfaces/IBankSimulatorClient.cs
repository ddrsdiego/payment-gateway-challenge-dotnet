using PaymentGateway.Api.Models.Bank;

namespace PaymentGateway.Api.Interfaces;

public interface IBankSimulatorClient
{
    Task<BankPaymentResponse> ProcessPaymentAsync(BankPaymentRequest request, CancellationToken cancellationToken = default);
}
