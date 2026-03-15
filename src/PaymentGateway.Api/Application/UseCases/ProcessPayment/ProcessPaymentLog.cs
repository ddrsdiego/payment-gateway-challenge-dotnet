using System.Diagnostics.CodeAnalysis;

namespace PaymentGateway.Api.Application.UseCases.ProcessPayment;

internal static partial class ProcessPaymentLog
{
    private const string LogType = "process-payment";

    [SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by source generator")]
    [LoggerMessage(Level = LogLevel.Information,
        Message = "[" + LogType + "] | [{TracerId}] - Processing payment")]
    public static partial void ProcessingPayment(ILogger logger, string tracerId);

    [SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by source generator")]
    [LoggerMessage(Level = LogLevel.Information,
        Message = "[" + LogType + "] | [{TracerId}] - Payment processed successfully")]
    public static partial void PaymentProcessed(ILogger logger, string tracerId);

    [SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by source generator")]
    [LoggerMessage(Level = LogLevel.Error,
        Message = "[" + LogType + "] | [{TracerId}] - Bank simulator bad request: {Error}")]
    public static partial void BankSimulatorBadRequest(ILogger logger, string tracerId, string error);

    [SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by source generator")]
    [LoggerMessage(Level = LogLevel.Error,
        Message = "[" + LogType + "] | [{TracerId}] - Bank simulator failed: {Error}")]
    public static partial void BankSimulatorFailed(ILogger logger, string tracerId, string error);

    [SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by source generator")]
    [LoggerMessage(Level = LogLevel.Error,
        Message = "[" + LogType + "] | [{TracerId}] - Unexpected error: {Error}")]
    public static partial void UnexpectedError(ILogger logger, string tracerId, string error);
}
