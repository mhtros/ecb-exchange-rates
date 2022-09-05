using EuropeanCentralBank.Contracts.Types;

namespace EuropeanCentralBank.Contracts.Core;

public interface IEuropeanCentralBankClient
{
    Task<RatesResponse> GetRates();
}