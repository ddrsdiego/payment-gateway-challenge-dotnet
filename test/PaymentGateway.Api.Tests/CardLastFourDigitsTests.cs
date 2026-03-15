using PaymentGateway.Api.Domain.Aggregates.PaymentAggregate;

namespace PaymentGateway.Api.Tests;

public class CardLastFourDigitsTests
{
    [Fact]
    public void From_ShouldExtractLastFourDigits_WhenCardNumberHas16Digits()
    {
        // Arrange
        const string cardNumber = "4111111111111111";

        // Act
        var cardLastFour = CardLastFourDigits.From(cardNumber);

        // Assert
        Assert.NotNull(cardLastFour);
        Assert.Equal("1111", cardLastFour.Value);
    }

    [Fact]
    public void From_ShouldExtractLastFourDigits_WhenCardNumberHas19Digits()
    {
        // Arrange
        const string cardNumber = "5555555555554444";

        // Act
        var cardLastFour = CardLastFourDigits.From(cardNumber);

        // Assert
        Assert.NotNull(cardLastFour);
        Assert.Equal("4444", cardLastFour.Value);
    }

    [Fact]
    public void From_ShouldExtractLastFourDigits_WhenCardNumberHas14Digits()
    {
        // Arrange
        const string cardNumber = "36227206271667";

        // Act
        var cardLastFour = CardLastFourDigits.From(cardNumber);

        // Assert
        Assert.NotNull(cardLastFour);
        Assert.Equal("1667", cardLastFour.Value);
    }

    [Fact]
    public void From_ShouldExtractLastFourDigits_WithLeadingZeros()
    {
        // Arrange
        const string cardNumber = "4111111100001234";

        // Act
        var cardLastFour = CardLastFourDigits.From(cardNumber);

        // Assert
        Assert.NotNull(cardLastFour);
        Assert.Equal("1234", cardLastFour.Value);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenCardNumberIsNull()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => CardLastFourDigits.From(null!));
        Assert.Equal("cardNumber", exception.ParamName);
        Assert.Contains("Card number must have at least 4 digits", exception.Message);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenCardNumberIsEmpty()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => CardLastFourDigits.From(string.Empty));
        Assert.Equal("cardNumber", exception.ParamName);
        Assert.Contains("Card number must have at least 4 digits", exception.Message);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenCardNumberIsWhitespace()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => CardLastFourDigits.From("   "));
        Assert.Equal("cardNumber", exception.ParamName);
        Assert.Contains("Card number must have at least 4 digits", exception.Message);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenCardNumberHasLessThanFourDigits()
    {
        // Arrange
        const string cardNumber = "123";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => CardLastFourDigits.From(cardNumber));
        Assert.Equal("cardNumber", exception.ParamName);
        Assert.Contains("Card number must have at least 4 digits", exception.Message);
    }

    [Fact]
    public void From_ShouldExtractLastFourDigits_WhenCardNumberHasExactlyFourDigits()
    {
        // Arrange
        const string cardNumber = "1234";

        // Act
        var cardLastFour = CardLastFourDigits.From(cardNumber);

        // Assert
        Assert.NotNull(cardLastFour);
        Assert.Equal("1234", cardLastFour.Value);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenLastFourDigitsAreNotNumeric()
    {
        // Arrange
        const string cardNumber = "4111111111111ABC";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => CardLastFourDigits.From(cardNumber));
        Assert.Equal("cardNumber", exception.ParamName);
        Assert.Contains("Last 4 digits must be numeric", exception.Message);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenLastFourDigitsContainSpecialCharacters()
    {
        // Arrange
        const string cardNumber = "4111111111111@#$%";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => CardLastFourDigits.From(cardNumber));
        Assert.Equal("cardNumber", exception.ParamName);
        Assert.Contains("Last 4 digits must be numeric", exception.Message);
    }

    [Theory]
    [InlineData("4111111111110000", "0000")]
    [InlineData("5555555555559999", "9999")]
    [InlineData("3782822463100005", "0005")]
    public void From_ShouldExtractLastFourDigits_WithVariousCardNumbers(string cardNumber, string expectedLastFour)
    {
        // Act
        var cardLastFour = CardLastFourDigits.From(cardNumber);

        // Assert
        Assert.Equal(expectedLastFour.ToString(), cardLastFour.Value);
    }
}