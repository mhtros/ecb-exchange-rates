using System.Runtime.CompilerServices;
using EcbWebApp.Exceptions;
using EcbWebApp.Repositories;

[assembly: InternalsVisibleTo("EcbWebApp.Tests")]

namespace EcbWebApp.ExchangeRateStrategies;

internal class SpecificDateExchangeRateStrategy : ExchangeRateStrategyBase
{
    private readonly ICurrencyRatesRepository _currencyRatesRepository;

    public SpecificDateExchangeRateStrategy(ICurrencyRatesRepository currencyRatesRepository)
    {
        _currencyRatesRepository = currencyRatesRepository;
    }

    protected override async Task<decimal> GetRateAsync(string currencyCodeFrom, string currencyCodeTo, DateTime date)
    {
        var fromCurrencyRate = await _currencyRatesRepository.GetSingleRateAsync(currencyCodeFrom, date);
        var toCurrencyRate = await _currencyRatesRepository.GetSingleRateAsync(currencyCodeTo, date);

        if (fromCurrencyRate is null) throw new ExchangeRateDateNotFoundException(date, currencyCodeFrom);
        if (toCurrencyRate is null) throw new ExchangeRateDateNotFoundException(date, currencyCodeTo);
        if (toCurrencyRate.Rate == 0) return decimal.Zero;

        var rate = toCurrencyRate.Rate / fromCurrencyRate.Rate;
        var roundedRate = decimal.Round(rate, 4);
        return roundedRate;
    }
}