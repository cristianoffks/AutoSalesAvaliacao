using AutoSales.Application.Entities;

namespace AutoSales.Application.Repositories;

public interface IVehicleRepository
{
    Task<bool> ExistsByPlateAsync(string plate, int? excludedId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Vehicle>> ListAsync(
        string? brand, string? model, string? plate, int? personId, CancellationToken cancellationToken);
    Task<Vehicle?> FindAsync(int id, CancellationToken cancellationToken);
    Task AddAsync(Vehicle entity, CancellationToken cancellationToken);
    Task UpdateAsync(Vehicle entity, CancellationToken cancellationToken);
    Task DeleteAsync(Vehicle entity, CancellationToken cancellationToken);
}
