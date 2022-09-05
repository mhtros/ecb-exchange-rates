using System.Globalization;
using EcbWebApp.Database;
using EcbWebApp.Entities;
using EcbWebApp.Exceptions;
using EcbWebApp.ExchangeRateStrategies;
using EcbWebApp.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EcbWebApp.Tests;

public class ExchangeRateStrategyTests
{
    [Fact]
    public async Task SpecificDateExchangeRateStrategy_ConvertAsync_ShouldThrowIfFromCurrencyRateIsNull()
    {
        // Arrange
        await using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;

        await using (var context = new ApplicationDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var date = new DateTime(2022, 01, 01);

            await context.AddRangeAsync(
                new CurrencyRateEntity("USD", 1.31m, date, 1),
                new CurrencyRateEntity("CAD", 1.56m, date, 2)
            );

            await context.SaveChangesAsync();
        }

        await using (var context = new ApplicationDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
            var currencyRateRepository = new CurrencyRatesRepository(context);
            var specificDateExchangeRateStrategy = new SpecificDateExchangeRateStrategy(currencyRateRepository);

            // Act
            var date = new DateTime(2020, 01, 01);

            // Assert
            var exception = await Assert.ThrowsAsync<ExchangeRateDateNotFoundException>(
                async () => await specificDateExchangeRateStrategy
                    .ConvertAsync(100m, "USD", "CAD", date));

            Assert.Equal(date, exception?.Date);
            Assert.Equal("USD", exception?.CurrencyCode);
        }
    }

    [Theory]
    [InlineData("100", "USD", "1.0169", "USD", "1.0169", "100.0000")]
    [InlineData("200", "USD", "1.0169", "CAD", "1.3114", "257.9200")]
    [InlineData("999", "CAD", "1.3114", "JPY", "135.4872", "103211.5851")]
    public async Task SpecificDateExchangeRateStrategy_ConvertAsync_ShouldCalculateRate(string rawAmount,
        string amountCurrencyCode, string rawAmountCurrencyCodeRate, string currencyCodeToConvert,
        string rawCurrencyCodeToConvertRate, string rawExpectedAmount)
    {
        // Arrange
        var amount = decimal.Parse(rawAmount, NumberStyles.Any, CultureInfo.InvariantCulture);

        var amountCurrencyCodeRate =
            decimal.Parse(rawAmountCurrencyCodeRate, NumberStyles.Any, CultureInfo.InvariantCulture);

        var currencyCodeToConvertRate =
            decimal.Parse(rawCurrencyCodeToConvertRate, NumberStyles.Any, CultureInfo.InvariantCulture);

        var expectedAmount = decimal.Parse(rawExpectedAmount, NumberStyles.Any, CultureInfo.InvariantCulture);

        await using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;

        await using (var context = new ApplicationDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var date = new DateTime(2022, 01, 01);

            await context.AddAsync(new CurrencyRateEntity(currencyCodeToConvert, currencyCodeToConvertRate, date, 1));

            if (amountCurrencyCode != currencyCodeToConvert)
            {
                await context.AddAsync(new CurrencyRateEntity(amountCurrencyCode, amountCurrencyCodeRate, date, 2));
            }

            await context.SaveChangesAsync();
        }

        await using (var context = new ApplicationDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
            var currencyRateRepository = new CurrencyRatesRepository(context);
            var specificDateExchangeRateStrategy = new SpecificDateExchangeRateStrategy(currencyRateRepository);

            // Act
            var date = new DateTime(2022, 01, 01);
            var rate = await specificDateExchangeRateStrategy
                .ConvertAsync(amount, amountCurrencyCode, currencyCodeToConvert, date);

            // Assert
            Assert.Equal(expectedAmount, rate);
        }
    }

    [Fact]
    public async Task SpecificDateOrNextAvailableRateStrategy_ConvertAsync_ShouldThrowIfFromCurrencyRateIsNull()
    {
        // Arrange
        await using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;

        await using (var context = new ApplicationDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var date1 = new DateTime(2000, 01, 01);
            var date2 = new DateTime(2022, 01, 01);

            await context.AddRangeAsync(
                new CurrencyRateEntity("USD", 1.31m, date1, 1),
                new CurrencyRateEntity("CAD", 1.56m, date2, 2)
            );

            await context.SaveChangesAsync();
        }

        await using (var context = new ApplicationDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
            var currencyRateRepository = new CurrencyRatesRepository(context);
            var specificDateOrNextAvailableRateStrategy =
                new SpecificDateOrNextAvailableRateStrategy(currencyRateRepository);

            // Act
            var date = new DateTime(2020, 01, 01);

            // Assert
            var exception = await Assert.ThrowsAsync<ExchangeRateDateNotFoundException>(
                async () => await specificDateOrNextAvailableRateStrategy
                    .ConvertAsync(100m, "USD", "CAD", date));

            Assert.Equal(date, exception?.Date);
            Assert.Equal("USD", exception?.CurrencyCode);
        }
    }

    [Theory]
    [InlineData("100", "USD", "1.0169", "USD", "1.0169", "100.0000")]
    [InlineData("200", "USD", "1.0169", "CAD", "1.3114", "257.9200")]
    [InlineData("999", "CAD", "1.3114", "JPY", "135.4872", "103211.5851")]
    public async Task SpecificDateOrNextAvailableRateStrategy_ConvertAsync_ShouldCalculateRate(string rawAmount,
        string amountCurrencyCode, string rawAmountCurrencyCodeRate, string currencyCodeToConvert,
        string rawCurrencyCodeToConvertRate, string rawExpectedAmount)
    {
        // Arrange
        var amount = decimal.Parse(rawAmount, NumberStyles.Any, CultureInfo.InvariantCulture);

        var amountCurrencyCodeRate =
            decimal.Parse(rawAmountCurrencyCodeRate, NumberStyles.Any, CultureInfo.InvariantCulture);

        var currencyCodeToConvertRate =
            decimal.Parse(rawCurrencyCodeToConvertRate, NumberStyles.Any, CultureInfo.InvariantCulture);

        var expectedAmount = decimal.Parse(rawExpectedAmount, NumberStyles.Any, CultureInfo.InvariantCulture);

        await using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;

        await using (var context = new ApplicationDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var date = new DateTime(2022, 01, 01);

            await context.AddAsync(new CurrencyRateEntity(currencyCodeToConvert, currencyCodeToConvertRate, date, 1));

            if (amountCurrencyCode != currencyCodeToConvert)
            {
                await context.AddAsync(new CurrencyRateEntity(amountCurrencyCode, amountCurrencyCodeRate, date, 2));
            }

            await context.SaveChangesAsync();
        }

        await using (var context = new ApplicationDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
            var currencyRateRepository = new CurrencyRatesRepository(context);
            var specificDateOrNextAvailableRateStrategy =
                new SpecificDateOrNextAvailableRateStrategy(currencyRateRepository);

            // Act
            var date = new DateTime(2022, 01, 01);
            var rate = await specificDateOrNextAvailableRateStrategy
                .ConvertAsync(amount, amountCurrencyCode, currencyCodeToConvert, date);

            // Assert
            Assert.Equal(expectedAmount, rate);
        }
    }
}