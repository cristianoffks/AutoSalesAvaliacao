using AutoSales.Application.Results;
using Microsoft.Extensions.Logging.Abstractions;
using AutoSales.Application.Entities;
using AutoSales.Application.Repositories;
using AutoSales.Application.Requests;
using MediatR;
using Moq;
using AutoSales.Application.Handlers.People;

namespace AutoSales.Application.Tests;

public class MediatorTests
{
    [Fact]
    public async Task Send_GetPersonRequest_CallsGetPersonHandler()
    {
        var person = new Person
        {
            Id = 1, FullName = "Ana Silva", Cpf = "12345678909",
            Email = "ana@example.com", Phone = "11999999999"
        };
        var repository = new Mock<IPersonRepository>();
        repository.Setup(repo => repo.FindAsync(1, CancellationToken.None)).ReturnsAsync(person);
        var provider = new Mock<IServiceProvider>();
        provider.Setup(service => service.GetService(typeof(IRequestHandler<GetPersonRequest, Result<Person>>)))
            .Returns(new GetPersonHandler(repository.Object, NullLogger<GetPersonHandler>.Instance));
        provider.Setup(service => service.GetService(typeof(IEnumerable<IPipelineBehavior<GetPersonRequest, Result<Person>>>)))
            .Returns(Array.Empty<IPipelineBehavior<GetPersonRequest, Result<Person>>>());
        var mediator = new Mediator(provider.Object);

        var result = await mediator.Send(new GetPersonRequest(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(person, result.Value);
        repository.Verify(repo => repo.FindAsync(1, CancellationToken.None), Times.Once);
    }
}
