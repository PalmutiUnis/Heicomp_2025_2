using MySqlConnector;

namespace MauiApp1.Services;

public interface ICargosService
{
    Task<IReadOnlyList<CargoCategoriaDto>> GetCargoCategoriasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CategoriaTotalDto>> GetCategoriaTotaisAsync(CancellationToken ct = default);
}

public class CargosService : ICargosService
{
    private readonly IMySqlConnectionFactory _factory;

    public CargosService(IMySqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<CargoCategoriaDto>> GetCargoCategoriasAsync(CancellationToken ct = default)
    {
        const string sql = @"SELECT
          `Título Reduzido (Cargo)` AS cargo,
          COUNT(DISTINCT `CPF`) AS colaboradores,
          CASE
            WHEN (
              (LOWER(`Título Reduzido (Cargo)`) LIKE '%professor%' OR LOWER(`Título Reduzido (Cargo)`) LIKE '%docente%')
              AND (
                LOWER(`Título Reduzido (Cargo)`) LIKE '%coordenador%'
                OR LOWER(`Título Reduzido (Cargo)`) LIKE '%analista%'
                OR LOWER(`Título Reduzido (Cargo)`) LIKE '%assistente%'
                OR LOWER(`Título Reduzido (Cargo)`) LIKE '%administrativo%'
              )
            ) THEN 'Dois Cargos'
            WHEN (LOWER(`Título Reduzido (Cargo)`) LIKE '%professor%' OR LOWER(`Título Reduzido (Cargo)`) LIKE '%docente%') THEN 'Docente'
            ELSE 'Administrativo' END AS categoria
        FROM `rhsenior_heicomp`.`rhdataset`
        WHERE `Descrição (Situação)` = 'Trabalhando'
        GROUP BY cargo, categoria
        ORDER BY colaboradores DESC, cargo ASC;";

        var list = new List<CargoCategoriaDto>();

        // ✅ Conecta explicitamente ao banco RHSenior
        await using var conn = await _factory.OpenConnectionAsync("RHSenior", ct);

        await using var cmd = new MySqlCommand { Connection = conn, CommandText = sql };
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var dto = new CargoCategoriaDto
            {
                Cargo = reader["cargo"]?.ToString() ?? string.Empty,
                Colaboradores = reader["colaboradores"] is int i ? i : Convert.ToInt32(reader["colaboradores"]),
                Categoria = reader["categoria"]?.ToString() ?? string.Empty
            };
            list.Add(dto);
        }

        return list;
    }

    public async Task<IReadOnlyList<CategoriaTotalDto>> GetCategoriaTotaisAsync(CancellationToken ct = default)
    {
        const string sql = @"WITH base AS (
          SELECT DISTINCT `CPF`,
            MAX(CASE WHEN LOWER(`Título Reduzido (Cargo)`) LIKE '%professor%' OR LOWER(`Título Reduzido (Cargo)`) LIKE '%docente%' THEN 1 ELSE 0 END) AS is_docente,
            MAX(CASE WHEN LOWER(`Título Reduzido (Cargo)`) LIKE '%coordenador%' OR LOWER(`Título Reduzido (Cargo)`) LIKE '%analista%' OR LOWER(`Título Reduzido (Cargo)`) LIKE '%assistente%' OR LOWER(`Título Reduzido (Cargo)`) LIKE '%administrativo%' THEN 1 ELSE 0 END) AS is_adm
          FROM `rhsenior_heicomp`.`rhdataset`
          WHERE `Descrição (Situação)` = 'Trabalhando'
          GROUP BY `CPF`
        )
        SELECT categoria, total FROM (
          SELECT
            CASE
              WHEN is_docente = 1 AND is_adm = 1 THEN 'Dois Cargos'
              WHEN is_docente = 1 THEN 'Docente'
              ELSE 'Administrativo' END AS categoria,
            COUNT(*) AS total
          FROM base
          GROUP BY categoria
          UNION ALL
          SELECT 'Total' AS categoria, COUNT(*) AS total FROM base
        ) t;";

        var list = new List<CategoriaTotalDto>();

        // ✅ Conecta explicitamente ao banco RHSenior
        await using var conn = await _factory.OpenConnectionAsync("RHSenior", ct);

        await using var cmd = new MySqlCommand { Connection = conn, CommandText = sql };
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var dto = new CategoriaTotalDto
            {
                Categoria = reader["categoria"]?.ToString() ?? string.Empty,
                Total = reader["total"] is int i ? i : Convert.ToInt32(reader["total"])
            };
            list.Add(dto);
        }

        return list;
    }
}
