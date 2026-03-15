namespace PaymentGateway.Api.Domain.Aggregates.PaymentAggregate;

public readonly record struct CardLastFourDigits
{
    private CardLastFourDigits(string value) => Value = value;

    public string Value { get; }

    public static CardLastFourDigits From(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
            throw new ArgumentException("Card number must have at least 4 digits", nameof(cardNumber));
        
        if (!long.TryParse(cardNumber, out var cardNumberAsNumber))
            throw new ArgumentException("Last 4 digits must be numeric", nameof(cardNumber));

        
        var lastFourString = cardNumber[^4..];
        return new CardLastFourDigits(lastFourString);
    }

    internal static CardLastFourDigits FromDataBase(string lastFourDataBase)
    {
        return From(lastFourDataBase); 
    }
}