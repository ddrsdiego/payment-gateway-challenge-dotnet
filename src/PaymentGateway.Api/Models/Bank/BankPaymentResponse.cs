using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Bank;

public record BankPaymentResponse(
    bool Authorized,
    [property: JsonPropertyName("authorization_code")] string AuthorizationCode);
