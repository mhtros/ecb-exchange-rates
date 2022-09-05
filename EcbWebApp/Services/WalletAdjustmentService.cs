using EcbWebApp.Exceptions;
using EcbWebApp.ExchangeRateStrategies;
using EcbWebApp.Repositories;

namespace EcbWebApp.Services;

internal class WalletAdjustmentService : IWalletAdjustmentService
{
    private readonly ICurrencyRatesRepository _currencyRatesRepository;

    private readonly Dictionary<string, Func<ICurrencyRatesRepository, IExchangeRateStrategy>> _strategies = new()
    {
        {Constants.ExchangeRateStrategies.SpecificDate, repo => new SpecificDateExchangeRateStrategy(repo)},
        {
            Constants.ExchangeRateStrategies.SpecificDateOrNextAvailable,
            repo => new SpecificDateOrNextAvailableRateStrategy(repo)
        }
    };

    private readonly IWalletRepository _walletRepository;

    public WalletAdjustmentService(IWalletRepository walletRepository, ICurrencyRatesRepository currencyRatesRepository)
    {
        _walletRepository = walletRepository;
        _currencyRatesRepository = currencyRatesRepository;
    }

    public async Task<decimal> AdjustBalanceAsync(string exchangeRateStrategy, DateTime date, int walletId,
        string currencyCode, decimal amount)
    {
        if (_strategies.ContainsKey(exchangeRateStrategy) == false)
            throw new ArgumentException("Invalid strategy type");

        // Retrieve the exchange rate strategy class instance at runtime using the strategy pattern.
        var exchangeRateStrategyInstance = _strategies[exchangeRateStrategy](_currencyRatesRepository);

        var wallet = await _walletRepository.GetWalletByIdAsync(walletId);
        if (wallet is null) throw new WalletNotFoundException(walletId);

        var convertedAmount =
            await exchangeRateStrategyInstance.ConvertAsync(amount, currencyCode, wallet.CurrencyCode, date);

        wallet.Adjust(convertedAmount);
        await _walletRepository.SaveAsync();

        return wallet.Balance;
    }
}