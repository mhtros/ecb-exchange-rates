namespace EcbWebApp.Models;

public class CurrencyRateModel
{
    public CurrencyRateModel(string currencyCode, decimal rate, DateTime createdAt, int? id = null)
    {
        Id = id;
        CurrencyCode = currencyCode;
        Rate = rate;
        CreatedAt = createdAt;
    }

    public int? Id { get; }
    public string CurrencyCode { get; }
    public decimal Rate { get; }
    public DateTime CreatedAt { get; }
}