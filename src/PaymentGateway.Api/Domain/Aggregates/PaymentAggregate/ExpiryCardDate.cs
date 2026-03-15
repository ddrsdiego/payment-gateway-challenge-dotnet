namespace PaymentGateway.Api.Domain.Aggregates.PaymentAggregate;

public readonly record struct ExpiryCardDate
{
    private ExpiryCardDate(int month, int year)
    {
        Month = month;
        Year = year;
    }

    public static ExpiryCardDate From(int expiryMonth, int expiryYear)
    {
        if (expiryMonth is < 1 or > 12)
            throw new ArgumentException("Expiry month must be between 1 and 12", nameof(expiryMonth));

        var currentYear = DateTime.UtcNow.Year;
        if (expiryYear < 2000)
            throw new ArgumentException("Expiry year must be a 4-digit year (e.g., 2026)", nameof(expiryYear));

        if (expiryYear > currentYear + 20)
            throw new ArgumentException("Expiry year exceeds the maximum allowed (max 20 years ahead)", nameof(expiryYear));

        return new ExpiryCardDate(expiryMonth, expiryYear);
    }
    
    public int Month { get; }
    public int Year { get; }
    public string ExpiryDate => $"{Month:D2}/{Year}";

    public bool IsExpired
    {
        get
        {
            var now = DateTime.UtcNow;
            var cardExpiry = new DateOnly(Year, Month, 1);
            var reference = new DateOnly(now.Year, now.Month, 1);
            return cardExpiry < reference;
        }
    }
}
