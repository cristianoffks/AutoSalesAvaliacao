using AutoSales.Application.Entities;
using AutoSales.Application.Handlers.People;
using AutoSales.Application.Repositories;
using AutoSales.Application.Requests;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutoSales.Application.Tests;

public class HandlerLoggingTests
{
    [Fact]
    public async Task ListAsync_Fails_LogsExceptionAndRethrows()
    {
        var repository = new Mock<IPersonRepository>();
        var logger = new Mock<ILogger<ListPersonHandler>>();
        var failure = new InvalidOperationException("Falha ao consultar pessoas.");
        repository.Setup(repo => repo.ListAsync("Ana", null, CancellationToken.None)).ThrowsAsync(failure);
        var handler = new ListPersonHandler(repository.Object, logger.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new ListPersonQuery("Ana", null), CancellationToken.None));

        Assert.Same(failure, exception);
        var log = Assert.Single(logger.Invocations);
        Assert.Equal(LogLevel.Error, log.Arguments[0]);
        Assert.Same(failure, log.Arguments[3]);
    }

    [Fact]
    public async Task ListAsync_Succeeds_DoesNotLog()
    {
        var repository = new Mock<IPersonRepository>();
        var logger = new Mock<ILogger<ListPersonHandler>>(MockBehavior.Strict);
        repository.Setup(repo => repo.ListAsync("Ana", null, CancellationToken.None)).ReturnsAsync(Array.Empty<Person>());
        var handler = new ListPersonHandler(repository.Object, logger.Object);

        var result = await handler.Handle(new ListPersonQuery("Ana", null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
        logger.VerifyNoOtherCalls();
    }
}
