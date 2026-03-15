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
            ExpiryMonth = payment.ExpiryCardDate.Month,
            ExpiryYear = payment.ExpiryCardDate.Year,
            Status = payment.Status.ToString(),
            Amount = payment.Money.Amount,
            Currency = payment.Money.Currency,
            AuthorizationCode = payment.Authorization.Code
        };

        return data;
    }

    public static Payment ToEntity(this PaymentsData paymentsData)
    {
        var money = Money.From(paymentsData.Amount, paymentsData.Currency);
        var cardLastFourDigits = CardLastFourDigits.FromDataBase(paymentsData.CardNumberLastFour);
        var expiryCardDate = ExpiryCardDate.From(paymentsData.ExpiryMonth, paymentsData.ExpiryYear);
        var authorizationCode = new AuthorizationCode(paymentsData.AuthorizationCode);
        
        var status = Enum.Parse<PaymentStatus>(paymentsData.Status);

        var payment = new Payment(
            paymentsData.Id,
            status,
            cardLastFourDigits,
            expiryCardDate,
            money,
            authorizationCode);

        return payment;
    }
}