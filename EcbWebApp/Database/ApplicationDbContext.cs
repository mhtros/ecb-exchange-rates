#nullable disable
using System.Reflection;
using EcbWebApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcbWebApp.Database;

public class ApplicationDbContext : DbContext
{
    public DbSet<CurrencyRateEntity> CurrencyRates { get; set; }
    public DbSet<WalletEntity> Wallets { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}