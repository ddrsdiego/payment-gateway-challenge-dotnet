using PaymentGateway.Api.Application.UseCases.ProcessPayment;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Controllers.Extensions;

public static class PostPaymentRequestEx
{
    public static ProcessPaymentCommand ToCommand(this CreatePaymentRequest request)
    {
        var tracerId = Guid.NewGuid().ToString();

        var command = new ProcessPaymentCommand(
            tracerId,
            request.CardNumber,
            request.ExpiryMonth,
            request.ExpiryYear,
            request.Currency,
            request.Amount,
            request.Cvv);

        return command;
    }
}