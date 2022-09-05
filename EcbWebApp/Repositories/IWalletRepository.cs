using EcbWebApp.Entities;

namespace EcbWebApp.Repositories;

public interface IWalletRepository : ISaveable
{
    public Task CreateWalletAsync(WalletEntity model);

    public Task<WalletEntity?> GetWalletByIdAsync(int id);

    public Task<bool> WalletExistsAsync(int id);
}