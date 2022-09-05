namespace EcbWebApp.Exceptions;

public class NoSufficientBalanceException : Exception
{
    public NoSufficientBalanceException(int walletId, decimal requestedAmount, decimal availableBalance)
    {
        WalletId = walletId;
        RequestedAmount = requestedAmount;
        AvailableBalance = availableBalance;
    }

    public int WalletId { get; }
    public decimal RequestedAmount { get; }
    public decimal AvailableBalance { get; }
}