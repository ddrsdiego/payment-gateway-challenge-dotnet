namespace PaymentGateway.Api.Interfaces;

using CSharpFunctionalExtensions;
using Domain.Entities;

public interface IPaymentsRepository
{
    void Add(Payment payment);
    Maybe<Payment> GetById(Guid id);
}
