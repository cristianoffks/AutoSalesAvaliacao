using MediatR;
using AutoSales.Application.Results;
using AutoSales.Application.Responses;

namespace AutoSales.Application.Requests;

public sealed record CreateVehicleRequest(
    string Plate,
    int ManufactureYear,
    string Brand,
    string Model,
    int PersonId) : IRequest<Result<VehicleResponse>>;
