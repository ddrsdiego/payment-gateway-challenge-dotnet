using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Moq;

using PaymentGateway.Api.Application.UseCases.ProcessPayment;
using PaymentGateway.Api.Domain.Aggregates.PaymentAggregate;
using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Interfaces;
using PaymentGateway.Api.Models.Bank;

namespace PaymentGateway.Api.Tests;

public class ProcessPaymentCommandHandlerTests
{
    private readonly Mock<IBankSimulatorClient> _bankClientMock;
    private readonly Mock<IPaymentsRepository> _repositoryMock;
    private readonly Mock<ILogger<ProcessPaymentCommandHandler>> _loggerMock;
    private readonly ProcessPaymentCommandHandler _handler;

    public ProcessPaymentCommandHandlerTests()
    {
        _bankClientMock = new Mock<IBankSimulatorClient>(MockBehavior.Loose);
        _repositoryMock = new Mock<IPaymentsRepository>(MockBehavior.Loose);
        _loggerMock = new Mock<ILogger<ProcessPaymentCommandHandler>>(MockBehavior.Loose);

        _handler = new ProcessPaymentCommandHandler(
            _bankClientMock.Object,
            _repositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenBankClientIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ProcessPaymentCommandHandler(null!, _repositoryMock.Object, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenRepositoryIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ProcessPaymentCommandHandler(_bankClientMock.Object, null!, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ProcessPaymentCommandHandler(_bankClientMock.Object, _repositoryMock.Object, null!));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PHASE 1 - INPUT VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ShouldReturnError_WhenCardNumberIsNull()
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            null!,
            12,
            futureYear,
            "USD",
            1000,
            "123");

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(!response.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenCardNumberIsNotNumeric()
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "411111111111ABCD",
            12,
            futureYear,
            "USD",
            1000,
            "123");

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(!response.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(13)]
    public async Task Handle_ShouldReturnError_WhenExpiryMonthIsInvalid(int month)
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "4111111111111111",
            month,
            futureYear,
            "USD",
            1000,
            "123");

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(!response.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenCardIsExpiredPreviousYear()
    {
        // Arrange
        var previousYear = DateTime.Now.Year - 1;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "4111111111111111",
            12,
            previousYear,
            "USD",
            1000,
            "123");

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(!response.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenCvvIsNull()
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "4111111111111111",
            12,
            futureYear,
            "USD",
            1000,
            null!);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(!response.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("12")]
    [InlineData("12345")]
    public async Task Handle_ShouldReturnError_WhenCvvHasInvalidLength(string cvv)
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "4111111111111111",
            12,
            futureYear,
            "USD",
            1000,
            cvv);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(!response.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenCvvIsNotNumeric()
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "4111111111111111",
            12,
            futureYear,
            "USD",
            1000,
            "12A");

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(!response.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task Handle_ShouldReturnError_WhenAmountIsInvalid(int amount)
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "4111111111111111",
            12,
            futureYear,
            "USD",
            amount,
            "123");

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(!response.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenCurrencyIsInvalid()
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "4111111111111111",
            12,
            futureYear,
            "BRL",
            1000,
            "123");

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(!response.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PHASE 2 - EXTERNAL SERVICE INTERACTION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ShouldCallBankSimulator_WithValidRequest()
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "4111111111111111",
            12,
            futureYear,
            "USD",
            1000,
            "123");

        var bankResponse = new BankPaymentResponse(true, "AUTH123");
        _bankClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bankResponse);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _bankClientMock.Verify(
            x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenBankSimulatorThrowsBadRequestException()
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "4111111111111111",
            12,
            futureYear,
            "USD",
            1000,
            "123");

        _bankClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BankSimulatorBadRequestException("Invalid card"));

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(!response.IsSuccess);
        Assert.Equal(StatusCodes.Status500InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenBankSimulatorThrowsException()
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "4111111111111111",
            12,
            futureYear,
            "USD",
            1000,
            "123");

        _bankClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BankSimulatorException("Service unavailable"));

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(!response.IsSuccess);
        Assert.Equal(StatusCodes.Status500InternalServerError, response.StatusCode);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PHASE 3 - HAPPY PATH & PERSISTENCE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenPaymentIsAuthorized()
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "4111111111111111",
            12,
            futureYear,
            "USD",
            1000,
            "123");

        var bankResponse = new BankPaymentResponse(true, "AUTH123");
        _bankClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bankResponse);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(StatusCodes.Status201Created, response.StatusCode);
        _repositoryMock.Verify(x => x.Add(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenPaymentIsDeclined()
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "4111111111111111",
            12,
            futureYear,
            "USD",
            1000,
            "123");

        var bankResponse = new BankPaymentResponse(false, "DECLINED");
        _bankClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bankResponse);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(StatusCodes.Status201Created, response.StatusCode);
        _repositoryMock.Verify(x => x.Add(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPersistPaymentWithCorrectData_WhenAuthorized()
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "4111111111111111",
            12,
            futureYear,
            "USD",
            1000,
            "123");

        var bankResponse = new BankPaymentResponse(true, "AUTH123");
        _bankClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bankResponse);

        Payment? capturedPayment = null;
        _repositoryMock
            .Setup(x => x.Add(It.IsAny<Payment>()))
            .Callback<Payment>(p => capturedPayment = p);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedPayment);
        Assert.NotEqual(Guid.Empty, capturedPayment.Id);
        Assert.Equal(PaymentStatus.Authorized, capturedPayment.Status);
        Assert.Equal("1111", capturedPayment.CardNumberLastFour.Value);
        Assert.Equal(12, capturedPayment.ExpiryCardDate.Month);
        Assert.Equal(futureYear, capturedPayment.ExpiryCardDate.Year);
        Assert.Equal("USD", capturedPayment.Money.Currency);
        Assert.Equal(1000, capturedPayment.Money.Amount);
    }

    [Theory]
    [InlineData("USD", 1000)]
    [InlineData("EUR", 5000)]
    [InlineData("GBP", 2500)]
    public async Task Handle_ShouldProcessPayment_WithVariousCurrenciesAndAmounts(string currency, int amount)
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "4111111111111111",
            12,
            futureYear,
            currency,
            amount,
            "123");

        var bankResponse = new BankPaymentResponse(true, "AUTH123");
        _bankClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bankResponse);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(response.IsSuccess);
        _repositoryMock.Verify(x => x.Add(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithPaymentData_WhenAuthorized()
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "4111111111111111",
            12,
            futureYear,
            "USD",
            1000,
            "123");

        var bankResponse = new BankPaymentResponse(true, "AUTH123");
        _bankClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bankResponse);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenRepositoryThrowsException()
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "4111111111111111",
            12,
            futureYear,
            "USD",
            1000,
            "123");

        var bankResponse = new BankPaymentResponse(true, "AUTH123");
        _bankClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bankResponse);

        _repositoryMock
            .Setup(x => x.Add(It.IsAny<Payment>()))
            .Throws(new Exception("Database error"));

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(!response.IsSuccess);
        Assert.Equal(StatusCodes.Status500InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task Handle_ShouldCreateAuthorizedPayment_WithCorrectCardLastFour()
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "5555555555554444",
            6,
            futureYear,
            "EUR",
            5000,
            "456");

        var bankResponse = new BankPaymentResponse(true, "AUTH456");
        _bankClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bankResponse);

        Payment? capturedPayment = null;
        _repositoryMock
            .Setup(x => x.Add(It.IsAny<Payment>()))
            .Callback<Payment>(p => capturedPayment = p);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.NotNull(capturedPayment);
        Assert.Equal("4444", capturedPayment.CardNumberLastFour.Value);
        Assert.Equal(PaymentStatus.Authorized, capturedPayment.Status);
    }

    [Fact]
    public async Task Handle_ShouldCreateDeclinedPayment_WhenBankRejectsPayment()
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2;
        var command = new ProcessPaymentCommand(
            "trace-123",
            "4111111111111111",
            12,
            futureYear,
            "USD",
            1000,
            "123");

        var bankResponse = new BankPaymentResponse(false, "DECLINED_INSUFFICIENT_FUNDS");
        _bankClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bankResponse);

        Payment? capturedPayment = null;
        _repositoryMock
            .Setup(x => x.Add(It.IsAny<Payment>()))
            .Callback<Payment>(p => capturedPayment = p);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.NotNull(capturedPayment);
        Assert.Equal(PaymentStatus.Declined, capturedPayment.Status);
    }
}