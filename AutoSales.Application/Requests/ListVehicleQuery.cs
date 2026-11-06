using MediatR;
using AutoSales.Application.Responses;
using AutoSales.Application.Results;

namespace AutoSales.Application.Requests;

public sealed record ListVehicleQuery(string? Brand, string? Model, string? Plate, int? PersonId) : IRequest<Result<IReadOnlyList<VehicleResponse>>>;
