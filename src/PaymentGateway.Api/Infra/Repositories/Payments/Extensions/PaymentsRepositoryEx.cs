using PaymentGateway.Api.Domain.Aggregates.PaymentAggregate;

namespace PaymentGateway.Api.Infra.Repositories.Payments.Extensions;

internal static class PaymentsRepositoryEx
{
    public static PaymentsData ToData(this Payment payment)
    {
        var data = new PaymentsData
        {
            Id = payment.Id,
            CardNumberLastFour = payment.CardNumberLastFour.Value,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Status = payment.Status.ToString(),
            Amount = payment.Money.Amount,
            Currency = payment.Money.Currency
        };
        
        return data;    
    }

    public static Payment ToEntity(this PaymentsData paymentsData)
    {
        var money = Money.From(paymentsData.Amount, paymentsData.Currency);
        var cardLastFourDigits = CardLastFourDigits.FromDataBase(paymentsData.CardNumberLastFour);

        var status = Enum.Parse<PaymentStatus>(paymentsData.Status);

        var payment = new Payment(
            paymentsData.Id,
            status,
            cardLastFourDigits,
            paymentsData.ExpiryMonth,
            paymentsData.ExpiryYear,
            money
        );

        return payment;
    }
}