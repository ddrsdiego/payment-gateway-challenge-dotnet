namespace PaymentGateway.Api.Domain.Aggregates.PaymentAggregate;

public record struct AuthorizationCode
{
    public string Code { get; }

    public AuthorizationCode(string code) => Code = code;
}