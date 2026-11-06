using AutoSales.Application.Entities;
using AutoSales.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AutoSales.Infrastructure.Repositories;

public sealed class VehicleRepository(RegistryDbContext db) : IVehicleRepository
{
    public Task<bool> ExistsByPlateAsync(string plate, int? excludedId, CancellationToken cancellationToken)
    {
        return db.Vehicles.AnyAsync(vehicle => vehicle.Plate == plate &&
            (!excludedId.HasValue || vehicle.Id != excludedId.Value), cancellationToken);
    }

    public async Task<IReadOnlyList<Vehicle>> ListAsync(
        string? brand, string? model, string? plate, int? personId, CancellationToken cancellationToken)
    {
        var query = db.Vehicles.AsNoTracking();

        if (!string.IsNullOrEmpty(brand))
        {
            query = query.Where(entity => entity.Brand.Contains(brand));
        }

        if (!string.IsNullOrEmpty(model))
        {
            query = query.Where(entity => entity.Model.Contains(model));
        }

        if (plate is not null)
        {
            query = query.Where(entity => entity.Plate == plate);
        }

        if (personId.HasValue)
        {
            query = query.Where(entity => entity.PersonId == personId.Value);
        }

        return await query.OrderBy(entity => entity.Id).ToListAsync(cancellationToken);
    }

    public Task<Vehicle?> FindAsync(int id, CancellationToken cancellationToken)
    {
        return db.Vehicles.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public async Task AddAsync(Vehicle entity, CancellationToken cancellationToken)
    {
        db.Vehicles.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Vehicle entity, CancellationToken cancellationToken)
    {
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Vehicle entity, CancellationToken cancellationToken)
    {
        db.Vehicles.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }
}
