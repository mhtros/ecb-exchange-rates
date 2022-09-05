using EcbWebApp.Repositories;
using EuropeanCentralBank.Contracts.Core;

namespace EcbWebApp.HostedServices;

public class UpdateRatesHostedService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<UpdateRatesHostedService> _logger;

    public UpdateRatesHostedService(ILogger<UpdateRatesHostedService> logger, IServiceProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UpdateRatesHostedService running...");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Passing the services dependencies using a newly created scope and
            // the service Locator pattern. Doing that will ensure that the dependencies
            // instances will be refreshed every time the ExecuteAsync is called and there
            // will be no captured dependencies
            using (var scope = _provider.CreateScope())
            {
                var client = scope.ServiceProvider.GetRequiredService<IEuropeanCentralBankClient>();
                var currencyRatesRepository = scope.ServiceProvider.GetRequiredService<ICurrencyRatesRepository>();

                var rates = await client.GetRates();
                await currencyRatesRepository.ManageCurrencyRatesAsync(rates);

                _logger.LogInformation("UpdateRatesHostedService rates updated");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

        _logger.LogInformation("UpdateRatesHostedService is stopping...");
    }
}