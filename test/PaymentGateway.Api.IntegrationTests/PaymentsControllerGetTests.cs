namespace PaymentGateway.Api.IntegrationTests;

using Moq;
using Xunit;

public class PaymentsControllerGetTests : IAsyncLifetime
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
    public async Task GetPayment_ShouldReturn200OK_WhenPaymentExists()
    {
        // Arrange
        var createRequest = new PostPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = 12,
            ExpiryYear = 2027,
            Currency = "USD",
            Amount = 1000,
            Cvv = "123"
        };

        var bankResponse = new BankPaymentResponse(true, "AUTH123");
        _factory.BankSimulatorClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bankResponse);

        var createContent = new StringContent(JsonSerializer.Serialize(createRequest), System.Text.Encoding.UTF8, "application/json");
        var createResponse = await _httpClient.PostAsync("/api/payments", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdPayment = JsonSerializer.Deserialize<PaymentResponse>(createResponseContent, JsonOptions);

        Assert.NotNull(createdPayment);
        var paymentId = createdPayment.Id;

        // Act
        var getResponse = await _httpClient.GetAsync($"/api/payments/{paymentId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var getResponseContent = await getResponse.Content.ReadAsStringAsync();
        var retrievedPayment = JsonSerializer.Deserialize<PaymentResponse>(getResponseContent, JsonOptions);
        Assert.NotNull(retrievedPayment);
        Assert.Equal(paymentId, retrievedPayment.Id);
        Assert.Equal("Authorized", retrievedPayment.Status.ToString());
    }

    [Fact]
    public async Task GetPayment_ShouldReturn404NotFound_WhenPaymentDoesNotExist()
    {
        // Arrange
        var nonExistentPaymentId = Guid.NewGuid();

        // Act
        var response = await _httpClient.GetAsync($"/api/payments/{nonExistentPaymentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
