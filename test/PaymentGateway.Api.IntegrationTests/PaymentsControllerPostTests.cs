namespace PaymentGateway.Api.IntegrationTests;

using Moq;
using Xunit;

public class PaymentsControllerPostTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private PaymentGatewayWebApplicationFactory _factory = null!;
    private HttpClient _httpClient = null!;

    public async Task InitializeAsync()
    {
        _factory = new PaymentGatewayWebApplicationFactory();
        _httpClient = _factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _httpClient?.Dispose();
        await _factory.DisposeAsync();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task CreatePayment_ShouldReturn201Created_WhenPaymentIsAuthorized()
    {
        // Arrange
        _factory.BankSimulatorClientMock.Reset();

        var request = new PostPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = 12,
            ExpiryYear = 2027,
            Currency = "USD",
            Amount = 1000,
            Cvv = "123"
        };

        var bankResponse = new BankPaymentResponse { Authorized = true, AuthorizationCode = "AUTH123" };
        _factory.BankSimulatorClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bankResponse);

        var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("/api/payments", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(responseContent, JsonOptions);
        Assert.NotNull(paymentResponse);
        Assert.Equal("Authorized", paymentResponse.Status.ToString());
    }

    [Fact]
    public async Task CreatePayment_ShouldReturn201Created_WhenPaymentIsDeclined()
    {
        // Arrange
        _factory.BankSimulatorClientMock.Reset();

        var request = new PostPaymentRequest
        {
            CardNumber = "5555555555554444",
            ExpiryMonth = 6,
            ExpiryYear = 2027,
            Currency = "EUR",
            Amount = 5000,
            Cvv = "456"
        };

        var bankResponse = new BankPaymentResponse { Authorized = false, AuthorizationCode = "DECLINED" };
        _factory.BankSimulatorClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bankResponse);

        var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("/api/payments", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(responseContent, JsonOptions);
        Assert.NotNull(paymentResponse);
        Assert.Equal("Declined", paymentResponse.Status.ToString());
    }

    [Fact]
    public async Task CreatePayment_ShouldReturn400BadRequest_WhenCardNumberIsTooShort()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "123456789",
            ExpiryMonth = 12,
            ExpiryYear = 2027,
            Currency = "USD",
            Amount = 1000,
            Cvv = "123"
        };

        var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("/api/payments", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_ShouldReturn400BadRequest_WhenCardNumberIsTooLong()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "12345678901234567890",
            ExpiryMonth = 12,
            ExpiryYear = 2027,
            Currency = "USD",
            Amount = 1000,
            Cvv = "123"
        };

        var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("/api/payments", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_ShouldReturn400BadRequest_WhenCardNumberIsNotNumeric()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "411111111111111X",
            ExpiryMonth = 12,
            ExpiryYear = 2027,
            Currency = "USD",
            Amount = 1000,
            Cvv = "123"
        };

        var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("/api/payments", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_ShouldReturn400BadRequest_WhenExpiryMonthIsInvalid()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = 13,
            ExpiryYear = 2027,
            Currency = "USD",
            Amount = 1000,
            Cvv = "123"
        };

        var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("/api/payments", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_ShouldReturn400BadRequest_WhenCardIsExpired()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = 1,
            ExpiryYear = 2020,
            Currency = "USD",
            Amount = 1000,
            Cvv = "123"
        };

        var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("/api/payments", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_ShouldReturn400BadRequest_WhenCvvIsInvalid()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = 12,
            ExpiryYear = 2027,
            Currency = "USD",
            Amount = 1000,
            Cvv = "12"
        };

        var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("/api/payments", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_ShouldReturn400BadRequest_WhenAmountIsInvalid()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = 12,
            ExpiryYear = 2027,
            Currency = "USD",
            Amount = 0,
            Cvv = "123"
        };

        var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("/api/payments", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_ShouldReturn400BadRequest_WhenCurrencyIsInvalid()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = 12,
            ExpiryYear = 2027,
            Currency = "INVALID",
            Amount = 1000,
            Cvv = "123"
        };

        var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("/api/payments", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_ShouldReturn500InternalServerError_WhenBankSimulatorThrowsException()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = 12,
            ExpiryYear = 2027,
            Currency = "USD",
            Amount = 1000,
            Cvv = "123"
        };

        _factory.BankSimulatorClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Bank simulator unavailable"));

        var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("/api/payments", content);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
