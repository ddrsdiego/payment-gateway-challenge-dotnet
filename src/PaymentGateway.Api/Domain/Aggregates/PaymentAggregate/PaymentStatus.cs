namespace PaymentGateway.Api.Domain.Aggregates.PaymentAggregate;

public enum PaymentStatus
{
    Authorized,
    Declined,
    Rejected
}