using EuropeanCentralBank.Contracts.Types;

namespace EuropeanCentralBank.Types;

public interface IXmlRateResponseDeserializer
{
    RatesResponse Deserialize(string rawXml);
}