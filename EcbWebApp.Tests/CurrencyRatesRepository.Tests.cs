using EcbWebApp.Database;
using EcbWebApp.Entities;
using EcbWebApp.Repositories;
using EuropeanCentralBank.Contracts.Types;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EcbWebApp.Tests;

public class CurrencyRatesRepositoryTests
{
    [Fact]
    public async Task ManageCurrencyRates_ShouldCreateEntityIfNotExist()
    {
        // Arrange
        await using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;

        await using (var context = new ApplicationDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var date = new DateTime(2022, 01, 01);
            var rates = new List<CurrencyRate> {new("USD", 1.31m)};

            var rateResponse = new RatesResponse(date, rates);
            var currencyRateRepository = new CurrencyRatesRepository(context);

            // Act
            await currencyRateRepository.ManageCurrencyRatesAsync(rateResponse);
        }

        // Assert
        await using (var context = new ApplicationDbContext(options))
        {
            var date = new DateTime(2022, 01, 01);
            var currencyRate = await context.CurrencyRates
                .FirstOrDefaultAsync(x => x.CurrencyCode == "USD" && x.CreatedAt.Date == date.Date);

            Assert.NotNull(currencyRate);
        }
    }

    [Fact]
    public async Task ManageCurrencyRates_ShouldUpdateEntityOnlyIfRateHasChanged()
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

        // Act
        await using (var context = new ApplicationDbContext(options))
        {
            var date = new DateTime(2022, 01, 01);

            var rates = new List<CurrencyRate>
            {
                new("USD", 1.31m),
                new("CAD", 1.42m)
            };

            var rateResponse = new RatesResponse(date, rates);
            var currencyRateRepository = new CurrencyRatesRepository(context);

            await currencyRateRepository.ManageCurrencyRatesAsync(rateResponse);
        }

        // Assert
        await using (var context = new ApplicationDbContext(options))
        {
            var usdCurrencyRate = await context.CurrencyRates.FirstOrDefaultAsync(x => x.Id == 1);
            var cadCurrencyRate = await context.CurrencyRates.FirstOrDefaultAsync(x => x.Id == 2);

            Assert.Equal(1.31m, usdCurrencyRate?.Rate);
            Assert.Equal(1.42m, cadCurrencyRate?.Rate);
        }
    }

    [Fact]
    public async Task GetSingleRateAsync_ShouldReturnNullWhenNextAvailableIsFalseAndDateDontExist()
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
            // Act
            var date = new DateTime(2020, 01, 01);
            var currencyRateRepository = new CurrencyRatesRepository(context);
            var rate = await currencyRateRepository.GetSingleRateAsync("USD", date);

            // Assert
            Assert.Null(rate);
        }
    }

    [Fact]
    public async Task GetSingleRateAsync_ShouldReturnEntityWhenNextAvailableIsFalseAndDateExist()
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
            // Act
            var date = new DateTime(2022, 01, 01);
            var currencyRateRepository = new CurrencyRatesRepository(context);
            var rate = await currencyRateRepository.GetSingleRateAsync("USD", date);

            // Assert
            Assert.NotNull(rate);
            Assert.Equal(1.31m, rate?.Rate);
            Assert.Equal("USD", rate?.CurrencyCode);
        }
    }

    [Fact]
    public async Task GetSingleRateAsync_ShouldReturnEntityWhenNextAvailableIsTrueAndNextAvailableDateExist()
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
                new CurrencyRateEntity("GBP", 0.86m, new DateTime(2022, 02, 01), 2)
            );

            await context.SaveChangesAsync();
        }

        await using (var context = new ApplicationDbContext(options))
        {
            // Act
            var date = new DateTime(2022, 01, 01);
            var currencyRateRepository = new CurrencyRatesRepository(context);
            var rate = await currencyRateRepository.GetSingleRateAsync("GBP", date, true);

            // Assert
            Assert.NotNull(rate);
            Assert.Equal(0.86m, rate?.Rate);
            Assert.Equal("GBP", rate?.CurrencyCode);
        }
    }
}