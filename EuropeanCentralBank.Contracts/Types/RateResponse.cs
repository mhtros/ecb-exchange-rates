namespace EuropeanCentralBank.Contracts.Types;

public class RatesResponse
{
    public RatesResponse(DateTime date, IReadOnlyCollection<CurrencyRate> rates)
    {
        Date = date;
        Rates = rates;
    }

    public DateTime Date { get; }
    public IReadOnlyCollection<CurrencyRate> Rates { get; }
}