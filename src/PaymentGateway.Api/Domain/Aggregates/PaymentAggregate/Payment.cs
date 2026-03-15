namespace PaymentGateway.Api.Domain.Aggregates.PaymentAggregate;

public class Payment
{
    public Guid Id { get; private set; }
    public PaymentStatus Status { get; private set; }
    public CardLastFourDigits CardNumberLastFour { get; private set; }
    public ExpiryCardDate ExpiryCardDate { get; private set; }
    public Money Money { get; private set; }
    public AuthorizationCode Authorization { get; private set; }

    internal Payment(
        Guid id,
        PaymentStatus status,
        CardLastFourDigits cardNumberLastFour,
        ExpiryCardDate expiryCardDate,
        Money money,
        AuthorizationCode authorization)
    {
        Id = id;
        Status = status;
        CardNumberLastFour = cardNumberLastFour;
        ExpiryCardDate = expiryCardDate;
        Money = money;
        Authorization = authorization;
    }

    public static Payment CreateAuthorized(
        string cardNumber,
        ExpiryCardDate expiryCardDate,
        Money money,
        AuthorizationCode authorizationCode)
    {
        var cardLastFour = CardLastFourDigits.From(cardNumber);
        return new Payment(
            Guid.NewGuid(),
            PaymentStatus.Authorized,
            cardLastFour,
            expiryCardDate,
            money,
            authorizationCode);
    }

    public static Payment CreateDeclined(
        string cardNumber,
        ExpiryCardDate expiryCardDate,
        Money money,
        AuthorizationCode authorizationCode)
    {
        var cardLastFour = CardLastFourDigits.From(cardNumber);
        return new Payment(
            Guid.NewGuid(),
            PaymentStatus.Declined,
            cardLastFour,
            expiryCardDate,
            money,
            authorizationCode);
    }
}
