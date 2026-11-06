namespace AutoSales.Application.Entities;

public sealed class Person
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string Cpf { get; set; }
    public DateOnly BirthDate { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
}
