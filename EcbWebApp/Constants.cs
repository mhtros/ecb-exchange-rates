namespace EcbWebApp;

public static class Constants
{
    public static class ExchangeRateStrategies
    {
        public const string SpecificDate = "SpecificDate";
        public const string SpecificDateOrNextAvailable = "SpecificDateOrNextAvailable";

        public static readonly List<string> StrategiesList = new()
        {
            SpecificDate, SpecificDateOrNextAvailable
        };
    }
}