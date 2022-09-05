using System;
using EuropeanCentralBank.Contracts.Core;
using EuropeanCentralBank.Core;
using EuropeanCentralBank.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EuropeanCentralBank;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEuropeanCentralBank(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IXmlRateResponseDeserializer, XmlRateResponseDeserializer>();

        // Configure typed HttpClient EuropeanCentralBankClient.
        services.AddHttpClient<IEuropeanCentralBankClient, EuropeanCentralBankClient>(client =>
        {
            client.BaseAddress = new Uri("https://www.ecb.europa.eu");
            client.DefaultRequestHeaders.Add("Accept", "application/xml");
        });

        services.Configure<EuropeanCentralBankOptions>(
            configuration.GetRequiredSection(EuropeanCentralBankOptions.EuropeanCentralBankSettings));

        return services;
    }
}