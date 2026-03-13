namespace PaymentGateway.Api.Services;

using System.Collections.Concurrent;
using CSharpFunctionalExtensions;
using Domain.Entities;
using Interfaces;

public class PaymentsRepository : IPaymentsRepository
{
    private readonly ConcurrentDictionary<Guid, Payment> _payments = new();

    public void Add(Payment payment)
    {
        _payments.TryAdd(payment.Id, payment);
    }

    public Maybe<Payment> GetById(Guid id)
    {
        return _payments.TryGetValue(id, out var payment)
            ? Maybe<Payment>.From(payment)
            : Maybe<Payment>.None;
    }
}
