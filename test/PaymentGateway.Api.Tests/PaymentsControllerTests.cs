using MediatR;

using Moq;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Domain.Aggregates.PaymentAggregate;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    [Fact]
    public void PaymentsController_CanBeInstantiated()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();

        // Act
        var controller = new PaymentsController(mediatorMock.Object);

        // Assert
        Assert.NotNull(controller);
    }

    [Fact]
    public void PaymentsController_Constructor_ThrowsWhenMediatorIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new PaymentsController(null!));
    }

    [Fact]
    public void PostPaymentRequest_CanBeCreatedWithValidData()
    {
        // Arrange & Act
        var request = new CreatePaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Currency = "USD",
            Amount = 1000,
            Cvv = "123"
        };

        // Assert
        Assert.NotNull(request);
        Assert.Equal("4111111111111111", request.CardNumber);
        Assert.Equal(12, request.ExpiryMonth);
        Assert.Equal(2025, request.ExpiryYear);
        Assert.Equal("USD", request.Currency);
        Assert.Equal(1000, request.Amount);
        Assert.Equal("123", request.Cvv);
    }

    [Fact]
    public void Payment_CanBeCreatedAuthorizedWithValidData()
    {
        // Arrange
        var cardNumber = "4111111111111111";
        var expiryMonth = 12;
        var expiryYear = 2025;
        var currency = "USD";
        var amount = 1000;
        var money = Money.From(amount, currency);

        // Act
        var payment = Payment.CreateAuthorized(cardNumber, expiryMonth, expiryYear, money);

        // Assert
        Assert.NotEqual(Guid.Empty, payment.Id);
        Assert.Equal(PaymentStatus.Authorized, payment.Status);
        Assert.Equal("1111", payment.CardNumberLastFour.Value);
        Assert.Equal(expiryMonth, payment.ExpiryMonth);
        Assert.Equal(expiryYear, payment.ExpiryYear);
        Assert.Equal(currency, payment.Money.Currency);
        Assert.Equal(amount, payment.Money.Amount);
    }

    [Fact]
    public void Payment_CanBeCreatedDeclinedWithValidData()
    {
        // Arrange
        var cardNumber = "5555555555554444";
        var expiryMonth = 6;
        var expiryYear = 2027;
        var currency = "EUR";
        var amount = 5000;
        var money = Money.From(amount, currency);

        // Act
        var payment = Payment.CreateDeclined(cardNumber, expiryMonth, expiryYear, money);

        // Assert
        Assert.NotEqual(Guid.Empty, payment.Id);
        Assert.Equal(PaymentStatus.Declined, payment.Status);
        Assert.Equal("4444", payment.CardNumberLastFour.Value);
        Assert.Equal(expiryMonth, payment.ExpiryMonth);
        Assert.Equal(expiryYear, payment.ExpiryYear);
        Assert.Equal(currency, payment.Money.Currency);
        Assert.Equal(amount, payment.Money.Amount);
    }

    [Fact]
    public void PaymentStatus_HasRequiredValues()
    {
        // Assert
        Assert.Equal(0, (int)PaymentStatus.Authorized);
        Assert.Equal(1, (int)PaymentStatus.Declined);
        Assert.Equal(2, (int)PaymentStatus.Rejected);
    }
}
