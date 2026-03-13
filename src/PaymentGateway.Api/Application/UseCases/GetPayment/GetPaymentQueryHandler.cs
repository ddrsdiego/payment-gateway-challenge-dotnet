namespace PaymentGateway.Api.Application.UseCases.GetPayment;

using MediatR;
using PaymentGateway.Api.Application.Utils;
using PaymentGateway.Api.Interfaces;
using PaymentGateway.Api.Models.Responses;

public sealed class GetPaymentQueryHandler : IRequestHandler<GetPaymentQuery, Response>
{
    private readonly IPaymentsRepository _paymentsRepository;

    public GetPaymentQueryHandler(IPaymentsRepository paymentsRepository)
    {
        _paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
    }

    public Task<Response> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        var paymentMaybe = _paymentsRepository.GetById(request.PaymentId);

        if (paymentMaybe.HasNoValue)
            return Task.FromResult(GetPaymentErrors.PaymentNotFound(request.PaymentId));

        var payment = paymentMaybe.Value;
        var response = new PaymentResponse
        {
            Id = payment.Id,
            Status = payment.Status,
            CardNumberLastFour = payment.CardNumberLastFour.Value,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Currency = payment.Money.Currency,
            Amount = payment.Money.Amount
        };

        return Task.FromResult(Response.Ok(response));
    }
}
