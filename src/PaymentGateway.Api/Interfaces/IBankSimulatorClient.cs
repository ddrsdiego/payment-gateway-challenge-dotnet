namespace PaymentGateway.Api.Interfaces;

using Models.Bank;

public interface IBankSimulatorClient
{
    Task<BankPaymentResponse> ProcessPaymentAsync(BankPaymentRequest request, CancellationToken cancellationToken = default);
}
