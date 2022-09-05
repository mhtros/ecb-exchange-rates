using EcbWebApp.Entities;
using EcbWebApp.Models;
using EuropeanCentralBank.Contracts.Types;

namespace EcbWebApp.Repositories;

public interface ICurrencyRatesRepository : ISaveable
{
    public Task ManageCurrencyRatesAsync(RatesResponse response);

    public Task CreateCurrencyRateAsync(CurrencyRateEntity model);

    public Task<IEnumerable<CurrencyRateModel>> GetRatesAsync(DateTime date);

    public Task<CurrencyRate?> GetSingleRateAsync(string currencyCode, DateTime date, bool nextAvailableDate = false);
}