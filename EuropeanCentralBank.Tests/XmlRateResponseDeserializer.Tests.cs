using System.Xml;
using EuropeanCentralBank.Core;

namespace EuropeanCentralBank.Tests;

public class XmlRateResponseDeserializerTests
{
    [Fact]
    public void XmlRateResponseDeserializer_ShouldThrowXmlExceptionWhenEmpty()
    {
        // Arrange
        var deserializer = new XmlRateResponseDeserializer();
        var rawXml = string.Empty;

        // Act - Assert
        Assert.Throws<XmlException>(() => deserializer.Deserialize(rawXml));
    }

    [Fact]
    public void XmlRateResponseDeserializer_ShouldThrowInvalidOperationExceptionWhenTimeAttributeIsMissing()
    {
        // Arrange
        var deserializer = new XmlRateResponseDeserializer();
        var rawXml = File.ReadAllText("Xml/no_time_attribute.xml");

        // Act - Assert
        Assert.Throws<InvalidOperationException>(() => deserializer.Deserialize(rawXml));
    }

    [Fact]
    public void XmlRateResponseDeserializer_ShouldReturnEmptyArrayWhenCurrencyAttributeIsMissing()
    {
        // Arrange
        var deserializer = new XmlRateResponseDeserializer();
        var rawXml = File.ReadAllText("Xml/no_currency_attribute.xml");

        // Act
        var ratesResponse = deserializer.Deserialize(rawXml);

        // Assert
        Assert.Empty(ratesResponse.Rates);
    }

    [Fact]
    public void XmlRateResponseDeserializer_ShouldHaveRateMinusOneWhenRateAttributeIsMissing()
    {
        // Arrange
        var deserializer = new XmlRateResponseDeserializer();
        var rawXml = File.ReadAllText("Xml/no_rate_attribute.xml");

        // Act
        var ratesResponse = deserializer.Deserialize(rawXml);

        // Assert
        Assert.All(ratesResponse.Rates, item => Assert.Equal(item.Rate, decimal.MinusOne));
    }
}