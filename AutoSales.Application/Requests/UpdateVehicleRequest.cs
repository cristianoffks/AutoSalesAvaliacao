using System.Text.Json.Serialization;
using AutoSales.Application.Results;
using MediatR;

namespace AutoSales.Application.Requests;

public sealed record UpdateVehicleRequest(
    string Plate,
    int ManufactureYear,
    string Brand,
    string Model) : IRequest<Result<bool>>
{
    [JsonIgnore]
    public int Id { get; set; }
}
