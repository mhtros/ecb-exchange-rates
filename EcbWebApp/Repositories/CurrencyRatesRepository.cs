using EcbWebApp.Database;
using EcbWebApp.Entities;
using EcbWebApp.Models;
using EuropeanCentralBank.Contracts.Types;
using Microsoft.EntityFrameworkCore;

namespace EcbWebApp.Repositories;

public class CurrencyRatesRepository : BaseRepository, ICurrencyRatesRepository
{
    public CurrencyRatesRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task ManageCurrencyRatesAsync(RatesResponse response)
    {
        if (response.Rates.Count == 0) return;

        foreach (var currencyRate in response.Rates)
        {
            var existingEntity = await Context.CurrencyRates.FirstOrDefaultAsync(x =>
                x.CurrencyCode == currencyRate.CurrencyCode && x.CreatedAt.Date == response.Date);

            if (existingEntity is null)
            {
                var model = new CurrencyRateEntity(currencyRate.CurrencyCode, currencyRate.Rate, response.Date);
                await CreateCurrencyRateAsync(model);
                continue; // Short-circuit
            }

            // No changes have been made so short-circuit
            if (existingEntity.Rate == currencyRate.Rate) continue;

            // Rate has been changed so update the entity
            existingEntity.Update(currencyRate.Rate);
        }

        await SaveAsync();
    }

    public async Task<IEnumerable<CurrencyRateModel>> GetRatesAsync(DateTime date)
    {
        var currencyRates = await Context.CurrencyRates
            .Where(x => x.CreatedAt.Date == date.Date)
            .Select(x => new CurrencyRateModel(x.CurrencyCode, x.Rate, x.CreatedAt, x.Id))
            .ToListAsync();

        return currencyRates;
    }

    public async Task<CurrencyRate?> GetSingleRateAsync(string currencyCode, DateTime date,
        bool nextAvailableDate = false)
    {
        CurrencyRate? rate;

        if (nextAvailableDate)
            // Add From-To Date range
            rate = await Context.CurrencyRates.Where(
                    x => x.CurrencyCode == currencyCode &&
                         date.Date <= x.CreatedAt.Date && x.CreatedAt <= DateTime.UtcNow.Date)
                .Select(x => new CurrencyRate(x.CurrencyCode, x.Rate))
                .FirstOrDefaultAsync();
        else
            rate = await Context.CurrencyRates
                .Where(x => x.CurrencyCode == currencyCode && x.CreatedAt.Date == date.Date)
                .Select(x => new CurrencyRate(x.CurrencyCode, x.Rate))
                .FirstOrDefaultAsync();

        return rate;
    }

    public async Task CreateCurrencyRateAsync(CurrencyRateEntity model)
    {
        await Context.CurrencyRates.AddAsync(model);
    }
}