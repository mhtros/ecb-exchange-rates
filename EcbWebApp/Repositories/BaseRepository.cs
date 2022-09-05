using EcbWebApp.Database;

namespace EcbWebApp.Repositories;

public abstract class BaseRepository
{
    protected readonly ApplicationDbContext Context;

    protected BaseRepository(ApplicationDbContext context)
    {
        Context = context;
    }

    public async Task<bool> SaveAsync()
    {
        return await Context.SaveChangesAsync() > 0;
    }
}