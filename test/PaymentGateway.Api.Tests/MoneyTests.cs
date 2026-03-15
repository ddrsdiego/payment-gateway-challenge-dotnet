using PaymentGateway.Api.Domain.Aggregates.PaymentAggregate;

namespace PaymentGateway.Api.Tests;

public class MoneyTests
{
    [Fact]
    public void From_ShouldCreateMoney_WhenAmountAndCurrencyAreValid()
    {
        // Arrange
        const int amount = 1000;
        const string currency = "USD";

        // Act
        var money = Money.From(amount, currency);

        // Assert
        Assert.NotNull(money);
        Assert.Equal(amount, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Theory]
    [InlineData("usd")]
    [InlineData("Usd")]
    [InlineData("UsD")]
    public void From_ShouldNormalizeCurrencyToUppercase_WhenCurrencyIsProvidedInLowercase(string currency)
    {
        // Arrange & Act
        var money = Money.From(1000, currency);

        // Assert
        Assert.Equal("USD", money.Currency);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    public void From_ShouldCreateMoney_WhenCurrencyIsSupported(string currency)
    {
        // Arrange & Act
        var money = Money.From(1000, currency);

        // Assert
        Assert.Equal(currency, money.Currency);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenAmountIsZero()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Money.From(0, "USD"));
        Assert.Equal("amount", exception.ParamName);
        Assert.Contains("Amount must be greater than 0", exception.Message);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenAmountIsNegative()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Money.From(-100, "USD"));
        Assert.Equal("amount", exception.ParamName);
        Assert.Contains("Amount must be greater than 0", exception.Message);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenCurrencyIsNull()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Money.From(1000, null!));
        Assert.Equal("currency", exception.ParamName);
        Assert.Contains("Currency must be one of: USD, EUR, GBP", exception.Message);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenCurrencyIsEmpty()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Money.From(1000, string.Empty));
        Assert.Equal("currency", exception.ParamName);
        Assert.Contains("Currency must be one of: USD, EUR, GBP", exception.Message);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenCurrencyIsWhitespace()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Money.From(1000, "   "));
        Assert.Equal("currency", exception.ParamName);
        Assert.Contains("Currency must be one of: USD, EUR, GBP", exception.Message);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenCurrencyHasTwoCharacters()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Money.From(1000, "US"));
        Assert.Equal("currency", exception.ParamName);
        Assert.Contains("Currency must be one of: USD, EUR, GBP", exception.Message);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenCurrencyHasFourCharacters()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Money.From(1000, "USDT"));
        Assert.Equal("currency", exception.ParamName);
        Assert.Contains("Currency must be one of: USD, EUR, GBP", exception.Message);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenCurrencyIsNotSupported()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Money.From(1000, "BRL"));
        Assert.Equal("currency", exception.ParamName);
        Assert.Contains("Currency must be one of: USD, EUR, GBP", exception.Message);
    }

    [Theory]
    [InlineData(1, "USD")]
    [InlineData(999999, "EUR")]
    [InlineData(int.MaxValue, "GBP")]
    public void From_ShouldCreateMoney_WithVariousValidAmounts(int amount, string currency)
    {
        // Act
        var money = Money.From(amount, currency);

        // Assert
        Assert.Equal(amount, money.Amount);
        Assert.Equal(currency, money.Currency);
    }
}
