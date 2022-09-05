namespace EcbWebApp.Exceptions;

public class WalletNotFoundException : Exception
{
    public WalletNotFoundException(int walletId)
    {
        WalletId = walletId;
    }

    public int WalletId { get; }
}