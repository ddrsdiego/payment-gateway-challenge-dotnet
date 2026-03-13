namespace PaymentGateway.Api.Application.UseCases.ProcessPayment;

using PaymentGateway.Api.Application.Utils;

public static class ProcessPaymentErrors
{
    private const string Instance = "/ProcessPayment";

    public static Response InvalidCardNumber(string tracerId) =>
        Response.Builder()
            .WithRequestId(tracerId)
            .WithStatusCode(StatusCodes.Status400BadRequest)
            .WithErrorResponse(
                ErrorResponse.Builder()
                    .WithInstance(Instance)
                    .WithTraceId(tracerId)
                    .WithError("INVALID_CARD_NUMBER", "INVALID_CARD_NUMBER", "Card number is invalid")
                    .Build())
            .Build();

    public static Response CardNumberNotNumeric(string tracerId) =>
        Response.Builder()
            .WithRequestId(tracerId)
            .WithStatusCode(StatusCodes.Status400BadRequest)
            .WithErrorResponse(
                ErrorResponse.Builder()
                    .WithInstance(Instance)
                    .WithTraceId(tracerId)
                    .WithError("CARD_NUMBER_NOT_NUMERIC", "CARD_NUMBER_NOT_NUMERIC", "Card number must contain only digits")
                    .Build())
            .Build();

    public static Response InvalidExpiryMonth(string tracerId) =>
        Response.Builder()
            .WithRequestId(tracerId)
            .WithStatusCode(StatusCodes.Status400BadRequest)
            .WithErrorResponse(
                ErrorResponse.Builder()
                    .WithInstance(Instance)
                    .WithTraceId(tracerId)
                    .WithError("INVALID_EXPIRY_MONTH", "INVALID_EXPIRY_MONTH", "Expiry month must be between 1 and 12")
                    .Build())
            .Build();

    public static Response CardExpired(string tracerId) =>
        Response.Builder()
            .WithRequestId(tracerId)
            .WithStatusCode(StatusCodes.Status400BadRequest)
            .WithErrorResponse(
                ErrorResponse.Builder()
                    .WithInstance(Instance)
                    .WithTraceId(tracerId)
                    .WithError("CARD_EXPIRED", "CARD_EXPIRED", "Card has expired")
                    .Build())
            .Build();

    public static Response InvalidCurrency(string tracerId) =>
        Response.Builder()
            .WithRequestId(tracerId)
            .WithStatusCode(StatusCodes.Status400BadRequest)
            .WithErrorResponse(
                ErrorResponse.Builder()
                    .WithInstance(Instance)
                    .WithTraceId(tracerId)
                    .WithError("INVALID_CURRENCY", "INVALID_CURRENCY", "Currency is invalid")
                    .Build())
            .Build();

    public static Response InvalidAmount(string tracerId) =>
        Response.Builder()
            .WithRequestId(tracerId)
            .WithStatusCode(StatusCodes.Status400BadRequest)
            .WithErrorResponse(
                ErrorResponse.Builder()
                    .WithInstance(Instance)
                    .WithTraceId(tracerId)
                    .WithError("INVALID_AMOUNT", "INVALID_AMOUNT", "Amount must be greater than 0")
                    .Build())
            .Build();

    public static Response InvalidCvv(string tracerId) =>
        Response.Builder()
            .WithRequestId(tracerId)
            .WithStatusCode(StatusCodes.Status400BadRequest)
            .WithErrorResponse(
                ErrorResponse.Builder()
                    .WithInstance(Instance)
                    .WithTraceId(tracerId)
                    .WithError("INVALID_CVV", "INVALID_CVV", "CVV is invalid")
                    .Build())
            .Build();

    public static Response BankSimulatorBadRequest(string tracerId, string error) =>
        Response.Builder()
            .WithRequestId(tracerId)
            .WithStatusCode(StatusCodes.Status500InternalServerError)
            .WithErrorResponse(
                ErrorResponse.Builder()
                    .WithInstance(Instance)
                    .WithTraceId(tracerId)
                    .WithError("BANK_SIMULATOR_BAD_REQUEST", "BANK_SIMULATOR_BAD_REQUEST", $"Bank simulator bad request: {error}")
                    .Build())
            .Build();

    public static Response BankSimulatorUnavailable(string tracerId, string error) =>
        Response.Builder()
            .WithRequestId(tracerId)
            .WithStatusCode(StatusCodes.Status500InternalServerError)
            .WithErrorResponse(
                ErrorResponse.Builder()
                    .WithInstance(Instance)
                    .WithTraceId(tracerId)
                    .WithError("BANK_SIMULATOR_UNAVAILABLE", "BANK_SIMULATOR_UNAVAILABLE", $"Bank simulator unavailable: {error}")
                    .Build())
            .Build();

    public static Response UnexpectedError(string tracerId, string error) =>
        Response.Builder()
            .WithRequestId(tracerId)
            .WithStatusCode(StatusCodes.Status500InternalServerError)
            .WithErrorResponse(
                ErrorResponse.Builder()
                    .WithInstance(Instance)
                    .WithTraceId(tracerId)
                    .WithError("UNEXPECTED_ERROR", "UNEXPECTED_ERROR", $"Unexpected error: {error}")
                    .Build())
            .Build();
}
