namespace PaymentGateway.Api.Settings;

public sealed class BankSimulatorOptions
{
    public string BaseUrl { get; init; } = string.Empty;
    public RetryOptions Retry { get; init; } = new();
}

public sealed class RetryOptions
{
    public int RetryCount { get; init; } = 3;
    public int DelayInSeconds { get; init; } = 1;
}
