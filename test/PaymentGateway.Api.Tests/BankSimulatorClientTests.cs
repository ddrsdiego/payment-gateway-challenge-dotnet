using System.Net;
using System.Text.Json;

using Moq;
using Moq.Protected;

using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class BankSimulatorClientTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenFactoryIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BankSimulatorClient(null!));
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldReturnBankPaymentResponse_WhenSuccessful()
    {
        // Arrange
        var request = new BankPaymentRequest("4111111111111111", "12/2025", "USD", 1000, "123");
        var bankResponse = new BankPaymentResponse(true, "AUTH123");
        var jsonResponse = JsonSerializer.Serialize(bankResponse);

        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            });

        var client = new HttpClient(handler.Object) { BaseAddress = new Uri("http://localhost:3000") };
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock
            .Setup(x => x.CreateClient("BankSimulator"))
            .Returns(client);

        var bankClient = new BankSimulatorClient(httpFactoryMock.Object);

        // Act
        var response = await bankClient.ProcessPaymentAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Authorized);
        Assert.Equal("AUTH123", response.AuthorizationCode);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldReturnBankPaymentResponse_WhenDeclined()
    {
        // Arrange
        var request = new BankPaymentRequest("5555555555554444", "06/2027", "EUR", 5000, "456");
        var bankResponse = new BankPaymentResponse(false, "DECLINED");
        var jsonResponse = JsonSerializer.Serialize(bankResponse);

        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            });

        var client = new HttpClient(handler.Object) { BaseAddress = new Uri("http://localhost:3000") };
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock
            .Setup(x => x.CreateClient("BankSimulator"))
            .Returns(client);

        var bankClient = new BankSimulatorClient(httpFactoryMock.Object);

        // Act
        var response = await bankClient.ProcessPaymentAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Authorized);
        Assert.Equal("DECLINED", response.AuthorizationCode);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrowBankSimulatorBadRequestException_WhenStatusCode400()
    {
        // Arrange
        var request = new BankPaymentRequest("invalid", "12/2025", "USD", 1000, "123");

        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest });

        var client = new HttpClient(handler.Object) { BaseAddress = new Uri("http://localhost:3000") };
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock
            .Setup(x => x.CreateClient("BankSimulator"))
            .Returns(client);

        var bankClient = new BankSimulatorClient(httpFactoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<BankSimulatorBadRequestException>(
            () => bankClient.ProcessPaymentAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrowBankSimulatorBadRequestException_WhenStatusCode401()
    {
        // Arrange
        var request = new BankPaymentRequest("4111111111111111", "12/2025", "USD", 1000, "123");

        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized });

        var client = new HttpClient(handler.Object) { BaseAddress = new Uri("http://localhost:3000") };
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock
            .Setup(x => x.CreateClient("BankSimulator"))
            .Returns(client);

        var bankClient = new BankSimulatorClient(httpFactoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<BankSimulatorBadRequestException>(
            () => bankClient.ProcessPaymentAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrowBankSimulatorException_WhenStatusCode500()
    {
        // Arrange
        var request = new BankPaymentRequest("4111111111111111", "12/2025", "USD", 1000, "123");

        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

        var client = new HttpClient(handler.Object) { BaseAddress = new Uri("http://localhost:3000") };
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock
            .Setup(x => x.CreateClient("BankSimulator"))
            .Returns(client);

        var bankClient = new BankSimulatorClient(httpFactoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<BankSimulatorException>(
            () => bankClient.ProcessPaymentAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrowBankSimulatorException_WhenHttpRequestExceptionThrown()
    {
        // Arrange
        var request = new BankPaymentRequest("4111111111111111", "12/2025", "USD", 1000, "123");

        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection timeout"));

        var client = new HttpClient(handler.Object) { BaseAddress = new Uri("http://localhost:3000") };
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock
            .Setup(x => x.CreateClient("BankSimulator"))
            .Returns(client);

        var bankClient = new BankSimulatorClient(httpFactoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<BankSimulatorException>(
            () => bankClient.ProcessPaymentAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldPostToCorrectEndpoint()
    {
        // Arrange
        var request = new BankPaymentRequest("4111111111111111", "12/2025", "USD", 1000, "123");
        var bankResponse = new BankPaymentResponse(true, "AUTH123");
        var jsonResponse = JsonSerializer.Serialize(bankResponse);

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(msg =>
                    msg.Method == HttpMethod.Post &&
                    msg.RequestUri!.PathAndQuery == "/payments"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            });

        var client = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost:3000") };
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock
            .Setup(x => x.CreateClient("BankSimulator"))
            .Returns(client);

        var bankClient = new BankSimulatorClient(httpFactoryMock.Object);

        // Act
        var response = await bankClient.ProcessPaymentAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(msg =>
                msg.Method == HttpMethod.Post &&
                msg.RequestUri!.PathAndQuery == "/payments"),
            ItExpr.IsAny<CancellationToken>());
    }
}
