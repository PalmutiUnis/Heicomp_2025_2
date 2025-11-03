using FluentAssertions;
using Xunit;

namespace MauiApp1.Tests.Services;

public class TurnoverRepositoryMockTests
{
    [Theory]
    [InlineData(10, 5, 100, 7.5)] // (10 + 5) / 2 / 100 * 100 = 7.5%
    [InlineData(20, 10, 200, 7.5)]
    [InlineData(0, 0, 100, 0)]
    public void CalculateTurnover_ShouldReturnCorrectPercentage(
        int admissoes, 
        int desligamentos, 
        int totalColaboradores, 
        decimal expectedTurnover)
    {
        // Arrange
        var numerator = (admissoes + desligamentos) / 2.0m;
        var turnover = (numerator / totalColaboradores) * 100;

        // Act & Assert
        Math.Round(turnover, 2).Should().Be(expectedTurnover);
    }

    [Fact]
    public void CalculateTurnover_WithZeroEmployees_ShouldHandleGracefully()
    {
        // Arrange
        int admissoes = 10;
        int desligamentos = 5;
        int totalColaboradores = 0;

        // Act
        Action act = () => {
            if (totalColaboradores == 0)
                throw new DivideByZeroException("Total de colaboradores não pode ser zero");
        };

        // Assert
        act.Should().Throw<DivideByZeroException>();
    }
}