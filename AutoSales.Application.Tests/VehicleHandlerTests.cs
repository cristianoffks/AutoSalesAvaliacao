using Microsoft.Extensions.Logging.Abstractions;
using AutoSales.Application.Requests;
using AutoSales.Application.Validators;
using AutoSales.Application.Entities;
using AutoSales.Application.Repositories;
using FluentValidation;
using Moq;
using AutoSales.Application.Handlers.Vehicles;
using AutoSales.Application.Results;

namespace AutoSales.Application.Tests;

public class VehicleHandlerTests
{
    private readonly Mock<IVehicleRepository> repository = new();
    private readonly Mock<IPersonRepository> people = new();

    public VehicleHandlerTests()
    {
        people.Setup(repo => repo.FindAsync(1, CancellationToken.None)).ReturnsAsync(new Person
        {
            Id = 1, FullName = "Ana Silva", Cpf = "12345678909", Email = "ana@example.com", Phone = "11999999999"
        });
    }

    private static CreateVehicleRequest CreateRequest() => new("abc-1234", 2020, " Honda ", " Civic ", 1);

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData(" ", " ")]
    public async Task ListAsync_NoFilters_ReturnsAllVehicles(string? brand, string? model)
    {
        var vehicle = new Vehicle
        {
            Id = 10, Plate = "ABC1234", PersonId = 1, Brand = "Honda", Model = "Civic", ManufactureYear = 2020
        };
        repository.Setup(repo => repo.ListAsync(null, null, null, null, CancellationToken.None))
            .ReturnsAsync(new[] { vehicle });
        var handler = new ListVehicleHandler(repository.Object, NullLogger<ListVehicleHandler>.Instance);

        var result = await handler.Handle(new ListVehicleQuery(brand, model, null, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!);
        Assert.Equal(vehicle.Id, item.Id);
        Assert.Equal(vehicle.Plate, item.Plate);
        Assert.Equal(vehicle.Brand, item.Brand);
        Assert.Equal(vehicle.Model, item.Model);
        Assert.Equal(vehicle.ManufactureYear, item.ManufactureYear);
        Assert.Equal(vehicle.PersonId, item.PersonId);
        repository.Verify(repo => repo.ListAsync(null, null, null, null, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData("Honda", null, null, null)]
    [InlineData(null, "Civic", null, null)]
    [InlineData(null, null, "ABC1234", null)]
    [InlineData(null, null, null, 1)]
    [InlineData(null, null, null, 0)]
    [InlineData(null, null, null, -1)]
    public async Task ListAsync_WithFilter_ReturnsSuccess(
        string? brand, string? model, string? plate, int? personId)
    {
        repository.Setup(repo => repo.ListAsync(brand, model, plate, personId, CancellationToken.None))
            .ReturnsAsync(Array.Empty<Vehicle>());
        var handler = new ListVehicleHandler(repository.Object, NullLogger<ListVehicleHandler>.Instance);

        var result = await handler.Handle(new ListVehicleQuery(brand, model, plate, personId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
        repository.Verify(repo => repo.ListAsync(brand, model, plate, personId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetAsync_MissingVehicle_ReturnsNotFoundAndLogsWarning()
    {
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<GetVehicleHandler>>();
        var handler = new GetVehicleHandler(repository.Object, logger.Object);

        var result = await handler.Handle(new GetVehicleRequest(10), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        var log = Assert.Single(logger.Invocations);
        Assert.Equal(Microsoft.Extensions.Logging.LogLevel.Warning, log.Arguments[0]);
    }

    [Fact]
    public async Task GetAsync_ExistingVehicle_ReturnsVehicle()
    {
        var vehicle = new Vehicle
        {
            Id = 10, Plate = "ABC1234", PersonId = 1, Brand = "Honda", Model = "Civic", ManufactureYear = 2020
        };
        repository.Setup(repo => repo.FindAsync(10, CancellationToken.None)).ReturnsAsync(vehicle);
        var handler = new GetVehicleHandler(repository.Object, NullLogger<GetVehicleHandler>.Instance);

        var result = await handler.Handle(new GetVehicleRequest(10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value!.Id);
        Assert.Equal("ABC1234", result.Value.Plate);
    }

    [Fact]
    public async Task UpdateAsync_MissingVehicle_ReturnsNotFoundWithoutSaving()
    {
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<UpdateVehicleHandler>>();
        var handler = new UpdateVehicleHandler(repository.Object, new UpdateVehicleValidator(), logger.Object);

        var result = await handler.Handle(new UpdateVehicleRequest("ABC1234", 2020, "Honda", "Civic") with { Id = 10 }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(AutoSales.Application.Results.ErrorType.NotFound, result.ErrorType);
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Never);
        var log = Assert.Single(logger.Invocations);
        Assert.Equal(Microsoft.Extensions.Logging.LogLevel.Warning, log.Arguments[0]);
    }

    [Fact]
    public async Task DeleteAsync_MissingVehicle_ReturnsNotFoundAndLogsWarning()
    {
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<DeleteVehicleHandler>>();
        var handler = new DeleteVehicleHandler(repository.Object, logger.Object);

        var result = await handler.Handle(new DeleteVehicleRequest(10), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(AutoSales.Application.Results.ErrorType.NotFound, result.ErrorType);
        Assert.Equal("Veículo não encontrado.", result.Message);
        repository.Verify(repo => repo.DeleteAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Never);
        var log = Assert.Single(logger.Invocations);
        Assert.Equal(Microsoft.Extensions.Logging.LogLevel.Warning, log.Arguments[0]);
    }

    [Fact]
    public async Task DeleteAsync_ExistingVehicle_DeletesAndReturnsSuccess()
    {
        var vehicle = new Vehicle { Id = 10, Plate = "ABC1234", PersonId = 1 };
        repository.Setup(repo => repo.FindAsync(10, CancellationToken.None)).ReturnsAsync(vehicle);
        var handler = new DeleteVehicleHandler(repository.Object, NullLogger<DeleteVehicleHandler>.Instance);

        var result = await handler.Handle(new DeleteVehicleRequest(10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        repository.Verify(repo => repo.DeleteAsync(vehicle, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NormalizesPlateAndSavesOwner()
    {
        repository.Setup(repo => repo.AddAsync(It.IsAny<Vehicle>(), CancellationToken.None))
            .Callback<Vehicle, CancellationToken>((vehicle, token) => vehicle.Id = 10);

        var result = await new CreateVehicleHandler(repository.Object, new CreateVehicleValidator(), people.Object, NullLogger<CreateVehicleHandler>.Instance).Handle(CreateRequest(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var vehicle = Assert.IsType<AutoSales.Application.Responses.VehicleResponse>(result.Value);
        Assert.Equal(10, vehicle.Id);
        Assert.Equal("ABC1234", vehicle.Plate);
        Assert.Equal(1, vehicle.PersonId);
        repository.Verify(repo => repo.AddAsync(It.Is<Vehicle>(vehicle => vehicle.PersonId == 1 && vehicle.Brand == "Honda" && vehicle.Model == "Civic"),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InvalidPlate_DoesNotSave()
    {
        var request = CreateRequest() with { Plate = "123" };

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            new CreateVehicleHandler(repository.Object, new CreateVehicleValidator(), people.Object, NullLogger<CreateVehicleHandler>.Instance).Handle(request, CancellationToken.None));

        Assert.Contains(exception.Errors, error => error.PropertyName == "Plate");
        repository.Verify(repo => repo.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_MissingOwner_DoesNotSave()
    {
        var request = CreateRequest() with { PersonId = 2 };

        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<CreateVehicleHandler>>();
        var result = await new CreateVehicleHandler(repository.Object, new CreateVehicleValidator(), people.Object, logger.Object)
            .Handle(request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Proprietário não encontrado.", result.Message);
        Assert.Equal(AutoSales.Application.Results.ErrorType.NotFound, result.ErrorType);
        var log = Assert.Single(logger.Invocations);
        Assert.Equal(Microsoft.Extensions.Logging.LogLevel.Warning, log.Arguments[0]);
        repository.Verify(repo => repo.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ExistingPlate_ReturnsFailureWithoutSaving()
    {
        repository.Setup(repo => repo.ExistsByPlateAsync("ABC1234", null, CancellationToken.None)).ReturnsAsync(true);

        var result = await new CreateVehicleHandler(repository.Object, new CreateVehicleValidator(), people.Object, NullLogger<CreateVehicleHandler>.Instance)
            .Handle(CreateRequest(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Plate", result.Field);
        repository.Verify(repo => repo.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_DuplicatePlate_ReturnsFailureWithoutSaving()
    {
        repository.Setup(repo => repo.FindAsync(10, CancellationToken.None))
            .ReturnsAsync(new Vehicle { Id = 10, Plate = "ABC1234", PersonId = 1 });
        repository.Setup(repo => repo.ExistsByPlateAsync("DEF5678", 10, CancellationToken.None)).ReturnsAsync(true);

        var result = await new UpdateVehicleHandler(repository.Object, new UpdateVehicleValidator(), NullLogger<UpdateVehicleHandler>.Instance)
            .Handle(new UpdateVehicleRequest("DEF5678", 2021, "Honda", "Civic") with { Id = 10 },
                CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Plate", result.Field);
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_PreservesOwner()
    {
        var vehicle = new Vehicle { Id = 10, Plate = "ABC1234", PersonId = 2 };
        repository.Setup(repo => repo.FindAsync(10, CancellationToken.None)).ReturnsAsync(vehicle);
        var request = new UpdateVehicleRequest("DEF5678", 2021, "Honda", "Civic");

        var result = await new UpdateVehicleHandler(repository.Object, new UpdateVehicleValidator(), NullLogger<UpdateVehicleHandler>.Instance)
            .Handle(request with { Id = 10 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, vehicle.PersonId);
        Assert.Equal("DEF5678", vehicle.Plate);
        Assert.Equal("Honda", vehicle.Brand);
        Assert.Equal("Civic", vehicle.Model);
        repository.Verify(repo => repo.UpdateAsync(vehicle, CancellationToken.None), Times.Once);
        repository.Verify(repo => repo.ExistsByPlateAsync("DEF5678", 10, CancellationToken.None), Times.Once);
    }
}
