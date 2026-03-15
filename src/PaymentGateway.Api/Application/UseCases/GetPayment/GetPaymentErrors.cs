using PaymentGateway.Api.Application.Utils;

namespace PaymentGateway.Api.Application.UseCases.GetPayment;

public static class GetPaymentErrors
{
    private const string Instance = "/GetPayment";

    public static Response PaymentNotFound(Guid paymentId) =>
        Response.Builder()
            .WithRequestId(paymentId.ToString())
            .WithStatusCode(StatusCodes.Status404NotFound)
            .WithErrorResponse(
                ErrorResponse.Builder()
                    .WithInstance(Instance)
                    .WithTraceId(paymentId.ToString())
                    .WithError("PAYMENT_NOT_FOUND", "PAYMENT_NOT_FOUND", $"Payment {paymentId} not found")
                    .Build())
            .Build();
}
