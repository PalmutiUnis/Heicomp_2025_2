using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MySqlConnector;

namespace MauiApp1.Services
{
    public class GraficosDetalhadosServices
    {
        private readonly IMySqlConnectionFactory _connectionFactory;

        public GraficosDetalhadosServices(IMySqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        private async Task<MySqlConnection> AbrirConexaoAsync()
        {
            return await _connectionFactory.OpenConnectionAsync("RHSenior");
        }

        public async Task<int> GetTotalHomensAsync(string unidade)
        {
            int total = 0;
            await using var conn = await AbrirConexaoAsync();

            string query = @"
                select count(rh.index) 
                from rhdataset rh
                where rh.Sexo = 'M'
                AND (@unidade = 'TODAS' OR rh.Filial = @unidade);
            ";

            await using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@unidade", unidade ?? "TODAS");
                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                    total = Convert.ToInt32(result);
            }
            return total;
        }

        public async Task<int> GetTotalMulheresAsync(string unidade)
        {
            int total = 0;
            await using var conn = await AbrirConexaoAsync();

            string query = @"
                select count(rh.index)
                from rhdataset rh
                where rh.Sexo = 'F'
                AND (@unidade = 'TODAS' OR rh.Filial = @unidade);
            ";

            await using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@unidade", unidade ?? "TODAS");
                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                    total = Convert.ToInt32(result);
            }
            return total;
        }

        // --- NOVO MÉTODO PARA A LISTA LATERAL (CAIXA VERDE) ---
        public async Task<List<SituacaoDto>> GetResumoSituacoesAsync(string unidade)
        {
            var lista = new List<SituacaoDto>();
            await using var conn = await AbrirConexaoAsync();

            // SQL Mágico: Agrupa por situação e conta
            // Atenção: Verifique se o nome da coluna no seu banco é 'Situacao' mesmo.
            string query = @"
                SELECT 
                    rh.`Descrição (Situação)` as NomeSituacao, 
                    COUNT(rh.index) as Quantidade
                FROM rhdataset rh
                WHERE (@unidade = 'TODAS' OR rh.Filial = @unidade)
                GROUP BY rh.`Descrição (Situação)`
                ORDER BY Quantidade DESC;
            ";

            await using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@unidade", unidade ?? "TODAS");

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new SituacaoDto
                        {
                            Nome = reader["NomeSituacao"]?.ToString() ?? "Indefinido",
                            Quantidade = Convert.ToInt32(reader["Quantidade"])
                        });
                    }
                }
            }
            return lista;
        }
    }

    // Classe simples para transportar os dados
    public class SituacaoDto
    {
        public string Nome { get; set; }
        public int Quantidade { get; set; }
    }
}