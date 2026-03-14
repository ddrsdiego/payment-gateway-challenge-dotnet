using System.Net;

namespace PaymentGateway.Api.Services;

using System.Text.Json;
using Exceptions;
using Interfaces;
using Models.Bank;

public class BankSimulatorClient : IBankSimulatorClient
{
    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;

    public BankSimulatorClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<BankPaymentResponse> ProcessPaymentAsync(BankPaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("BankSimulator");

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync("/payments", jsonContent, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var bankResponse = JsonSerializer.Deserialize<BankPaymentResponse>(
                    responseContent,
                    DefaultJsonSerializerOptions);

                return bankResponse!;
            }

            if (response.StatusCode is >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError)
                throw new BankSimulatorBadRequestException("Bank simulator returned a bad request error");

            throw new BankSimulatorException("Bank simulator service is unavailable");
        }
        catch (HttpRequestException ex)
        {
            throw new BankSimulatorException("Bank simulator service is unavailable", ex);
        }
    }
}
