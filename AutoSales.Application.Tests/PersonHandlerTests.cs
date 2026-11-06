using Microsoft.Extensions.Logging.Abstractions;
using AutoSales.Application.Requests;
using AutoSales.Application.Validators;
using AutoSales.Application.Entities;
using AutoSales.Application.Repositories;
using FluentValidation;
using Moq;
using AutoSales.Application.Handlers.People;

namespace AutoSales.Application.Tests;

public class PersonHandlerTests
{
    private readonly Mock<IPersonRepository> repository = new();

    private static CreatePersonRequest CreateRequest() => new(
        " Ana Silva ", "12345678909", new DateOnly(1990, 1, 15), "ana@example.com", "11999999999");

    private static UpdatePersonRequest CreateUpdateRequest(int id) => new(
        " Ana Silva ", "12345678909", new DateOnly(1990, 1, 15), "ana@example.com", "11999999999") { Id = id };

    private static Person CreatePerson(string cpf = "12345678909") => new()
    {
        Id = 1,
        FullName = "Ana",
        Cpf = cpf,
        BirthDate = new DateOnly(1990, 1, 15),
        Email = "ana@example.com",
        Phone = "11999999999"
    };

    [Fact]
    public async Task CreateAsync_NormalizesCpfAndName()
    {
        var result = await new CreatePersonHandler(repository.Object, new PersonValidator(), NullLogger<CreatePersonHandler>.Instance).Handle(CreateRequest(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var person = Assert.IsType<Person>(result.Value);
        Assert.Equal("12345678909", person.Cpf);
        Assert.Equal("Ana Silva", person.FullName);
        repository.Verify(repo => repo.AddAsync(person, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_MaskedCpf_DoesNotSave()
    {
        var request = CreateRequest() with { Cpf = "123.456.789-09" };

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            new CreatePersonHandler(repository.Object, new PersonValidator(), NullLogger<CreatePersonHandler>.Instance)
                .Handle(request, CancellationToken.None));

        Assert.Contains(exception.Errors, error => error.PropertyName == "Cpf");
        repository.Verify(repo => repo.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_SaveFails_PreservesOriginalException()
    {
        var failure = new InvalidOperationException("Falha na conexão.");
        repository.Setup(repo => repo.AddAsync(It.IsAny<Person>(), CancellationToken.None)).ThrowsAsync(failure);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new CreatePersonHandler(repository.Object, new PersonValidator(), NullLogger<CreatePersonHandler>.Instance)
                .Handle(CreateRequest(), CancellationToken.None));

        Assert.Same(failure, exception);
    }

    [Fact]
    public async Task CreateAsync_Canceled_PropagatesCancellation()
    {
        repository.Setup(repo => repo.AddAsync(It.IsAny<Person>(), CancellationToken.None))
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            new CreatePersonHandler(repository.Object, new PersonValidator(), NullLogger<CreatePersonHandler>.Instance)
                .Handle(CreateRequest(), CancellationToken.None));

    }

    [Fact]
    public async Task CreateAsync_InvalidEmail_DoesNotSave()
    {
        var request = CreateRequest() with { Email = "invalid" };

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            new CreatePersonHandler(repository.Object, new PersonValidator(), NullLogger<CreatePersonHandler>.Instance).Handle(request, CancellationToken.None));

        Assert.Contains(exception.Errors, error => error.PropertyName == "Email");
        repository.Verify(repo => repo.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ExistingCpf_ReturnsFailureWithoutSaving()
    {
        repository.Setup(repo => repo.ExistsByCpfAsync("12345678909", CancellationToken.None)).ReturnsAsync(true);

        var result = await new CreatePersonHandler(repository.Object, new PersonValidator(), NullLogger<CreatePersonHandler>.Instance)
            .Handle(CreateRequest(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Cpf", result.Field);
        repository.Verify(repo => repo.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_MissingPerson_ReturnsFailureAndLogsWarning()
    {
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<DeletePersonHandler>>();
        var handler = new DeletePersonHandler(repository.Object, logger.Object);

        var result = await handler.Handle(new DeletePersonRequest(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Pessoa não encontrada.", result.Message);
        repository.Verify(repo => repo.DeleteAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
        var log = Assert.Single(logger.Invocations);
        Assert.Equal(Microsoft.Extensions.Logging.LogLevel.Warning, log.Arguments[0]);
    }

    [Fact]
    public async Task DeleteAsync_ExistingPerson_DeletesAndReturnsSuccess()
    {
        var person = CreatePerson();
        repository.Setup(repo => repo.FindAsync(1, CancellationToken.None)).ReturnsAsync(person);
        var handler = new DeletePersonHandler(repository.Object, NullLogger<DeletePersonHandler>.Instance);

        var result = await handler.Handle(new DeletePersonRequest(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        repository.Verify(repo => repo.DeleteAsync(person, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task FindAsync_MissingPerson_ReturnsFailureAndLogsWarning()
    {
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<GetPersonHandler>>();
        var handler = new GetPersonHandler(repository.Object, logger.Object);

        var result = await handler.Handle(new GetPersonRequest(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Pessoa não encontrada.", result.Message);
        var log = Assert.Single(logger.Invocations);
        Assert.Equal(Microsoft.Extensions.Logging.LogLevel.Warning, log.Arguments[0]);
    }

    [Fact]
    public async Task UpdateAsync_ChangedCpf_DoesNotSave()
    {
        var person = CreatePerson(cpf: "98765432100");
        repository.Setup(repo => repo.FindAsync(1, CancellationToken.None)).ReturnsAsync(person);

        var result = await new UpdatePersonHandler(repository.Object, new PersonValidator(), NullLogger<UpdatePersonHandler>.Instance)
            .Handle(CreateUpdateRequest(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(AutoSales.Application.Results.ErrorType.Validation, result.ErrorType);
        Assert.Equal("Cpf", result.Field);
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_MissingPerson_ReturnsNotFound()
    {
        var result = await new UpdatePersonHandler(repository.Object, new PersonValidator(), NullLogger<UpdatePersonHandler>.Instance)
            .Handle(CreateUpdateRequest(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(AutoSales.Application.Results.ErrorType.NotFound, result.ErrorType);
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_SameCpf_UpdatesPerson()
    {
        var person = CreatePerson();
        repository.Setup(repo => repo.FindAsync(1, CancellationToken.None)).ReturnsAsync(person);

        var result = await new UpdatePersonHandler(repository.Object, new PersonValidator(), NullLogger<UpdatePersonHandler>.Instance)
            .Handle(CreateUpdateRequest(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("12345678909", person.Cpf);
        repository.Verify(repo => repo.UpdateAsync(person, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ListAsync_NoFilters_ReturnsAllPeople()
    {
        var people = new[] { CreatePerson() };
        repository.Setup(repo => repo.ListAsync(null, null, CancellationToken.None)).ReturnsAsync(people);
        var handler = new ListPersonHandler(repository.Object, NullLogger<ListPersonHandler>.Instance);

        var result = await handler.Handle(new ListPersonQuery(" ", null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(people, result.Value);
        repository.Verify(repo => repo.ListAsync(null, null, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ListAsync_NormalizesSearchFilters()
    {
        await new ListPersonHandler(repository.Object, NullLogger<ListPersonHandler>.Instance).Handle(new ListPersonQuery(" Ana ", "123.456.789-09"), CancellationToken.None);

        repository.Verify(repo => repo.ListAsync("Ana", "12345678909", CancellationToken.None), Times.Once);
    }
}
