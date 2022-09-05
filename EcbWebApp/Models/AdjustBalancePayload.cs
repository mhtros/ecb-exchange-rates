namespace EcbWebApp.Models;

public class AdjustBalancePayload
{
    public AdjustBalancePayload(int walletId, string currencyCode, decimal amount)
    {
        WalletId = walletId;
        CurrencyCode = currencyCode;
        Amount = amount;
    }

    /// <summary>
    ///     target wallet id
    /// </summary>
    public int WalletId { get; }

    /// <summary>
    ///     Currency code for current adjust request amount
    /// </summary>
    public string CurrencyCode { get; }

    /// <summary>
    ///     Requested amount to be adjusted
    /// </summary>
    public decimal Amount { get; }
}