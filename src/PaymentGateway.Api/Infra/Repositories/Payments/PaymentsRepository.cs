using System.Collections.Concurrent;

using CSharpFunctionalExtensions;

using PaymentGateway.Api.Domain.Aggregates.PaymentAggregate;
using PaymentGateway.Api.Infra.Repositories.Payments.Extensions;

namespace PaymentGateway.Api.Infra.Repositories.Payments;

public class PaymentsRepository : IPaymentsRepository
{
    private readonly ConcurrentDictionary<Guid, PaymentsData> _store = new();

    public void Add(Payment payment)
    {
        var data = payment.ToData();
        _store.TryAdd(data.Id, data);
    }

    public Maybe<Payment> GetById(Guid id)
    {
        return !_store.TryGetValue(id, out var paymentsData) 
            ? Maybe<Payment>.None 
            : Maybe<Payment>.From(paymentsData.ToEntity());
    }
}