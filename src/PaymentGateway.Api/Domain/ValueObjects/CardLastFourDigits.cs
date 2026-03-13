namespace PaymentGateway.Api.Domain.ValueObjects;

public record CardLastFourDigits
{
    public int Value { get; }

    private CardLastFourDigits(int value) => Value = value;

    public static CardLastFourDigits From(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
            throw new ArgumentException("Card number must have at least 4 digits", nameof(cardNumber));

        var lastFourString = cardNumber[^4..];
        if (!int.TryParse(lastFourString, out var lastFour))
            throw new ArgumentException("Last 4 digits must be numeric", nameof(cardNumber));

        return new CardLastFourDigits(lastFour);
    }
}
