namespace PaymentGateway.Api.Models.Bank;

using System.Text.Json.Serialization;

public class BankPaymentResponse
{
    public bool Authorized { get; set; }

    [JsonPropertyName("authorization_code")]
    public string AuthorizationCode { get; set; }
}
