using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml;
using EuropeanCentralBank.Contracts.Types;
using EuropeanCentralBank.Types;

[assembly: InternalsVisibleTo("EuropeanCentralBank.Tests")]

namespace EuropeanCentralBank.Core;

internal class XmlRateResponseDeserializer : IXmlRateResponseDeserializer
{
    public RatesResponse Deserialize(string rawXml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(rawXml);

        // Using xPath expression to retrieve the node containing the time attribute 
        var dateNode = xmlDocument.SelectSingleNode("//*[@time]");

        var rawDate = dateNode?.Attributes?["time"]?.Value;
        var dateTime = DateTime.Parse(rawDate ?? throw new InvalidOperationException());

        // Using xPath expression to retrieve the nodes containing the currency attribute
        var currencyRatesNodeList = xmlDocument.SelectNodes("//*[@currency]");

        if (currencyRatesNodeList == null)
            return new RatesResponse(dateTime, Array.Empty<CurrencyRate>());

        var currencyRates = new List<CurrencyRate>();

        foreach (XmlNode node in currencyRatesNodeList)
        {
            var rawCurrencyCode = node?.Attributes?["currency"]?.Value!;
            var rawRate = node?.Attributes?["rate"]?.Value;

            // Don't throw exception.
            var succeed = decimal.TryParse(rawRate,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var parsedRate);

            // Pass -1 to know that something went wrong.
            var rate = succeed ? parsedRate : -1;

            var item = new CurrencyRate(rawCurrencyCode, rate);
            currencyRates.Add(item);
        }

        var response = new RatesResponse(dateTime, currencyRates.AsReadOnly());
        return response;
    }
}