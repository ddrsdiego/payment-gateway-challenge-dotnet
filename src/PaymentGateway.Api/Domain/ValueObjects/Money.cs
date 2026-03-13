namespace PaymentGateway.Api.Domain.ValueObjects;

public record Money
{
    private static readonly string[] ValidCurrencies = ["USD", "EUR", "GBP"];

    public int Amount { get; }
    public string Currency { get; }

    private Money(int amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money From(int amount, string currency)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than 0", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3 || !ValidCurrencies.Contains(currency.ToUpper()))
            throw new ArgumentException("Currency must be one of: USD, EUR, GBP", nameof(currency));

        return new Money(amount, currency.ToUpper());
    }
}
