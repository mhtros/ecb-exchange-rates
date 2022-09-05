namespace EcbWebApp.Exceptions;

public class ExchangeRateDateNotFoundException : Exception
{
    public ExchangeRateDateNotFoundException(DateTime date, string currencyCode)
    {
        Date = date;
        CurrencyCode = currencyCode;
    }

    public DateTime Date { get; }
    public string CurrencyCode { get; }
}