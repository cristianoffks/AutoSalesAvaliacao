using AutoSales.Application.Entities;
using AutoSales.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AutoSales.Infrastructure.Repositories;

public sealed class PersonRepository(RegistryDbContext db) : IPersonRepository
{
    public Task<bool> ExistsByCpfAsync(string cpf, CancellationToken cancellationToken)
    {
        return db.People.AnyAsync(person => person.Cpf == cpf, cancellationToken);
    }

    public async Task<IReadOnlyList<Person>> ListAsync(string? name, string? cpf, CancellationToken cancellationToken)
    {
        var query = db.People.AsNoTracking();

        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(entity => entity.FullName.Contains(name));
        }

        if (cpf is not null)
        {
            query = query.Where(entity => entity.Cpf == cpf);
        }

        return await query.OrderBy(entity => entity.Id).ToListAsync(cancellationToken);
    }

    public Task<Person?> FindAsync(int id, CancellationToken cancellationToken)
    {
        return db.People.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public async Task AddAsync(Person entity, CancellationToken cancellationToken)
    {
        db.People.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Person entity, CancellationToken cancellationToken)
    {
        db.People.Update(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Person entity, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.Vehicles.Where(vehicle => vehicle.PersonId == entity.Id)
            .ExecuteDeleteAsync(cancellationToken);
        db.People.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
