using System.Net.Http;
using System.Threading.Tasks;
using EuropeanCentralBank.Contracts.Core;
using EuropeanCentralBank.Contracts.Types;
using EuropeanCentralBank.Types;
using Microsoft.Extensions.Options;

namespace EuropeanCentralBank.Core;

internal class EuropeanCentralBankClient : IEuropeanCentralBankClient
{
    private readonly HttpClient _client;
    private readonly EuropeanCentralBankOptions _options;
    private readonly IXmlRateResponseDeserializer _xmlDeserializer;

    public EuropeanCentralBankClient(HttpClient client, IOptions<EuropeanCentralBankOptions> options,
        IXmlRateResponseDeserializer xmlDeserializer)
    {
        _client = client;
        _options = options.Value;
        _xmlDeserializer = xmlDeserializer;
    }

    public async Task<RatesResponse> GetRates()
    {
        var httpResponse = await _client.GetAsync(_options.RatesRoute);
        httpResponse.EnsureSuccessStatusCode();

        var rawXml = await httpResponse.Content.ReadAsStringAsync();
        var ratesResponse = _xmlDeserializer.Deserialize(rawXml);

        return ratesResponse;
    }
}