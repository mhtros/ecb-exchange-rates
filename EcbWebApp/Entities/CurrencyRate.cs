namespace EcbWebApp.Entities;

public class CurrencyRateEntity
{
    public CurrencyRateEntity(string currencyCode, decimal rate, DateTime createdAt, int? id = null)
    {
        Id = id;
        CurrencyCode = currencyCode;
        Rate = rate;
        CreatedAt = createdAt;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     currency id
    /// </summary>
    public int? Id { get; }

    /// <summary>
    ///     ISO code of the currency
    /// </summary>
    public string CurrencyCode { get; }

    /// <summary>
    ///     currency rate related to EUR
    /// </summary>
    public decimal Rate { get; private set; }

    /// <summary>
    ///     creation date of the Currency
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    ///     date of the last update of the Currency
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    public void Update(decimal rate)
    {
        Rate = rate;
        UpdatedAt = DateTime.UtcNow;
    }
}