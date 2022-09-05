using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EcbWebApp.Tests")]

namespace EcbWebApp.ExchangeRateStrategies;

internal abstract class ExchangeRateStrategyBase : IExchangeRateStrategy
{
    public async Task<decimal> ConvertAsync(decimal amount, string amountCurrencyCode, string currencyCodeToConvert,
        DateTime date)
    {
        var rate = await GetRateAsync(amountCurrencyCode, currencyCodeToConvert, date);
        return amount * rate;
    }

    protected abstract Task<decimal> GetRateAsync(string currencyCodeFrom, string currencyCodeTo, DateTime date);
}