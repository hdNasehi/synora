using Microsoft.EntityFrameworkCore;
using Orange.Infrastructure.Persistence;
using Testcontainers.MsSql;

namespace Orange.IntegrationTests;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateContext();
        await db.Database.EnsureCreatedAsync();
    }

    public OrangeDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OrangeDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new OrangeDbContext(options);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>;
