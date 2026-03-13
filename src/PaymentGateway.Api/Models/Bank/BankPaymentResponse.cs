namespace PaymentGateway.Api.Models.Bank;

using System.Text.Json.Serialization;

public record BankPaymentResponse(
    bool Authorized,
    [property: JsonPropertyName("authorization_code")] string AuthorizationCode);
