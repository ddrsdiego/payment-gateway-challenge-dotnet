namespace PaymentGateway.Api.Application.UseCases.ProcessPayment;

using MediatR;
using PaymentGateway.Api.Application.Utils;

public record ProcessPaymentCommand(
    string TracerId,
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    string Currency,
    int Amount,
    string Cvv) : IRequest<Response>;
