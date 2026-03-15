using MediatR;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Application.UseCases.GetPayment;
using PaymentGateway.Api.Controllers.Extensions;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost]
    public async Task<ActionResult<CreatePaymentResponse>> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request.ToCommand(), cancellationToken);
        return StatusCode(response.StatusCode, response.IsSuccess ? response.Data : response.ErrorContent!.ErrorResponse);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CreatePaymentResponse>> GetPayment(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetPaymentQuery(id), cancellationToken);
        return StatusCode(response.StatusCode, response.IsSuccess ? response.Data : response.ErrorContent!.ErrorResponse);
    }
}
