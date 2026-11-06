using AutoSales.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoSales.Infrastructure.Configurations;

internal sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("People");
        builder.Property(entity => entity.FullName).HasMaxLength(150);
        builder.Property(entity => entity.Cpf).HasMaxLength(11);
        builder.Property(entity => entity.Email).HasMaxLength(254);
        builder.Property(entity => entity.Phone).HasMaxLength(30);
        builder.HasIndex(entity => entity.Cpf).IsUnique();
    }
}
