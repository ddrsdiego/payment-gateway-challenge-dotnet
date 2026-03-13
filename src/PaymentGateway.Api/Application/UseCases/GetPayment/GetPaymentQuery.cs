namespace PaymentGateway.Api.Application.UseCases.GetPayment;

using MediatR;
using PaymentGateway.Api.Application.Utils;

public record GetPaymentQuery(Guid PaymentId) : IRequest<Response>;
