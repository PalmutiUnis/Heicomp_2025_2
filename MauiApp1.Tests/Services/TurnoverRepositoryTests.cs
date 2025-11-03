using FluentAssertions;
using MauiApp1.Services;
using MySqlConnector;
using Xunit;

namespace MauiApp1.Tests.Services;

public class TurnoverRepositoryTests : IDisposable
{
    private readonly string _connectionString;
    private readonly TurnoverRepository _repository;

    public TurnoverRepositoryTests()
    {
        // Connection string de teste - AJUSTE PARA SEU AMBIENTE
        _connectionString = "Server=localhost;Database=rhsenior_heicomp_test;User=root;Password=sua_senha;";
        _repository = new TurnoverRepository(_connectionString);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithConnectionString()
    {
        // Arrange & Act
        var repository = new TurnoverRepository(_connectionString);

        // Assert
        repository.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTurnoverAsync_ShouldReturnDecimalValue()
    {
        // Arrange

        // Act
        var result = await _repository.GetTurnoverAsync();

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
        result.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public async Task GetAdmissoesUltimoAnoAsync_ShouldReturnPositiveInteger()
    {
        // Act
        var result = await _repository.GetAdmissoesUltimoAnoAsync();

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetDesligamentosUltimoAnoAsync_ShouldReturnPositiveInteger()
    {
        // Act
        var result = await _repository.GetDesligamentosUltimoAnoAsync();

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetTotalColaboradoresAsync_ShouldReturnPositiveInteger()
    {
        // Act
        var result = await _repository.GetTotalColaboradoresAsync();

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetTurnoverAsync_WithInvalidConnection_ShouldThrowException()
    {
        // Arrange
        var invalidRepository = new TurnoverRepository("Server=invalid;Database=test;");

        // Act & Assert
        await Assert.ThrowsAsync<MySqlException>(async () => 
            await invalidRepository.GetTurnoverAsync());
    }

    [Fact]
    public async Task GetTurnoverAsync_WithEmptyDatabase_ShouldReturnZero()
    {
        // Arrange
        // (Assumindo uma base vazia ou sem dados no período)
        
        // Act
        var result = await _repository.GetTurnoverAsync();

        // Assert
        result.Should().Be(0);
    }

    public void Dispose()
    {
        // Cleanup se necessário
    }
}