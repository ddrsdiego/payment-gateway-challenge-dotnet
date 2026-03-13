namespace PaymentGateway.Api.Exceptions;

public sealed class BankSimulatorException : Exception
{
    public BankSimulatorException(string message) : base(message)
    {
    }

    public BankSimulatorException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
