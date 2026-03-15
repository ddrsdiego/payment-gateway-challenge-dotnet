using PaymentGateway.Api.Domain.Aggregates.PaymentAggregate;

namespace PaymentGateway.Api.Tests;

public class ExpiryCardDateTests
{
    [Fact]
    public void From_ShouldCreateExpiryCardDate_WhenMonthAndYearAreValid()
    {
        // Arrange
        const int expiryMonth = 6;
        const int expiryYear = 2025;

        // Act
        var expiryCardDate = ExpiryCardDate.From(expiryMonth, expiryYear);

        // Assert
        Assert.NotEqual(default, expiryCardDate);
        Assert.Equal(expiryMonth, expiryCardDate.Month);
        Assert.Equal(expiryYear, expiryCardDate.Year);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenMonthIsZero()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ExpiryCardDate.From(0, 2025));
        Assert.Equal("expiryMonth", exception.ParamName);
        Assert.Contains("Expiry month must be between 1 and 12", exception.Message);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenMonthIsNegative()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ExpiryCardDate.From(-1, 2025));
        Assert.Equal("expiryMonth", exception.ParamName);
        Assert.Contains("Expiry month must be between 1 and 12", exception.Message);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenMonthIsThirteen()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ExpiryCardDate.From(13, 2025));
        Assert.Equal("expiryMonth", exception.ParamName);
        Assert.Contains("Expiry month must be between 1 and 12", exception.Message);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenYearIsTwoDigits()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ExpiryCardDate.From(6, 26));
        Assert.Equal("expiryYear", exception.ParamName);
        Assert.Contains("must be a 4-digit year", exception.Message);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenYearIsZero()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ExpiryCardDate.From(6, 0));
        Assert.Equal("expiryYear", exception.ParamName);
        Assert.Contains("must be a 4-digit year", exception.Message);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenYearIsNegative()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ExpiryCardDate.From(6, -1));
        Assert.Equal("expiryYear", exception.ParamName);
        Assert.Contains("must be a 4-digit year", exception.Message);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenYearIsTooFarInFuture()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;
        var invalidYear = currentYear + 21;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ExpiryCardDate.From(6, invalidYear));
        Assert.Equal("expiryYear", exception.ParamName);
        Assert.Contains("exceeds the maximum allowed", exception.Message);
    }

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenCardIsExpiredPreviousYear()
    {
        // Arrange
        const int expiryMonth = 6;
        const int expiryYear = 2020; // Far in the past

        // Act
        var expiryCardDate = ExpiryCardDate.From(expiryMonth, expiryYear);

        // Assert
        Assert.True(expiryCardDate.IsExpired);
    }

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenCardIsExpiredPreviousMonth()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var expiryMonth = now.Month == 1 ? 12 : now.Month - 1;
        var expiryYear = now.Month == 1 ? now.Year - 1 : now.Year;

        // Act
        var expiryCardDate = ExpiryCardDate.From(expiryMonth, expiryYear);

        // Assert
        Assert.True(expiryCardDate.IsExpired);
    }

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenCardIsValidCurrentMonthAndYear()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var expiryCardDate = ExpiryCardDate.From(now.Month, now.Year);

        // Assert
        Assert.False(expiryCardDate.IsExpired);
    }

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenCardIsValidFutureMonth()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var expiryMonth = now.Month == 12 ? 1 : now.Month + 1;
        var expiryYear = now.Month == 12 ? now.Year + 1 : now.Year;

        // Act
        var expiryCardDate = ExpiryCardDate.From(expiryMonth, expiryYear);

        // Assert
        Assert.False(expiryCardDate.IsExpired);
    }

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenCardIsValidFutureYear()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;
        const int expiryMonth = 6;
        var expiryYear = currentYear + 5; // 5 years in the future

        // Act
        var expiryCardDate = ExpiryCardDate.From(expiryMonth, expiryYear);

        // Assert
        Assert.False(expiryCardDate.IsExpired);
    }

    [Theory]
    [InlineData(1, 2025, 1)]
    [InlineData(6, 2025, 6)]
    [InlineData(12, 2025, 12)]
    public void From_ShouldStoreMonthCorrectly_WhenMonthIsValid(int month, int year, int expectedMonth)
    {
        // Act
        var expiryCardDate = ExpiryCardDate.From(month, year);

        // Assert
        Assert.Equal(expectedMonth, expiryCardDate.Month);
    }

    [Fact]
    public void From_ShouldStoreYearCorrectly_WhenYearIsValid()
    {
        // Arrange
        const int expiryMonth = 6;
        const int expiryYear = 2025;

        // Act
        var expiryCardDate = ExpiryCardDate.From(expiryMonth, expiryYear);

        // Assert
        Assert.Equal(expiryYear, expiryCardDate.Year);
    }

    [Theory]
    [InlineData(1, 2025)]
    [InlineData(2, 2026)]
    [InlineData(3, 2027)]
    [InlineData(4, 2028)]
    [InlineData(5, 2029)]
    [InlineData(6, 2030)]
    [InlineData(7, 2031)]
    [InlineData(8, 2032)]
    [InlineData(9, 2033)]
    [InlineData(10, 2034)]
    [InlineData(11, 2035)]
    [InlineData(12, 2036)]
    public void From_ShouldCreateExpiryCardDate_WithAllValidMonths(int month, int year)
    {
        // Act
        var expiryCardDate = ExpiryCardDate.From(month, year);

        // Assert
        Assert.Equal(month, expiryCardDate.Month);
        Assert.Equal(year, expiryCardDate.Year);
    }

    [Fact]
    public void ExpiryDate_ShouldFormatMonthWithLeadingZero_WhenMonthIsSingleDigit()
    {
        // Arrange
        const int expiryMonth = 1;
        const int expiryYear = 2025;

        // Act
        var expiryCardDate = ExpiryCardDate.From(expiryMonth, expiryYear);

        // Assert
        Assert.Equal("01/2025", expiryCardDate.ExpiryDate);
    }

    [Fact]
    public void ExpiryDate_ShouldFormatMonthWithoutExtraZero_WhenMonthIsTwoDigits()
    {
        // Arrange
        const int expiryMonth = 12;
        const int expiryYear = 2025;

        // Act
        var expiryCardDate = ExpiryCardDate.From(expiryMonth, expiryYear);

        // Assert
        Assert.Equal("12/2025", expiryCardDate.ExpiryDate);
    }

    [Theory]
    [InlineData(1, 2025, "01/2025")]
    [InlineData(2, 2025, "02/2025")]
    [InlineData(3, 2025, "03/2025")]
    [InlineData(4, 2025, "04/2025")]
    [InlineData(5, 2025, "05/2025")]
    [InlineData(6, 2025, "06/2025")]
    [InlineData(7, 2025, "07/2025")]
    [InlineData(8, 2025, "08/2025")]
    [InlineData(9, 2025, "09/2025")]
    [InlineData(10, 2025, "10/2025")]
    [InlineData(11, 2025, "11/2025")]
    [InlineData(12, 2025, "12/2025")]
    public void ExpiryDate_ShouldFormatCorrectly_WithAllValidMonths(int month, int year, string expectedFormat)
    {
        // Act
        var expiryCardDate = ExpiryCardDate.From(month, year);

        // Assert
        Assert.Equal(expectedFormat, expiryCardDate.ExpiryDate);
    }

    [Fact]
    public void ExpiryDate_ShouldIncludeYearWithoutModification_WhenYearIsProvided()
    {
        // Arrange
        const int expiryMonth = 6;
        const int expiryYear = 2025;

        // Act
        var expiryCardDate = ExpiryCardDate.From(expiryMonth, expiryYear);

        // Assert
        Assert.Contains("2025", expiryCardDate.ExpiryDate);
    }

    [Fact]
    public void From_ShouldCreateEqualInstances_WhenMonthAndYearAreIdentical()
    {
        // Arrange
        const int expiryMonth = 6;
        const int expiryYear = 2025;

        // Act
        var expiryCardDate1 = ExpiryCardDate.From(expiryMonth, expiryYear);
        var expiryCardDate2 = ExpiryCardDate.From(expiryMonth, expiryYear);

        // Assert
        Assert.Equal(expiryCardDate1, expiryCardDate2);
    }

    [Fact]
    public void From_ShouldCreateDifferentInstances_WhenMonthIsDifferent()
    {
        // Arrange
        const int expiryYear = 2025;

        // Act
        var expiryCardDate1 = ExpiryCardDate.From(1, expiryYear);
        var expiryCardDate2 = ExpiryCardDate.From(12, expiryYear);

        // Assert
        Assert.NotEqual(expiryCardDate1, expiryCardDate2);
    }

    [Fact]
    public void From_ShouldCreateDifferentInstances_WhenYearIsDifferent()
    {
        // Arrange
        const int expiryMonth = 6;

        // Act
        var expiryCardDate1 = ExpiryCardDate.From(expiryMonth, 2025);
        var expiryCardDate2 = ExpiryCardDate.From(expiryMonth, 2026);

        // Assert
        Assert.NotEqual(expiryCardDate1, expiryCardDate2);
    }
}
