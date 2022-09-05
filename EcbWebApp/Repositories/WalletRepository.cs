using EcbWebApp.Database;
using EcbWebApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcbWebApp.Repositories;

public class WalletRepository : BaseRepository, IWalletRepository
{
    public WalletRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task CreateWalletAsync(WalletEntity model)
    {
        await Context.Wallets.AddAsync(model);
    }

    public async Task<WalletEntity?> GetWalletByIdAsync(int id)
    {
        var wallet = await Context.Wallets.FirstOrDefaultAsync();
        return wallet;
    }

    public async Task<bool> WalletExistsAsync(int id)
    {
        var exists = await Context.Wallets.AnyAsync(x => x.Id == id);
        return exists;
    }
}