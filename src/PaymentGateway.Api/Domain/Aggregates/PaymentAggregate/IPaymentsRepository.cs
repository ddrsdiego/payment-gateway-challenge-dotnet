using CSharpFunctionalExtensions;

namespace PaymentGateway.Api.Domain.Aggregates.PaymentAggregate;

public interface IPaymentsRepository
{
    void Add(Payment payment);
    Maybe<Payment> GetById(Guid id);
}
