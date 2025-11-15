using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector; // <-- MySqlConnector types
using MauiApp1.Services; // <-- onde IMySqlConnectionFactory está definido
using Heicomp_2025_2.Models;   // <-- onde estão seus modelos (ColaboradorResumoModel, SetorModel, etc)

namespace Heicomp_2025_2.Services
{
    public class ColaboradoresService
    {
        private readonly IMySqlConnectionFactory _connectionFactory;

        public ColaboradoresService(IMySqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        // 🔹 Abre conexão sempre no banco "RHSenior"
        private async Task<MySqlConnection> AbrirConexaoAsync()
        {
            return await _connectionFactory.OpenConnectionAsync("RHSenior");
        }

        // 1. Unidades
        public async Task<List<string>> GetUnidadesAsync()
        {
            var unidades = new List<string>();

            await using var conn = await AbrirConexaoAsync();
            const string query = "SELECT DISTINCT Filial AS unidades FROM rhdataset;";

            await using (var cmd = new MySqlCommand(query, conn))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    unidades.Add(reader.GetString(0));
                }
            }

            unidades.Insert(0, "TODAS");
            return unidades;
        }

        // 2. Anos
        public async Task<List<int>> GetAnosAsync()
        {
            var anos = new List<int>();

            await using var conn = await AbrirConexaoAsync();
            const string query = "SELECT DISTINCT YEAR(STR_TO_DATE(`Admissão`, '%d/%m/%Y')) AS anos FROM rhdataset ORDER BY anos DESC;";

            await using (var cmd = new MySqlCommand(query, conn))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    anos.Add(reader.GetInt32(0));
                }
            }

            return anos;
        }

        // 3. Total colaboradores
        public async Task<int> GetTotalColaboradoresAsync(string unidade, int ano)
        {
            int total = 0;
            await using var conn = await AbrirConexaoAsync();

            var query = @"
                WITH base_colaboradores AS (
                    SELECT *
                    FROM rhdataset
                    WHERE
                        STR_TO_DATE(`Admissão`, '%d/%m/%Y') <= STR_TO_DATE(CONCAT(@ano, '-12-31'), '%Y-%m-%d')
                        AND (
                            `Data Afastamento` = '00/00/0000'
                            OR STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y') >= STR_TO_DATE(CONCAT(@ano, '-01-01'), '%Y-%m-%d')
                        )
                        AND (
                            @unidade = 'TODAS'
                            OR `Filial` = @unidade
                        )
                )
                SELECT COUNT(*) AS total_colaboradores FROM base_colaboradores;
            ";

            await using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ano", ano);
                cmd.Parameters.AddWithValue("@unidade", unidade);
                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                    total = Convert.ToInt32(result);
            }

            return total;
        }

        // 4. Distribuição por gênero
        public async Task<List<(string Sexo, int Quantidade, double Percentual)>> GetDistribuicaoGeneroAsync(string unidade, int ano)
        {
            var distribuicao = new List<(string Sexo, int Quantidade, double Percentual)>();
            await using var conn = await AbrirConexaoAsync();

            var query = @"
                WITH base_colaboradores AS (
                    SELECT *
                    FROM rhdataset
                    WHERE
                        STR_TO_DATE(`Admissão`, '%d/%m/%Y') <= STR_TO_DATE(CONCAT(@ano, '-12-31'), '%Y-%m-%d')
                        AND (
                            `Data Afastamento` = '00/00/0000'
                            OR STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y') >= STR_TO_DATE(CONCAT(@ano, '-01-01'), '%Y-%m-%d')
                        )
                        AND (
                            @unidade = 'TODAS'
                            OR `Filial` = @unidade
                        )
                )
                SELECT
                    `Sexo`,
                    COUNT(*) AS quantidade,
                    ROUND(100 * COUNT(*) / (SELECT COUNT(*) FROM base_colaboradores), 1) AS percentual
                FROM base_colaboradores
                GROUP BY `Sexo`;
            ";

            await using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ano", ano);
                cmd.Parameters.AddWithValue("@unidade", unidade);

                await using var reader = await cmd.ExecuteReaderAsync();
                int idxSexo = reader.GetOrdinal("Sexo");
                int idxQuantidade = reader.GetOrdinal("quantidade");
                int idxPercentual = reader.GetOrdinal("percentual");

                while (await reader.ReadAsync())
                {
                    string sexo = reader.IsDBNull(idxSexo) ? string.Empty : reader.GetString(idxSexo);
                    int quantidade = reader.IsDBNull(idxQuantidade) ? 0 : reader.GetInt32(idxQuantidade);
                    double percentual = 0;
                    if (!reader.IsDBNull(idxPercentual))
                        percentual = Convert.ToDouble(reader.GetValue(idxPercentual));

                    distribuicao.Add((sexo, quantidade, percentual));
                }
            }

            return distribuicao;
        }

        // 5. Status
        public async Task<(int Ativos, int EmLicenca, int Estagiarios, int Pcd)> GetStatusColaboradoresAsync(string unidade, int ano)
        {
            await using var conn = await AbrirConexaoAsync();

            var baseQuery = @"
                WITH base_colaboradores AS (
                    SELECT *
                    FROM rhdataset
                    WHERE
                        STR_TO_DATE(`Admissão`, '%d/%m/%Y') <= STR_TO_DATE(CONCAT(@ano, '-12-31'), '%Y-%m-%d')
                        AND (
                            `Data Afastamento` = '00/00/0000'
                            OR STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y') >= STR_TO_DATE(CONCAT(@ano, '-01-01'), '%Y-%m-%d')
                        )
                        AND (
                            @unidade = 'TODAS'
                            OR `Filial` = @unidade
                        )
                )
            ";

            async Task<int> ExecCount(string q)
            {
                await using var cmd = new MySqlCommand(baseQuery + q, conn);
                cmd.Parameters.AddWithValue("@ano", ano);
                cmd.Parameters.AddWithValue("@unidade", unidade);
                var r = await cmd.ExecuteScalarAsync();
                return (r != null && r != DBNull.Value) ? Convert.ToInt32(r) : 0;
            }

            int ativos = await ExecCount("SELECT COUNT(*) FROM base_colaboradores;");
            int emLicenca = await ExecCount(@"
                SELECT COUNT(*) FROM base_colaboradores
                WHERE YEAR(STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y')) = @ano
                AND `Descrição (Situação)` IN (
                    'Lic. s/ Remuneração','Ferias','Lic.Medica - 15 Dias',
                    'Lic.Medica - 30 Dias Prof.','Licença Maternidade',
                    'Licença Paternidade','Licença Paternidade Prof.','Auxilio Doenca'
                );
            ");
            int estagiarios = await ExecCount(@"
                SELECT COUNT(*) FROM base_colaboradores
                WHERE (`Título Reduzido (Cargo)` LIKE '%estag%' OR `Descrição (Instrução)` LIKE '%estag%');
            ");
            int pcd = await ExecCount(@"
                SELECT COUNT(*) FROM base_colaboradores
                WHERE `Descrição (Deficiência)` IS NOT NULL
                AND `Descrição (Deficiência)` <> ''
                AND `Descrição (Deficiência)` <> 'Nenhuma';
            ");

            return (ativos, emLicenca, estagiarios, pcd);
        }

        // 6. Colaboradores por setor
        public async Task<List<(string Setor, int Quantidade)>> GetColaboradoresPorSetorAsync(string unidade, int ano, bool top5 = true)
        {
            var setores = new List<(string Setor, int Quantidade)>();
            await using var conn = await AbrirConexaoAsync();

            var query = @"
                WITH base_colaboradores AS (
                    SELECT *
                    FROM rhdataset
                    WHERE
                        STR_TO_DATE(`Admissão`, '%d/%m/%Y') <= STR_TO_DATE(CONCAT(@ano, '-12-31'), '%Y-%m-%d')
                        AND (
                            `Data Afastamento` = '00/00/0000'
                            OR STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y') >= STR_TO_DATE(CONCAT(@ano, '-01-01'), '%Y-%m-%d')
                        )
                        AND (
                            @unidade = 'TODAS'
                            OR `Filial` = @unidade
                        )
                )
                SELECT 
                    `Descrição (C.Custo)` AS setor,
                    COUNT(*) AS quantidade
                FROM base_colaboradores
                GROUP BY `Descrição (C.Custo)`
                ORDER BY quantidade DESC
            ";

            if (top5) query += " LIMIT 5;";

            await using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ano", ano);
                cmd.Parameters.AddWithValue("@unidade", unidade);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string setor = reader.IsDBNull(0) ? "Indefinido" : reader.GetString(0);
                    int qtd = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    setores.Add((setor, qtd));
                }
            }

            return setores;
        }

        // 7. Lista de colaboradores (top5 ou todos)
        public async Task<List<(string Nome, string Setor, string Cargo, string Status)>> GetListaColaboradoresAsync(string unidade, int ano, bool top5 = true)
        {
            var colaboradores = new List<(string Nome, string Setor, string Cargo, string Status)>();
            await using var conn = await AbrirConexaoAsync();

            var query = @"
                WITH base_colaboradores AS (
                    SELECT *
                    FROM rhdataset
                    WHERE
                        STR_TO_DATE(`Admissão`, '%d/%m/%Y') <= STR_TO_DATE(CONCAT(@ano, '-12-31'), '%Y-%m-%d')
                        AND (
                            `Data Afastamento` = '00/00/0000'
                            OR STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y') >= STR_TO_DATE(CONCAT(@ano, '-01-01'), '%Y-%m-%d')
                        )
                        AND (
                            @unidade = 'TODAS'
                            OR `Filial` = @unidade
                        )
                )
                SELECT
                    `Nome`,
                    `Descrição (C.Custo)` AS setor,
                    `Título Reduzido (Cargo)` AS cargo,
                    CASE
                        WHEN `Descrição (Situação)` = 'Demitido'
                             AND YEAR(STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y')) = @ano
                            THEN 'Demitido'
                        WHEN `Descrição (Situação)` IN (
                            'Lic. s/ Remuneração','Ferias','Lic.Medica - 15 Dias',
                            'Lic.Medica - 30 Dias Prof.','Licença Maternidade',
                            'Licença Paternidade','Licença Paternidade Prof.','Auxilio Doenca'
                        )
                        AND YEAR(STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y')) = @ano
                            THEN 'Com licença'
                        ELSE 'Ativo'
                    END AS status
                FROM base_colaboradores
                ORDER BY `Nome`
            ";

            if (top5) query += " LIMIT 5;";

            await using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ano", ano);
                cmd.Parameters.AddWithValue("@unidade", unidade);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string nome = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    string setor = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    string cargo = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    string status = reader.IsDBNull(3) ? "" : reader.GetString(3);

                    colaboradores.Add((nome, setor, cargo, status));
                }
            }

            return colaboradores;
        }

        // 8. Lista paginada + filtro
        public async Task<List<(string Nome, string Setor, string Cargo, string Status)>> GetListaColaboradoresPaginadoAsync(
            string unidade, int ano, int offset, int limit, string filtro)
        {
            var todos = await GetListaColaboradoresAsync(unidade, ano, false);

            IEnumerable<(string Nome, string Setor, string Cargo, string Status)> query = todos;

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                string f = filtro.ToLowerInvariant();
                query = query.Where(c =>
                    (!string.IsNullOrEmpty(c.Nome) && c.Nome.ToLowerInvariant().Contains(f)) ||
                    (!string.IsNullOrEmpty(c.Setor) && c.Setor.ToLowerInvariant().Contains(f)) ||
                    (!string.IsNullOrEmpty(c.Cargo) && c.Cargo.ToLowerInvariant().Contains(f)) ||
                    (!string.IsNullOrEmpty(c.Status) && c.Status.ToLowerInvariant().Contains(f))
                );
            }

            return query.Skip(offset).Take(limit).ToList();
        }

        // 9. Teste de conexão
        public async Task<MySqlConnection> TestarConexaoAsync()
        {
            try
            {
                var conn = await _connectionFactory.OpenConnectionAsync("RHSenior");
                return conn;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro de conexão: {ex.Message}");
                return null;
            }
        }
    }
}
