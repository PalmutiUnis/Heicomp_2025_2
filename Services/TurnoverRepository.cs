using MySqlConnector;

namespace MauiApp1.Services
{
    public class TurnoverRepository
    {
        private readonly IMySqlConnectionFactory _factory;

        public TurnoverRepository(IMySqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<TurnoverCompleto> GetTurnoverCompletoAsync()
        {
            string query = @"
                WITH
                data_limite AS (
                    SELECT DATE_SUB(CURDATE(), INTERVAL 1 YEAR) AS data_um_ano_atras
                ),
                dados_processados AS (
                    SELECT
                        `CPF`,
                        `Descrição (Situação)`,
                        CASE
                            WHEN `Admissão` IS NOT NULL AND `Admissão` != ''
                            THEN STR_TO_DATE(`Admissão`, '%d/%m/%Y')
                            ELSE NULL
                        END as admissao_date,
                        CASE
                            WHEN `Data Afastamento` IS NOT NULL AND `Data Afastamento` != ''
                            THEN STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y')
                            ELSE NULL
                        END as afastamento_date
                    FROM rhsenior_heicomp.rhdataset
                ),
                metricas_turnover AS (
                    SELECT
                        COUNT(*) as total_colaboradores,
                        SUM(CASE
                            WHEN admissao_date >= (SELECT data_um_ano_atras FROM data_limite)
                            THEN 1 ELSE 0
                        END) as admissoes_ultimo_ano,
                        SUM(CASE
                            WHEN `Descrição (Situação)` LIKE 'Dem%'
                                 AND afastamento_date >= (SELECT data_um_ano_atras FROM data_limite)
                            THEN 1 ELSE 0
                        END) as desligamentos_ultimo_ano
                    FROM dados_processados
                    WHERE admissao_date IS NOT NULL
                )
                SELECT
                    ROUND(
                        ((admissoes_ultimo_ano + desligamentos_ultimo_ano) / 2)
                        / total_colaboradores
                        * 100, 2
                    ) AS Turnover,
                    admissoes_ultimo_ano AS Admissoes,
                    desligamentos_ultimo_ano AS Desligamentos,
                    total_colaboradores AS TotalColaboradores
                FROM metricas_turnover";

            await using var connection = await _factory.OpenConnectionAsync();
            await using var command = new MySqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new TurnoverCompleto
                {
                    Turnover = reader["Turnover"] != DBNull.Value ? Convert.ToDecimal(reader["Turnover"]) : 0,
                    Admissoes = reader["Admissoes"] != DBNull.Value ? Convert.ToInt32(reader["Admissoes"]) : 0,
                    Desligamentos = reader["Desligamentos"] != DBNull.Value ? Convert.ToInt32(reader["Desligamentos"]) : 0,
                    TotalColaboradores = reader["TotalColaboradores"] != DBNull.Value ? Convert.ToInt32(reader["TotalColaboradores"]) : 0
                };
            }

            return new TurnoverCompleto();
        }

        // Métodos individuais mantidos para compatibilidade (agora mais rápidos)
        public async Task<decimal> GetTurnoverAsync()
        {
            var resultado = await GetTurnoverCompletoAsync();
            return resultado.Turnover;
        }

        public async Task<int> GetAdmissoesUltimoAnoAsync()
        {
            var resultado = await GetTurnoverCompletoAsync();
            return resultado.Admissoes;
        }

        public async Task<int> GetDesligamentosUltimoAnoAsync()
        {
            var resultado = await GetTurnoverCompletoAsync();
            return resultado.Desligamentos;
        }

        public async Task<int> GetTotalColaboradoresAsync()
        {
            var resultado = await GetTurnoverCompletoAsync();
            return resultado.TotalColaboradores;
        }

        public async Task<string?> QueryVersionAsync()
        {
            await using var connection = await _factory.OpenConnectionAsync();
            await using var command = new MySqlCommand("SELECT VERSION();", connection);
            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }
    }

    // DTO para retornar todos os dados de uma vez
    public class TurnoverCompleto
    {
        public decimal Turnover { get; set; }
        public int Admissoes { get; set; }
        public int Desligamentos { get; set; }
        public int TotalColaboradores { get; set; }
    }
}
