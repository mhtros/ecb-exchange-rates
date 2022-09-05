namespace EuropeanCentralBank.Contracts.Types;

public class CurrencyRate
{
    public CurrencyRate(string currencyCode, decimal rate)
    {
        CurrencyCode = currencyCode;
        Rate = rate;
    }

    public string CurrencyCode { get; }
    public decimal Rate { get; }
}