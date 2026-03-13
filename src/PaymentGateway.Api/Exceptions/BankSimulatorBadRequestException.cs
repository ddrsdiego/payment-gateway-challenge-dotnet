namespace PaymentGateway.Api.Exceptions;

public sealed class BankSimulatorBadRequestException : Exception
{
    public BankSimulatorBadRequestException(string message) : base(message)
    {
    }

    public BankSimulatorBadRequestException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
