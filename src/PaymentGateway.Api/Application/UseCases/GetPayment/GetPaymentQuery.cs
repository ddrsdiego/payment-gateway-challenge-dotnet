using MediatR;

using PaymentGateway.Api.Application.Utils;

namespace PaymentGateway.Api.Application.UseCases.GetPayment;

public record GetPaymentQuery(Guid PaymentId) : IRequest<Response>;
