using System.Text.Json.Serialization;
using AutoSales.Application.Results;
using MediatR;

namespace AutoSales.Application.Requests;

public sealed record UpdatePersonRequest(
    string FullName,
    string Cpf,
    DateOnly? BirthDate,
    string Email,
    string Phone) : IRequest<Result<bool>>
{
    [JsonIgnore]
    public int Id { get; set; }
}
