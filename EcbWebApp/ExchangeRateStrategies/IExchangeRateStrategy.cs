namespace EcbWebApp.ExchangeRateStrategies;

public interface IExchangeRateStrategy
{
    Task<decimal> ConvertAsync(decimal amount, string amountCurrencyCode, string currencyCodeToConvert, DateTime date);
}