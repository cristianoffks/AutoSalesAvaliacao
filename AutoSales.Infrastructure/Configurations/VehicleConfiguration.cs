using AutoSales.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoSales.Infrastructure.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        builder.Property(entity => entity.Plate).HasMaxLength(7);
        builder.Property(vehicle => vehicle.Brand).HasMaxLength(80).IsRequired();
        builder.Property(vehicle => vehicle.Model).HasMaxLength(100).IsRequired();
        builder.HasIndex(entity => entity.Plate).IsUnique();
        builder.HasOne<Person>().WithMany().HasForeignKey(vehicle => vehicle.PersonId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
