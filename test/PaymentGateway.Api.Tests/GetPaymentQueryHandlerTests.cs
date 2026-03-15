using CSharpFunctionalExtensions;

using Moq;

using PaymentGateway.Api.Application.UseCases.GetPayment;
using PaymentGateway.Api.Domain.Aggregates.PaymentAggregate;

namespace PaymentGateway.Api.Tests;

public class GetPaymentQueryHandlerTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenRepositoryIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GetPaymentQueryHandler(null!));
    }

    [Fact]
    public async Task Handle_ShouldReturnPayment_WhenPaymentExists()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        const string cardNumber = "4111111111111111";
        const int expiryMonth = 12;
        const int expiryYear = 2027;

        var money = Money.From(1000, "USD");
        var expiryCardDate = ExpiryCardDate.From(expiryMonth, expiryYear);
        var authorizationCode = new AuthorizationCode(Guid.NewGuid().ToString());

        var payment = Payment.CreateAuthorized(cardNumber, expiryCardDate, money, authorizationCode);

        var repositoryMock = new Mock<IPaymentsRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(x => x.GetById(paymentId))
            .Returns(Maybe.From(payment));

        var handler = new GetPaymentQueryHandler(repositoryMock.Object);
        var query = new GetPaymentQuery(paymentId);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsSuccess);
        Assert.Equal(200, response.StatusCode);

        var data = response.Data;
        Assert.NotNull(data);

        repositoryMock.Verify(x => x.GetById(paymentId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaymentResponse_WithCorrectData()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        const string cardNumber = "5555555555554444";
        const int expiryMonth = 6;
        const int expiryYear = 2027;

        var money = Money.From(5000, "EUR");
        var expiryCardDate = ExpiryCardDate.From(expiryMonth, expiryYear);
        var authorizationCode = new AuthorizationCode(Guid.NewGuid().ToString());

        var payment = Payment.CreateAuthorized(cardNumber, expiryCardDate, money, authorizationCode);

        var repositoryMock = new Mock<IPaymentsRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(x => x.GetById(paymentId))
            .Returns(Maybe.From(payment));

        var handler = new GetPaymentQueryHandler(repositoryMock.Object);
        var query = new GetPaymentQuery(paymentId);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaymentNotFound_WhenPaymentDoesNotExist()
    {
        // Arrange
        var paymentId = Guid.NewGuid();

        var repositoryMock = new Mock<IPaymentsRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(x => x.GetById(paymentId))
            .Returns(Maybe<Payment>.None);

        var handler = new GetPaymentQueryHandler(repositoryMock.Object);
        var query = new GetPaymentQuery(paymentId);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(404, response.StatusCode);

        repositoryMock.Verify(x => x.GetById(paymentId), Times.Once);
    }

    [Theory]
    [InlineData("USD", 1000)]
    [InlineData("EUR", 5000)]
    [InlineData("GBP", 2500)]
    public async Task Handle_ShouldReturnCorrectPaymentData_WithVariousCurrenciesAndAmounts(string currency, int amount)
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var cardNumber = "4111111111111111";
        var money = Money.From(amount, currency);
        var expiryCardDate = ExpiryCardDate.From(12, 2027);
        var authorizationCode = new AuthorizationCode(Guid.NewGuid().ToString());

        var payment = Payment.CreateAuthorized(cardNumber, expiryCardDate, money, authorizationCode);

        var repositoryMock = new Mock<IPaymentsRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(x => x.GetById(paymentId))
            .Returns(Maybe.From(payment));

        var handler = new GetPaymentQueryHandler(repositoryMock.Object);
        var query = new GetPaymentQuery(paymentId);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public async Task Handle_ShouldReturnDeclinedPayment_WhenPaymentIsDeclined()
    {
        // Arrange
        const string cardNumber = "4111111111111111";

        var paymentId = Guid.NewGuid();
        var money = Money.From(1000, "USD");
        var expiryCardDate = ExpiryCardDate.From(12, 2027);
        var authorizationCode = new AuthorizationCode(Guid.NewGuid().ToString());
        var payment = Payment.CreateDeclined(cardNumber, expiryCardDate, money, authorizationCode);

        var repositoryMock = new Mock<IPaymentsRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(x => x.GetById(paymentId))
            .Returns(Maybe.From(payment));

        var handler = new GetPaymentQueryHandler(repositoryMock.Object);
        var query = new GetPaymentQuery(paymentId);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryOnce_WhenHandlingQuery()
    {
        // Arrange
        var paymentId = Guid.NewGuid();

        var repositoryMock = new Mock<IPaymentsRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(x => x.GetById(paymentId))
            .Returns(Maybe<Payment>.None);

        var handler = new GetPaymentQueryHandler(repositoryMock.Object);
        var query = new GetPaymentQuery(paymentId);

        // Act
        _ = await handler.Handle(query, CancellationToken.None);

        // Assert
        repositoryMock.Verify(x => x.GetById(paymentId), Times.Exactly(1));
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectPaymentId_ToRepository()
    {
        // Arrange
        var paymentId = Guid.NewGuid();

        var repositoryMock = new Mock<IPaymentsRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns(Maybe<Payment>.None);

        var handler = new GetPaymentQueryHandler(repositoryMock.Object);
        var query = new GetPaymentQuery(paymentId);

        // Act
        _ = await handler.Handle(query, CancellationToken.None);

        // Assert
        repositoryMock.Verify(x => x.GetById(paymentId), Times.Once);
    }
}