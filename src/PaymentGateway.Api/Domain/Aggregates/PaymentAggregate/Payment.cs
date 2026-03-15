namespace PaymentGateway.Api.Domain.Aggregates.PaymentAggregate;

public class Payment
{
    public Guid Id { get; private set; }
    public PaymentStatus Status { get; private set; }
    public CardLastFourDigits CardNumberLastFour { get; private set; }
    public int ExpiryMonth { get; private set; }
    public int ExpiryYear { get; private set; }
    public Money Money { get; private set; }

    internal Payment(
        Guid id,
        PaymentStatus status,
        CardLastFourDigits cardNumberLastFour,
        int expiryMonth,
        int expiryYear,
        Money money)
    {
        Id = id;
        Status = status;
        CardNumberLastFour = cardNumberLastFour;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        Money = money;
    }

    public static Payment CreateAuthorized(
        string cardNumber,
        int expiryMonth,
        int expiryYear,
        Money money)
    {
        var cardLastFour = CardLastFourDigits.From(cardNumber);
        return new Payment(
            Guid.NewGuid(),
            PaymentStatus.Authorized,
            cardLastFour,
            expiryMonth,
            expiryYear,
            money);
    }

    public static Payment CreateDeclined(
        string cardNumber,
        int expiryMonth,
        int expiryYear,
        Money money)
    {
        var cardLastFour = CardLastFourDigits.From(cardNumber);
        return new Payment(
            Guid.NewGuid(),
            PaymentStatus.Declined,
            cardLastFour,
            expiryMonth,
            expiryYear,
            money);
    }
}
