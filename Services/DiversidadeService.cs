using MySqlConnector;
using MauiApp1.Models;
using MauiApp1.Services;
using System.Diagnostics;

namespace MauiApp1.Services
{
    public class DiversidadeService
    {
        private readonly IMySqlConnectionFactory _connectionFactory;

        public DiversidadeService(IMySqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        /// <summary>
        /// Obtém dados gerais de diversidade para o ano especificado
        /// </summary>
        public async Task<DiversidadeGeral> ObterDadosGeraisAsync(int ano)
        {
            try
            {
                using var conexao = await _connectionFactory.OpenConnectionAsync();

                var query = @"
                    WITH base_colaboradores AS (
                        SELECT * FROM rhdataset 
                        WHERE STR_TO_DATE(`Admissão`, '%d/%m/%Y') <= STR_TO_DATE(CONCAT(@ano, '-12-31'), '%Y-%m-%d')
                        AND (`Data Afastamento` = '00/00/0000'
                             OR STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y') >= STR_TO_DATE(CONCAT(@ano, '-01-01'), '%Y-%m-%d'))
                    )
                    SELECT
                        COUNT(*) AS total,
                        ROUND(100 * SUM(CASE WHEN Sexo = 'Masculino' THEN 1 ELSE 0 END) / COUNT(*), 1) AS percentual_homens,
                        ROUND(100 * SUM(CASE WHEN Sexo = 'Feminino' THEN 1 ELSE 0 END) / COUNT(*), 1) AS percentual_mulheres,
                        ROUND(100 * SUM(CASE WHEN Sexo NOT IN ('Masculino', 'Feminino') OR Sexo IS NULL THEN 1 ELSE 0 END) / COUNT(*), 1) AS percentual_nao_informado
                    FROM base_colaboradores;";

                using var comando = new MySqlCommand(query, conexao);
                comando.Parameters.AddWithValue("@ano", ano);

                using var reader = await comando.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new DiversidadeGeral
                    {
                        TotalColaboradores = reader.GetInt32("total"),
                        PercentualHomens = reader.GetDecimal("percentual_homens"),
                        PercentualMulheres = reader.GetDecimal("percentual_mulheres"),
                        PercentualNaoInformado = reader.GetDecimal("percentual_nao_informado")
                    };
                }

                return new DiversidadeGeral();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DiversidadeService] Erro ao obter dados gerais: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtém distribuição por gênero
        /// </summary>
        public async Task<List<DistribuicaoGenero>> ObterDistribuicaoGeneroAsync(int ano)
        {
            var lista = new List<DistribuicaoGenero>();

            try
            {
                using var conexao = await _connectionFactory.OpenConnectionAsync();

                var query = @"
                    WITH base_colaboradores AS (
                        SELECT * FROM rhdataset 
                        WHERE STR_TO_DATE(`Admissão`, '%d/%m/%Y') <= STR_TO_DATE(CONCAT(@ano, '-12-31'), '%Y-%m-%d')
                        AND (`Data Afastamento` = '00/00/0000'
                             OR STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y') >= STR_TO_DATE(CONCAT(@ano, '-01-01'), '%Y-%m-%d'))
                    )
                    SELECT
                        COALESCE(`Sexo`, 'Não Informado') AS Sexo,
                        COUNT(*) AS quantidade,
                        ROUND(100 * COUNT(*) / (SELECT COUNT(*) FROM base_colaboradores), 1) AS percentual
                    FROM base_colaboradores 
                    GROUP BY Sexo
                    ORDER BY quantidade DESC;";

                using var comando = new MySqlCommand(query, conexao);
                comando.Parameters.AddWithValue("@ano", ano);

                using var reader = await comando.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    lista.Add(new DistribuicaoGenero
                    {
                        Sexo = reader.GetString("Sexo"),
                        Quantidade = reader.GetInt32("quantidade"),
                        Percentual = reader.GetDecimal("percentual")
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DiversidadeService] Erro ao obter distribuição por gênero: {ex.Message}");
                throw;
            }

            return lista;
        }

        /// <summary>
        /// Obtém distribuição de Pessoas com Deficiência
        /// </summary>
        public async Task<List<DistribuicaoPCD>> ObterDistribuicaoPCDAsync(int ano)
        {
            var lista = new List<DistribuicaoPCD>();

            try
            {
                using var conexao = await _connectionFactory.OpenConnectionAsync();

                var query = @"
                    WITH base_colaboradores AS (
                        SELECT * FROM rhdataset 
                        WHERE STR_TO_DATE(`Admissão`, '%d/%m/%Y') <= STR_TO_DATE(CONCAT(@ano, '-12-31'), '%Y-%m-%d')
                        AND (`Data Afastamento` = '00/00/0000'
                             OR STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y') >= STR_TO_DATE(CONCAT(@ano, '-01-01'), '%Y-%m-%d'))
                    )
                    SELECT
                        COALESCE(`Descrição (Deficiência)`, 'Não Informado') AS Descricao,
                        COUNT(*) AS quantidade,
                        ROUND(100 * COUNT(*) / (SELECT COUNT(*) FROM base_colaboradores), 1) AS percentual
                    FROM base_colaboradores 
                    GROUP BY `Descrição (Deficiência)`
                    ORDER BY percentual DESC;";

                using var comando = new MySqlCommand(query, conexao);
                comando.Parameters.AddWithValue("@ano", ano);

                using var reader = await comando.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    lista.Add(new DistribuicaoPCD
                    {
                        Descricao = reader.GetString("Descricao"),
                        Quantidade = reader.GetInt32("quantidade"),
                        Percentual = reader.GetDecimal("percentual")
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DiversidadeService] Erro ao obter distribuição PCD: {ex.Message}");
                throw;
            }

            return lista;
        }

        /// <summary>
        /// Obtém distribuição por Raça/Etnia
        /// </summary>
        public async Task<List<DistribuicaoRacaEtnia>> ObterDistribuicaoRacaEtniaAsync(int ano)
        {
            var lista = new List<DistribuicaoRacaEtnia>();

            try
            {
                using var conexao = await _connectionFactory.OpenConnectionAsync();

                var query = @"
                    WITH base_colaboradores AS (
                        SELECT * FROM rhdataset 
                        WHERE STR_TO_DATE(`Admissão`, '%d/%m/%Y') <= STR_TO_DATE(CONCAT(@ano, '-12-31'), '%Y-%m-%d')
                        AND (`Data Afastamento` = '00/00/0000'
                             OR STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y') >= STR_TO_DATE(CONCAT(@ano, '-01-01'), '%Y-%m-%d'))
                    )
                    SELECT
                        COALESCE(`Descrição (Raça/Etnia)`, 'Não Informado') AS Descricao,
                        COUNT(*) AS quantidade,
                        ROUND(100 * COUNT(*) / (SELECT COUNT(*) FROM base_colaboradores), 1) AS percentual
                    FROM base_colaboradores 
                    GROUP BY `Descrição (Raça/Etnia)`
                    ORDER BY percentual DESC;";

                using var comando = new MySqlCommand(query, conexao);
                comando.Parameters.AddWithValue("@ano", ano);

                using var reader = await comando.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    lista.Add(new DistribuicaoRacaEtnia
                    {
                        Descricao = reader.GetString("Descricao"),
                        Quantidade = reader.GetInt32("quantidade"),
                        Percentual = reader.GetDecimal("percentual")
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DiversidadeService] Erro ao obter distribuição Raça/Etnia: {ex.Message}");
                throw;
            }

            return lista;
        }

        /// <summary>
        /// Obtém distribuição por Estado Civil
        /// </summary>
        public async Task<List<DistribuicaoEstadoCivil>> ObterDistribuicaoEstadoCivilAsync(int ano)
        {
            var lista = new List<DistribuicaoEstadoCivil>();

            try
            {
                using var conexao = await _connectionFactory.OpenConnectionAsync();

                var query = @"
                    WITH base_colaboradores AS (
                        SELECT * FROM rhdataset 
                        WHERE STR_TO_DATE(`Admissão`, '%d/%m/%Y') <= STR_TO_DATE(CONCAT(@ano, '-12-31'), '%Y-%m-%d')
                        AND (`Data Afastamento` = '00/00/0000'
                             OR STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y') >= STR_TO_DATE(CONCAT(@ano, '-01-01'), '%Y-%m-%d'))
                    )
                    SELECT
                        COALESCE(`Descrição (Estado Civil)`, 'Não Informado') AS Descricao,
                        COUNT(*) AS quantidade,
                        ROUND(100 * COUNT(*) / (SELECT COUNT(*) FROM base_colaboradores), 1) AS percentual
                    FROM base_colaboradores 
                    GROUP BY `Descrição (Estado Civil)`
                    ORDER BY percentual DESC;";

                using var comando = new MySqlCommand(query, conexao);
                comando.Parameters.AddWithValue("@ano", ano);

                using var reader = await comando.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    lista.Add(new DistribuicaoEstadoCivil
                    {
                        Descricao = reader.GetString("Descricao"),
                        Quantidade = reader.GetInt32("quantidade"),
                        Percentual = reader.GetDecimal("percentual")
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DiversidadeService] Erro ao obter distribuição Estado Civil: {ex.Message}");
                throw;
            }

            return lista;
        }
    }
}