namespace EcbWebApp.Services;

public interface IWalletAdjustmentService
{
    Task<decimal> AdjustBalanceAsync(string exchangeRateStrategy, DateTime exchangeRateDate, int walletId,
        string currencyCode, decimal amount);
}