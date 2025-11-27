using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MauiApp1.Models.Administrativa;
using MySqlConnector;

namespace MauiApp1.Services.Admin
{
    public class AdminService
    {
        private readonly IMySqlConnectionFactory _connectionFactory;
        private ObservableCollection<AdminModel> _usuariosCache;

        // Tabela e colunas (exatas com acento/espacos) — use crases nas queries
        private const string TableName = "rhdataset";
        private const string ColId = "index";
        private const string ColNome = "nome";
        private const string ColCargo = "Título Reduzido (Cargo)";
        private const string ColDescricaoSituacao = "Descrição (Situação)";
        private const string ColUnidadeGrupo = "Descrição (C.Custo)";

        public AdminService(IMySqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _usuariosCache = new ObservableCollection<AdminModel>();
        }

        public async Task<bool> TestarConexaoAsync(CancellationToken ct = default)
        {
            try
            {
                using var conn = await _connectionFactory.OpenConnectionAsync("RHSenior", ct);
                // Se abriu sem exceção, considera-se OK
                System.Diagnostics.Debug.WriteLine("✅ Conexão com MySQL estabelecida!");
                await conn.CloseAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao conectar: {ex.Message}");
                return false;
            }
        }

        public async Task<ObservableCollection<AdminModel>> ObterTodosUsuariosAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Buscando TODOS os usuários da tabela rhdataset...");

                var sql = $@"
SELECT
  `{ColId}` AS Id,
  `{ColNome}` AS Nome,
  `{ColCargo}` AS Cargo,
  `{ColDescricaoSituacao}` AS DescricaoSituacao,
  `{ColUnidadeGrupo}` AS UnidadeGrupo
FROM `{TableName}`
ORDER BY `{ColNome}`;";

                var usuarios = await ExecuteQueryAsync(sql, reader => MapReaderToModel(reader));

                System.Diagnostics.Debug.WriteLine($"✅ {usuarios.Count} usuários encontrados");

                _usuariosCache.Clear();
                foreach (var usuario in usuarios)
                {
                    _usuariosCache.Add(usuario);
                }

                return _usuariosCache;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                return _usuariosCache;
            }
        }

        /// <summary>
        /// Obter apenas usuários TRABALHANDO
        /// </summary>
        public async Task<ObservableCollection<AdminModel>> ObterUsuariosTrabalhando()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Buscando usuários TRABALHANDO...");

                var sql = $@"
SELECT
  `{ColId}` AS Id,
  `{ColNome}` AS Nome,
  `{ColCargo}` AS Cargo,
  `{ColDescricaoSituacao}` AS DescricaoSituacao,
  `{ColUnidadeGrupo}` AS UnidadeGrupo
FROM `{TableName}`
WHERE `{ColDescricaoSituacao}` = @sit
ORDER BY `{ColNome}`;";

                var usuarios = await ExecuteQueryAsync(sql, reader => MapReaderToModel(reader),
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@sit", "Trabalhando");
                    });

                System.Diagnostics.Debug.WriteLine($"✅ {usuarios.Count} usuários trabalhando");

                return CriarCollectionComFotos(usuarios);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro: {ex.Message}");
                return new ObservableCollection<AdminModel>();
            }
        }

        /// <summary>
        /// Obter apenas usuários DEMITIDOS
        /// </summary>
        public async Task<ObservableCollection<AdminModel>> ObterUsuariosDemitidos()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Buscando usuários DEMITIDOS...");

                var sql = $@"
SELECT
  `{ColId}` AS Id,
  `{ColNome}` AS Nome,
  `{ColCargo}` AS Cargo,
  `{ColDescricaoSituacao}` AS DescricaoSituacao,
  `{ColUnidadeGrupo}` AS UnidadeGrupo
FROM `{TableName}`
WHERE `{ColDescricaoSituacao}` = @sit
ORDER BY `{ColNome}`;";

                var usuarios = await ExecuteQueryAsync(sql, reader => MapReaderToModel(reader),
                    cmd => cmd.Parameters.AddWithValue("@sit", "Demitido"));

                System.Diagnostics.Debug.WriteLine($"✅ {usuarios.Count} usuários demitidos");

                return CriarCollectionComFotos(usuarios);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro: {ex.Message}");
                return new ObservableCollection<AdminModel>();
            }
        }

        /// <summary>
        /// Obter apenas usuários em APOSENTADORIA POR INVALIDEZ
        /// </summary>
        public async Task<ObservableCollection<AdminModel>> ObterUsuariosAposentadosPorInvalidez()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Buscando usuários APOSENTADOS POR INVALIDEZ...");

                var sql = $@"
SELECT
  `{ColId}` AS Id,
  `{ColNome}` AS Nome,
  `{ColCargo}` AS Cargo,
  `{ColDescricaoSituacao}` AS DescricaoSituacao,
  `{ColUnidadeGrupo}` AS UnidadeGrupo
FROM `{TableName}`
WHERE `{ColDescricaoSituacao}` = @sit
ORDER BY `{ColNome}`;";

                var usuarios = await ExecuteQueryAsync(sql, reader => MapReaderToModel(reader),
                    cmd => cmd.Parameters.AddWithValue("@sit", "Aposentadoria por Invalidez"));

                System.Diagnostics.Debug.WriteLine($"✅ {usuarios.Count} usuários aposentados por invalidez");

                return CriarCollectionComFotos(usuarios);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro: {ex.Message}");
                return new ObservableCollection<AdminModel>();
            }
        }

        /// <summary>
        /// Obter apenas usuários em AUXÍLIO DOENÇA
        /// </summary>
        public async Task<ObservableCollection<AdminModel>> ObterUsuariosAuxilioDoenca()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Buscando usuários em AUXÍLIO DOENÇA...");

                var sql = $@"
SELECT
  `{ColId}` AS Id,
  `{ColNome}` AS Nome,
  `{ColCargo}` AS Cargo,
  `{ColDescricaoSituacao}` AS DescricaoSituacao,
  `{ColUnidadeGrupo}` AS UnidadeGrupo
FROM `{TableName}`
WHERE `{ColDescricaoSituacao}` IN (@sit1, @sit2)
ORDER BY `{ColNome}`;";

                var usuarios = await ExecuteQueryAsync(sql, reader => MapReaderToModel(reader),
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@sit1", "Auxilio Doença");
                        cmd.Parameters.AddWithValue("@sit2", "Auxílio Doença");
                    });

                System.Diagnostics.Debug.WriteLine($"✅ {usuarios.Count} usuários em auxílio doença");

                return CriarCollectionComFotos(usuarios);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro: {ex.Message}");
                return new ObservableCollection<AdminModel>();
            }
        }

        /// <summary>
        /// Filtrar usuários por situação
        /// </summary>
        public async Task<ObservableCollection<AdminModel>> FiltrarPorSituacaoAsync(string situacao)
        {
            System.Diagnostics.Debug.WriteLine($"🔍 Filtrando por situação: {situacao}");

            switch (situacao?.ToUpper())
            {
                case "TRABALHANDO":
                    return await ObterUsuariosTrabalhando();

                case "DEMITIDOS":
                case "DEMITIDO":
                    return await ObterUsuariosDemitidos();

                case "APOSENTADORIA POR INVALIDEZ":
                case "APOSENTADOS":
                    return await ObterUsuariosAposentadosPorInvalidez();

                case "AUXÍLIO DOENÇA":
                case "AUXILIO DOENÇA":
                case "AUXILIO DOENCA":
                    return await ObterUsuariosAuxilioDoenca();

                case "TODOS":
                default:
                    return await ObterTodosUsuariosAsync();
            }
        }

        // ==================== OUTROS FILTROS ====================

        public ObservableCollection<AdminModel> ObterUsuariosPorUnidade(string unidadeGrupo)
        {
            if (string.IsNullOrEmpty(unidadeGrupo) || unidadeGrupo == "Todas as Unidades")
            {
                return _usuariosCache;
            }

            var filtrados = _usuariosCache
                .Where(u => u.UnidadeGrupo == unidadeGrupo)
                .ToList();

            return new ObservableCollection<AdminModel>(filtrados);
        }

        public async Task<ObservableCollection<AdminModel>> ObterUsuariosPorCargoAsync(string cargo)
        {
            try
            {
                var sql = $@"
SELECT
  `{ColId}` AS Id,
  `{ColNome}` AS Nome,
  `{ColCargo}` AS Cargo,
  `{ColDescricaoSituacao}` AS DescricaoSituacao,
  `{ColUnidadeGrupo}` AS UnidadeGrupo
FROM `{TableName}`
WHERE `{ColCargo}` = @cargo
ORDER BY `{ColNome}`;";

                var usuarios = await ExecuteQueryAsync(sql, reader => MapReaderToModel(reader),
                    cmd => cmd.Parameters.AddWithValue("@cargo", cargo));

                return CriarCollectionComFotos(usuarios);
            }
            catch
            {
                return new ObservableCollection<AdminModel>();
            }
        }

        // ==================== ESTATÍSTICAS ====================

        public async Task<Dictionary<string, int>> ObterEstatisticasPorSituacaoAsync()
        {
            try
            {
                var stats = new Dictionary<string, int>();

                // Total geral
                var total = Convert.ToInt32(await ExecuteScalarAsync($"SELECT COUNT(*) FROM `{TableName}`;"));
                stats["Total"] = total;

                // Por situação específica
                var trabalhando = Convert.ToInt32(await ExecuteScalarAsync(
                    $"SELECT COUNT(*) FROM `{TableName}` WHERE `{ColDescricaoSituacao}` = @sit;",
                    cmd => cmd.Parameters.AddWithValue("@sit", "Trabalhando")));
                stats["Trabalhando"] = trabalhando;

                var demitidos = Convert.ToInt32(await ExecuteScalarAsync(
                    $"SELECT COUNT(*) FROM `{TableName}` WHERE `{ColDescricaoSituacao}` = @sit;",
                    cmd => cmd.Parameters.AddWithValue("@sit", "Demitido")));
                stats["Demitidos"] = demitidos;

                var aposentados = Convert.ToInt32(await ExecuteScalarAsync(
                    $"SELECT COUNT(*) FROM `{TableName}` WHERE `{ColDescricaoSituacao}` = @sit;",
                    cmd => cmd.Parameters.AddWithValue("@sit", "Aposentadoria por Invalidez")));
                stats["Aposentadoria por Invalidez"] = aposentados;

                var auxilioDoenca = Convert.ToInt32(await ExecuteScalarAsync(
                    $"SELECT COUNT(*) FROM `{TableName}` WHERE `{ColDescricaoSituacao}` IN (@sit1, @sit2);",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@sit1", "Auxilio Doença");
                        cmd.Parameters.AddWithValue("@sit2", "Auxílio Doença");
                    }));
                stats["Auxílio Doença"] = auxilioDoenca;

                System.Diagnostics.Debug.WriteLine("📊 ESTATÍSTICAS:");
                foreach (var stat in stats)
                {
                    System.Diagnostics.Debug.WriteLine($"  {stat.Key}: {stat.Value}");
                }

                return stats;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao obter estatísticas: {ex.Message}");
                return new Dictionary<string, int>();
            }
        }

        public async Task<List<string>> ObterListaCargosAsync()
        {
            try
            {
                var sql = $@"SELECT DISTINCT `{ColCargo}` FROM `{TableName}` WHERE `{ColCargo}` IS NOT NULL AND `{ColCargo}` <> '' ORDER BY `{ColCargo}`;";
                var cargos = await ExecuteQueryAsync(sql, reader => reader.IsDBNull(0) ? string.Empty : reader.GetString(0));
                return cargos.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().OrderBy(s => s).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<List<string>> ObterListaUnidadesAsync()
        {
            try
            {
                var sql = $@"SELECT DISTINCT `{ColUnidadeGrupo}` FROM `{TableName}` WHERE `{ColUnidadeGrupo}` IS NOT NULL AND `{ColUnidadeGrupo}` <> '' ORDER BY `{ColUnidadeGrupo}`;";
                var unidades = await ExecuteQueryAsync(sql, reader => reader.IsDBNull(0) ? string.Empty : reader.GetString(0));

                var lista = unidades.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().OrderBy(s => s).ToList();
                lista.Insert(0, "Todas as Unidades");
                return lista;
            }
            catch
            {
                return new List<string> { "Todas as Unidades" };
            }
        }

        // ==================== MÉTODOS AUXILIARES ====================

        public ObservableCollection<AdminModel> ObterTodosUsuarios()
        {
            return _usuariosCache;
        }

        public AdminModel? ObterUsuarioPorId(int id)
        {
            return _usuariosCache.FirstOrDefault(u => u.Id == id);
        }

        public List<string> ObterSituacoesDisponiveis()
        {
            return new List<string>
            {
                "Todos",
                "Trabalhando",
                "Demitidos",
                "Aposentadoria por Invalidez",
                "Auxílio Doença"
            };
        }

        public List<string> ObterUnidadesGrupos()
        {
            var unidades = _usuariosCache
                .Where(u => !string.IsNullOrEmpty(u.UnidadeGrupo))
                .Select(u => u.UnidadeGrupo!)
                .Distinct()
                .OrderBy(u => u)
                .ToList();

            unidades.Insert(0, "Todas as Unidades");

            if (unidades.Count == 1)
            {
                return new List<string> { "Todas as Unidades" };
            }

            return unidades;
        }

        private ObservableCollection<AdminModel> CriarCollectionComFotos(List<AdminModel> usuarios)
        {
            var result = new ObservableCollection<AdminModel>();
            foreach (var usuario in usuarios)
            {
                result.Add(usuario);
            }
            return result;
        }

        public async Task<bool> AdicionarUsuario(AdminModel usuario)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"➕ Adicionando usuário: {usuario.Nome}");

                var sql = $@"
INSERT INTO `{TableName}` (`{ColNome}`, `{ColCargo}`, `{ColDescricaoSituacao}`, `{ColUnidadeGrupo}`)
VALUES (@nome, @cargo, @situacao, @unidade);";

                await ExecuteNonQueryAsync(sql, cmd =>
                {
                    cmd.Parameters.AddWithValue("@nome", usuario.Nome ?? string.Empty);
                    cmd.Parameters.AddWithValue("@cargo", usuario.Cargo ?? string.Empty);
                    cmd.Parameters.AddWithValue("@situacao", usuario.DescricaoSituacao ?? string.Empty);
                    cmd.Parameters.AddWithValue("@unidade", usuario.UnidadeGrupo ?? string.Empty);
                });

                System.Diagnostics.Debug.WriteLine($"✅ Usuário {usuario.Nome} adicionado com sucesso!");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao adicionar usuário: {ex.Message}");
                return false;
            }
        }

        public List<string> ObterCargosDisponiveis()
        {
            var cargos = _usuariosCache
                .Select(u => u.Cargo)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            if (cargos.Count == 0)
            {
                // Lista padrão de cargos caso o cache esteja vazio
                return new List<string>
                {
                    "Analista",
                    "Assistente",
                    "Coordenador",
                    "Desenvolvedor",
                    "Diretor",
                    "Gerente",
                    "Supervisor",
                    "Técnico"
                };
            }

            return cargos;
        }

        // ==================== DIAGNÓSTICO ====================

        public async Task<string> DiagnosticarBancoDadosAsync()
        {
            try
            {
                var resultado = new StringBuilder();

                bool conectado = await TestarConexaoAsync();
                resultado.AppendLine($"Conexão: {(conectado ? "✅ OK" : "❌ FALHOU")}");
                resultado.AppendLine($"Banco: rhsenior_heicomp");
                resultado.AppendLine($"Tabela: rhdataset");
                resultado.AppendLine($"Coluna Situação: Descrição (Situação)");
                resultado.AppendLine();

                var total = Convert.ToInt32(await ExecuteScalarAsync($"SELECT COUNT(*) FROM `{TableName}`;"));
                resultado.AppendLine($"Total de registros: {total}");
                resultado.AppendLine();

                var stats = await ObterEstatisticasPorSituacaoAsync();
                resultado.AppendLine("Estatísticas por Situação:");
                foreach (var stat in stats)
                {
                    resultado.AppendLine($"  • {stat.Key}: {stat.Value}");
                }
                resultado.AppendLine();

                var primeiros = await ExecuteQueryAsync($@"SELECT `{ColId}` AS Id, `{ColNome}` AS Nome, `{ColDescricaoSituacao}` AS DescricaoSituacao FROM `{TableName}` LIMIT 3;",
                    reader => new AdminModel
                    {
                        Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        Nome = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        DescricaoSituacao = reader.IsDBNull(2) ? null : reader.GetString(2)
                    });

                resultado.AppendLine($"Primeiros 3 registros:");
                foreach (var u in primeiros)
                {
                    resultado.AppendLine($"  {u.StatusEmoji} ID: {u.Id} | Nome: {u.Nome} | Situação: {u.DescricaoSituacao}");
                }

                return resultado.ToString();
            }
            catch (Exception ex)
            {
                return $"❌ Erro: {ex.Message}\n\nStack: {ex.StackTrace}";
            }
        }

        // ==================== Helpers de Acesso a Dados ====================

        private async Task<List<T>> ExecuteQueryAsync<T>(string sql, Func<MySqlDataReader, T> selector, Action<MySqlCommand>? configure = null)
        {
            var list = new List<T>();

            try
            {
                using var conn = await _connectionFactory.OpenConnectionAsync("RHSenior");
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                configure?.Invoke(cmd);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(selector(reader));
                }

                await conn.CloseAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ExecuteQueryAsync error: {ex.Message}");
            }

            return list;
        }

        private async Task<int> ExecuteNonQueryAsync(string sql, Action<MySqlCommand>? configure = null)
        {
            try
            {
                using var conn = await _connectionFactory.OpenConnectionAsync("RHSenior");
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                configure?.Invoke(cmd);

                var affected = await cmd.ExecuteNonQueryAsync();
                await conn.CloseAsync();
                return affected;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ExecuteNonQueryAsync error: {ex.Message}");
                return 0;
            }
        }

        private async Task<object?> ExecuteScalarAsync(string sql, Action<MySqlCommand>? configure = null)
        {
            try
            {
                using var conn = await _connectionFactory.OpenConnectionAsync("RHSenior");
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                configure?.Invoke(cmd);

                var result = await cmd.ExecuteScalarAsync();
                await conn.CloseAsync();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ExecuteScalarAsync error: {ex.Message}");
                return null;
            }
        }

        private AdminModel MapReaderToModel(MySqlDataReader reader)
        {
            return new AdminModel
            {
                Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                Nome = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                Cargo = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                DescricaoSituacao = reader.IsDBNull(3) ? null : reader.GetString(3),
                UnidadeGrupo = reader.IsDBNull(4) ? null : reader.GetString(4)
            };
        }
    }
}
