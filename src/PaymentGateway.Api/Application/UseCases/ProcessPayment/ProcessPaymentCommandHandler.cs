using CSharpFunctionalExtensions;

using MediatR;

using PaymentGateway.Api.Application.Utils;
using PaymentGateway.Api.Domain.Entities;
using PaymentGateway.Api.Domain.ValueObjects;
using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Interfaces;
using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Application.UseCases.ProcessPayment;


public sealed class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Response>
{

    private readonly IBankSimulatorClient _bankSimulatorClient;
    private readonly IPaymentsRepository _paymentsRepository;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;

    public ProcessPaymentCommandHandler(
        IBankSimulatorClient bankSimulatorClient,
        IPaymentsRepository paymentsRepository,
        ILogger<ProcessPaymentCommandHandler> logger)
    {
        _bankSimulatorClient = bankSimulatorClient ?? throw new ArgumentNullException(nameof(bankSimulatorClient));
        _paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Response> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        ProcessPaymentLog.ProcessingPayment(_logger, request.TracerId);

        var validationError = ValidatePaymentInputs(request);
        if (validationError.IsFailure)
            return validationError.Error;

        var moneyResult = CreateMoney(request);
        if (moneyResult.IsFailure)
            return moneyResult.Error;

        try
        {
            var bankAuthorizationResult = await GetBankAuthorizationAsync(request, moneyResult.Value, cancellationToken);
            if (bankAuthorizationResult.IsFailure)
                return bankAuthorizationResult.Error;

            var persistResult = PersistPayment(request, moneyResult.Value, bankAuthorizationResult.Value);
            if (persistResult.IsFailure)
                return persistResult.Error;

            ProcessPaymentLog.PaymentProcessed(_logger, request.TracerId);
            return persistResult.Value;
        }
        catch (Exception ex)
        {
            ProcessPaymentLog.UnexpectedError(_logger, request.TracerId, ex.Message);
            return ProcessPaymentErrors.UnexpectedError(request.TracerId, ex.Message);
        }
    }

    private static Result<Result, Response> ValidatePaymentInputs(ProcessPaymentCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.CardNumber) || request.CardNumber.Length < 14 || request.CardNumber.Length > 19)
            return ProcessPaymentErrors.InvalidCardNumber(request.TracerId);

        if (!request.CardNumber.All(char.IsDigit))
            return ProcessPaymentErrors.CardNumberNotNumeric(request.TracerId);

        if (request.ExpiryMonth is < 1 or > 12)
            return ProcessPaymentErrors.InvalidExpiryMonth(request.TracerId);

        if (request.ExpiryYear < DateTime.Now.Year ||
            (request.ExpiryYear == DateTime.Now.Year && request.ExpiryMonth < DateTime.Now.Month))
            return ProcessPaymentErrors.CardExpired(request.TracerId);

        if (string.IsNullOrWhiteSpace(request.Cvv) 
            || request.Cvv.Length < 3 
            || request.Cvv.Length > 4 
            || !request.Cvv.All(char.IsDigit))
            return ProcessPaymentErrors.InvalidCvv(request.TracerId);

        return Result.Success<Result, Response>(Result.Success());
    }

    private static Result<Money, Response> CreateMoney(ProcessPaymentCommand request)
    {
        try
        {
            var money = Money.From(request.Amount, request.Currency);
            return Result.Success<Money, Response>(money);
        }
        catch (ArgumentException ex) when (ex.ParamName == "amount")
        {
            return Result.Failure<Money, Response>(
                ProcessPaymentErrors.InvalidAmount(request.TracerId));
        }
        catch (ArgumentException ex) when (ex.ParamName == "currency")
        {
            return Result.Failure<Money, Response>(
                ProcessPaymentErrors.InvalidCurrency(request.TracerId));
        }
    }

    private async Task<Result<BankPaymentResponse, Response>> GetBankAuthorizationAsync(
        ProcessPaymentCommand request,
        Money money,
        CancellationToken cancellationToken)
    {
        try
        {
            var bankRequest = new BankPaymentRequest
            {
                CardNumber = request.CardNumber,
                ExpiryDate = $"{request.ExpiryMonth:D2}/{request.ExpiryYear}",
                Currency = money.Currency,
                Amount = money.Amount,
                Cvv = request.Cvv
            };

            var bankResponse = await _bankSimulatorClient.ProcessPaymentAsync(bankRequest, cancellationToken);
            return Result.Success<BankPaymentResponse, Response>(bankResponse);
        }
        catch (BankSimulatorBadRequestException ex)
        {
            ProcessPaymentLog.BankSimulatorBadRequest(_logger, request.TracerId, ex.Message);
            return Result.Failure<BankPaymentResponse, Response>(
                ProcessPaymentErrors.BankSimulatorBadRequest(request.TracerId, ex.Message));
        }
        catch (BankSimulatorException ex)
        {
            ProcessPaymentLog.BankSimulatorFailed(_logger, request.TracerId, ex.Message);
            return Result.Failure<BankPaymentResponse, Response>(
                ProcessPaymentErrors.BankSimulatorUnavailable(request.TracerId, ex.Message));
        }
    }

    private Result<Response, Response> PersistPayment(
        ProcessPaymentCommand request,
        Money money,
        BankPaymentResponse bankResponse)
    {
        try
        {
            var payment = bankResponse.Authorized
                ? Payment.CreateAuthorized(request.CardNumber, request.ExpiryMonth, request.ExpiryYear, money)
                : Payment.CreateDeclined(request.CardNumber, request.ExpiryMonth, request.ExpiryYear, money);

            _paymentsRepository.Add(payment);

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

            return Result.Success<Response, Response>(Response.Created(response));
        }
        catch (Exception ex)
        {
            ProcessPaymentLog.UnexpectedError(_logger, request.TracerId, ex.Message);
            return Result.Failure<Response, Response>(
                ProcessPaymentErrors.UnexpectedError(request.TracerId, ex.Message));
        }
    }
}
