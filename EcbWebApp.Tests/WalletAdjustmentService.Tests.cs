using System.Globalization;
using EcbWebApp.Database;
using EcbWebApp.Entities;
using EcbWebApp.Exceptions;
using EcbWebApp.Repositories;
using EcbWebApp.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EcbWebApp.Tests;

public class WalletAdjustmentServiceTests
{
    [Fact]
    public async Task WalletAdjustmentService_ShouldThrowIfInvalidStrategy()
    {
        // Arrange
        await using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var walletRepository = new WalletRepository(context);
        var currencyRateRepository = new CurrencyRatesRepository(context);
        var walletAdjustmentService = new WalletAdjustmentService(walletRepository, currencyRateRepository);

        // Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await walletAdjustmentService.AdjustBalanceAsync("INVALID", DateTime.Now, 0, "", 0));

        Assert.Equal("Invalid strategy type", exception.Message);
    }

    [Fact]
    public async Task WalletAdjustmentService_ShouldThrowIfInsufficientBalance()
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

            await context.Wallets.AddAsync(new WalletEntity("USD", 1000m, date, date, 1));

            await context.SaveChangesAsync();
        }

        await using (var context = new ApplicationDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var walletRepository = new WalletRepository(context);
            var currencyRateRepository = new CurrencyRatesRepository(context);
            var walletAdjustmentService = new WalletAdjustmentService(walletRepository, currencyRateRepository);

            var wallet = await walletRepository.GetWalletByIdAsync(1);
            var date = new DateTime(2022, 01, 01);

            // Assert
            var exception = await Assert.ThrowsAsync<NoSufficientBalanceException>(async () =>
                await walletAdjustmentService.AdjustBalanceAsync(Constants.ExchangeRateStrategies.SpecificDate, date,
                    wallet?.Id ?? 0, "CAD", -3000m));

            Assert.Equal(1, exception.WalletId);
            Assert.Equal(1000m, exception.AvailableBalance);
            Assert.Equal(-2519.1m, exception.RequestedAmount);
        }
    }

    [Theory]
    [InlineData("2000", "USD", Constants.ExchangeRateStrategies.SpecificDate, "3000")]
    [InlineData("1000", "CAD", Constants.ExchangeRateStrategies.SpecificDateOrNextAvailable, "1839.7000")]
    [InlineData("-500", "CAD", Constants.ExchangeRateStrategies.SpecificDate, "580.1500")]
    public async Task WalletAdjustmentService_ShouldCalculateUpdateWallet(string rawAmount, string amountCurrencyCode,
        string strategy, string rawExpectedBalance)
    {
        // Arrange
        var amount = decimal.Parse(rawAmount, NumberStyles.Any, CultureInfo.InvariantCulture);
        var expectedBalance = decimal.Parse(rawExpectedBalance, NumberStyles.Any, CultureInfo.InvariantCulture);

        await using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;

        await using (var context = new ApplicationDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var date = new DateTime(2022, 01, 01);

            await context.AddRangeAsync(
                new CurrencyRateEntity("USD", 1.31m, date, 1),
                new CurrencyRateEntity("CAD", 1.56m, date, 2));

            await context.Wallets.AddAsync(new WalletEntity("USD", 1000m, date, date, 1));

            await context.SaveChangesAsync();
        }

        await using (var context = new ApplicationDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var walletRepository = new WalletRepository(context);
            var currencyRateRepository = new CurrencyRatesRepository(context);
            var walletAdjustmentService = new WalletAdjustmentService(walletRepository, currencyRateRepository);

            var wallet = await walletRepository.GetWalletByIdAsync(1);
            var date = new DateTime(2022, 01, 01);

            // Assert
            var balance = await walletAdjustmentService
                .AdjustBalanceAsync(strategy, date, wallet?.Id ?? 0, amountCurrencyCode, amount);

            Assert.Equal(expectedBalance, balance);
            Assert.Equal(expectedBalance, wallet?.Balance);
        }
    }
}