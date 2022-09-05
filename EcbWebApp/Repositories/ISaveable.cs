namespace EcbWebApp.Repositories;

public interface ISaveable
{
    public Task<bool> SaveAsync();
}