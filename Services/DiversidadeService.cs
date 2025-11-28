using MySqlConnector;

namespace MauiApp1.Services;

public interface IDiversidadeService
{
    Task<Dictionary<string, float>> GetGenderDistributionAsync(CancellationToken ct = default);
}

public class DiversidadeService : IDiversidadeService
{
    private readonly IMySqlConnectionFactory _factory;

    public DiversidadeService(IMySqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<Dictionary<string, float>> GetGenderDistributionAsync(CancellationToken ct = default)
    {
        const string sql = @"SELECT
            `Sexo` AS gender,
            COUNT(DISTINCT `CPF`) AS total
        FROM `rhsenior_heicomp`.`rhdataset`
        WHERE `Descrição (Situação)` = 'Trabalhando'
        GROUP BY `Sexo`;";

        var result = new Dictionary<string, float>();
        await using var conn = await _factory.OpenConnectionAsync(ct);
        await using var cmd = new MySqlCommand { Connection = conn, CommandText = sql };
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var gender = reader["gender"]?.ToString() ?? "Não informado";
            var total = reader["total"] is int i ? i : Convert.ToInt32(reader["total"]);
            result[gender] = total;
        }

        return result;
    }
}
