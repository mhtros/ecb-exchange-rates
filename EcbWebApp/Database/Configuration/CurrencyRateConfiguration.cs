using EcbWebApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcbWebApp.Database.Configuration;

internal class CurrencyRateConfiguration : IEntityTypeConfiguration<CurrencyRateEntity>
{
    public void Configure(EntityTypeBuilder<CurrencyRateEntity> builder)
    {
        builder.ToTable("CurrencyRates");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.CurrencyCode).IsRequired();
        builder.Property(x => x.Rate).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => new {x.CurrencyCode, x.CreatedAt}).IsUnique();
    }
}