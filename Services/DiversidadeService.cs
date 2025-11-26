using MySqlConnector;
using MauiApp1.Models;
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

        public async Task DebugListarValoresSexoAsync()
        {
            try
            {
                using var conexao = await _connectionFactory.OpenConnectionAsync();
                var query = "SELECT `Sexo` as valor, COUNT(*) as qtd FROM rhdataset GROUP BY `Sexo` ORDER BY qtd DESC;";
                using var comando = new MySqlCommand(query, conexao);
                using var reader = await comando.ExecuteReaderAsync();

                Debug.WriteLine("========== VALORES NO BANCO ==========");
                while (await reader.ReadAsync())
                {
                    var valor = reader.IsDBNull(0) ? "NULL" : reader.GetString("valor");
                    Debug.WriteLine($"[DEBUG] Sexo: '{valor}' -> {reader.GetInt32("qtd")}");
                }
                Debug.WriteLine("======================================");
            }
            catch (Exception ex) { Debug.WriteLine($"[DEBUG] Erro: {ex.Message}"); }
        }

        /// <summary>
        /// Obtém dados gerais (Cards e Gráfico de Rosca) com lógica flexível para M/F
        /// </summary>
        public async Task<DiversidadeGeral> ObterDadosGeraisAsync(int ano)
        {
            try
            {
                using var conexao = await _connectionFactory.OpenConnectionAsync();

                // QUERY CORRIGIDA E ROBUSTA:
                var query = @"
                    WITH base_colaboradores AS (
                        SELECT * FROM rhdataset 
                        WHERE STR_TO_DATE(`Admissão`, '%d/%m/%Y') <= STR_TO_DATE(CONCAT(@ano, '-12-31'), '%Y-%m-%d')
                        AND (`Data Afastamento` = '00/00/0000'
                             OR STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y') >= STR_TO_DATE(CONCAT(@ano, '-01-01'), '%Y-%m-%d'))
                    )
                    SELECT
                        COUNT(*) AS total,
                        
                        -- Aceita 'M', 'Masculino', 'Homem', etc.
                        ROUND(100 * SUM(CASE 
                            WHEN TRIM(Sexo) LIKE 'M%' OR TRIM(Sexo) = 'Homem' THEN 1 
                            ELSE 0 
                        END) / NULLIF(COUNT(*), 0), 1) AS percentual_homens,

                        -- Aceita 'F', 'Feminino', 'Mulher', etc.
                        ROUND(100 * SUM(CASE 
                            WHEN TRIM(Sexo) LIKE 'F%' OR TRIM(Sexo) = 'Mulher' THEN 1 
                            ELSE 0 
                        END) / NULLIF(COUNT(*), 0), 1) AS percentual_mulheres

                    FROM base_colaboradores;";

                using var comando = new MySqlCommand(query, conexao);
                comando.Parameters.AddWithValue("@ano", ano);

                using var reader = await comando.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var total = reader.IsDBNull(reader.GetOrdinal("total")) ? 0 : reader.GetInt32("total");

                    if (total == 0) return new DiversidadeGeral();

                    var pHome = reader.IsDBNull(reader.GetOrdinal("percentual_homens")) ? 0m : reader.GetDecimal("percentual_homens");
                    var pMulher = reader.IsDBNull(reader.GetOrdinal("percentual_mulheres")) ? 0m : reader.GetDecimal("percentual_mulheres");

                    // O restante é Não Informado
                    var pNaoInf = 100m - pHome - pMulher;
                    if (pNaoInf < 0) pNaoInf = 0;

                    return new DiversidadeGeral
                    {
                        TotalColaboradores = total,
                        PercentualHomens = pHome,
                        PercentualMulheres = pMulher,
                        PercentualNaoInformado = pNaoInf
                    };
                }

                return new DiversidadeGeral();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DiversidadeService] Erro: {ex.Message}");
                return new DiversidadeGeral(); // Retorna vazio em vez de quebrar
            }
        }

        public async Task<List<DistribuicaoGenero>> ObterDistribuicaoGeneroAsync(int ano)
        {
            var lista = new List<DistribuicaoGenero>();
            try
            {
                using var conexao = await _connectionFactory.OpenConnectionAsync();

                // QUERY NORMALIZADA: Agrupa variações de nome
                var query = @"
                    WITH base_colaboradores AS (
                        SELECT * FROM rhdataset 
                        WHERE STR_TO_DATE(`Admissão`, '%d/%m/%Y') <= STR_TO_DATE(CONCAT(@ano, '-12-31'), '%Y-%m-%d')
                        AND (`Data Afastamento` = '00/00/0000'
                             OR STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y') >= STR_TO_DATE(CONCAT(@ano, '-01-01'), '%Y-%m-%d'))
                    )
                    SELECT
                        CASE 
                            WHEN TRIM(Sexo) LIKE 'M%' THEN 'Masculino'
                            WHEN TRIM(Sexo) LIKE 'F%' THEN 'Feminino'
                            ELSE 'Não Informado'
                        END AS SexoNormalizado,
                        COUNT(*) AS quantidade,
                        ROUND(100 * COUNT(*) / (SELECT COUNT(*) FROM base_colaboradores), 1) AS percentual
                    FROM base_colaboradores 
                    GROUP BY SexoNormalizado
                    ORDER BY quantidade DESC;";

                using var comando = new MySqlCommand(query, conexao);
                comando.Parameters.AddWithValue("@ano", ano);

                using var reader = await comando.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    lista.Add(new DistribuicaoGenero
                    {
                        Sexo = reader.GetString("SexoNormalizado"),
                        Quantidade = reader.GetInt32("quantidade"),
                        Percentual = reader.GetDecimal("percentual")
                    });
                }
            }
            catch (Exception ex) { Debug.WriteLine($"Erro: {ex.Message}"); }
            return lista;
        }

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
                        AND (`Data Afastamento` = '00/00/0000' OR STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y') >= STR_TO_DATE(CONCAT(@ano, '-01-01'), '%Y-%m-%d'))
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
            catch (Exception ex) { Debug.WriteLine($"Erro: {ex.Message}"); }
            return lista;
        }

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
                        AND (`Data Afastamento` = '00/00/0000' OR STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y') >= STR_TO_DATE(CONCAT(@ano, '-01-01'), '%Y-%m-%d'))
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
            catch (Exception ex) { Debug.WriteLine($"Erro: {ex.Message}"); }
            return lista;
        }

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
                        AND (`Data Afastamento` = '00/00/0000' OR STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y') >= STR_TO_DATE(CONCAT(@ano, '-01-01'), '%Y-%m-%d'))
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
            catch (Exception ex) { Debug.WriteLine($"Erro: {ex.Message}"); }
            return lista;
        }
    }
}