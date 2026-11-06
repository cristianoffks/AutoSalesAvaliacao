using AutoSales.Application.Entities;

namespace AutoSales.Application.Repositories;

public interface IPersonRepository
{
    Task<bool> ExistsByCpfAsync(string cpf, CancellationToken cancellationToken);
    Task<IReadOnlyList<Person>> ListAsync(string? name, string? cpf, CancellationToken cancellationToken);
    Task<Person?> FindAsync(int id, CancellationToken cancellationToken);
    Task AddAsync(Person entity, CancellationToken cancellationToken);
    Task UpdateAsync(Person entity, CancellationToken cancellationToken);
    Task DeleteAsync(Person entity, CancellationToken cancellationToken);
}
