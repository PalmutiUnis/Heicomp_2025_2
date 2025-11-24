using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using MauiApp1.Services;

namespace Heicomp_2025_2.Services
{
    public class AlunosService
    {
        private readonly IMySqlConnectionFactory _connectionFactory;

        public AlunosService(IMySqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        private async Task<MySqlConnection> AbrirConexaoAsync()
        {
            return await _connectionFactory.OpenConnectionAsync("Corporem");
        }
        // Consulta Card Total de Alunos
        public async Task<int> GetTotalAlunosAsync(string campus = null, string modalidade = null, string curso = null, int? turno = null, string periodo = null, string turma = null)
        {
            var query = "SELECT COUNT(DISTINCT m.RA) AS TOTAL_ALUNOS FROM SMATRICPL m INNER JOIN STURMA tu ON m.CODCOLIGADA = tu.CODCOLIGADA AND m.CODTURMA = tu.CODTURMA AND m.IDPERLET = tu.IDPERLET INNER JOIN SHABILITACAOFILIAL hf ON tu.IDHABILITACAOFILIAL = hf.IDHABILITACAOFILIAL INNER JOIN SHABILITACAOFILIALCAMPUS hfc ON hf.IDHABILITACAOFILIAL = hfc.IDHABILITACAOFILIAL INNER JOIN SCURSO s ON hf.CODCURSO = s.CODCURSO AND hf.CODCOLIGADA = s.CODCOLIGADA WHERE 1=1";
            var parametros = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(campus)) { query += " AND hfc.CODCAMPUS = @Campus"; parametros.Add(new MySqlParameter("@Campus", campus)); }
            if (!string.IsNullOrEmpty(modalidade)) { if (modalidade == "Presencial (sem definição)") query += " AND (s.CURPRESDIST = 'P' OR s.CURPRESDIST IS NULL)"; else { query += " AND s.CURPRESDIST = @Modalidade"; parametros.Add(new MySqlParameter("@Modalidade", modalidade)); } }
            if (!string.IsNullOrEmpty(curso)) { query += " AND s.CODCURSO = @Curso"; parametros.Add(new MySqlParameter("@Curso", curso)); }
            if (turno.HasValue && turno.Value > 0) { query += " AND hf.CODTURNO = @Turno"; parametros.Add(new MySqlParameter("@Turno", turno.Value)); }
            if (!string.IsNullOrEmpty(periodo)) { query += " AND EXISTS (SELECT 1 FROM SPLETIVO p WHERE tu.IDPERLET = p.IDPERLET AND p.CODPERLET = @Periodo)"; parametros.Add(new MySqlParameter("@Periodo", periodo)); }
            await using var conn = await AbrirConexaoAsync();
            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddRange(parametros.ToArray());
            var result = await cmd.ExecuteScalarAsync();
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        //Consulta Card Modalidade mais Comum
        public async Task<(string Modalidade, int Quantidade)> GetModalidadeMaisAlunosAsync(string campus = null, string curso = null, int? turno = null, string periodo = null)
        {
            var query = "SELECT COALESCE(s.CURPRESDIST, 'Presencial (sem definição)') AS MODALIDADE, COUNT(DISTINCT m.RA) AS QTD_ALUNOS FROM SMATRICPL m INNER JOIN STURMA tu ON m.CODCOLIGADA = tu.CODCOLIGADA AND m.CODTURMA = tu.CODTURMA AND m.IDPERLET = tu.IDPERLET INNER JOIN SHABILITACAOFILIAL hf ON tu.IDHABILITACAOFILIAL = hf.IDHABILITACAOFILIAL INNER JOIN SHABILITACAOFILIALCAMPUS hfc ON hf.IDHABILITACAOFILIAL = hfc.IDHABILITACAOFILIAL INNER JOIN SCURSO s ON hf.CODCURSO = s.CODCURSO AND hf.CODCOLIGADA = s.CODCOLIGADA WHERE 1=1";
            var parametros = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(campus)) { query += " AND hfc.CODCAMPUS = @Campus"; parametros.Add(new MySqlParameter("@Campus", campus)); }
            if (!string.IsNullOrEmpty(curso)) { query += " AND s.CODCURSO = @Curso"; parametros.Add(new MySqlParameter("@Curso", curso)); }
            if (turno.HasValue && turno.Value > 0) { query += " AND hf.CODTURNO = @Turno"; parametros.Add(new MySqlParameter("@Turno", turno.Value)); }
            query += " GROUP BY MODALIDADE ORDER BY QTD_ALUNOS DESC LIMIT 1";
            await using var conn = await AbrirConexaoAsync();
            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddRange(parametros.ToArray());
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) { var mod = reader.GetString("MODALIDADE"); var modFormatada = mod == "P" ? "Presencial" : mod == "D" ? "EAD (À Distância)" : mod; return (modFormatada, reader.GetInt32("QTD_ALUNOS")); }
            return ("N/A", 0);
        }

        //Consulta Curso com mais Alunos
        public async Task<(string Curso, int Quantidade)> GetCursoComMaisAlunosAsync(string campus = null, string modalidade = null, int? turno = null, string periodo = null)
        {
            var query = "SELECT s.NOME AS CURSO, COUNT(DISTINCT m.RA) AS QTD_ALUNOS FROM SMATRICPL m INNER JOIN STURMA tu ON m.CODCOLIGADA = tu.CODCOLIGADA AND m.CODTURMA = tu.CODTURMA AND m.IDPERLET = tu.IDPERLET INNER JOIN SHABILITACAOFILIAL hf ON tu.IDHABILITACAOFILIAL = hf.IDHABILITACAOFILIAL INNER JOIN SHABILITACAOFILIALCAMPUS hfc ON hf.IDHABILITACAOFILIAL = hfc.IDHABILITACAOFILIAL INNER JOIN SCURSO s ON hf.CODCURSO = s.CODCURSO AND hf.CODCOLIGADA = s.CODCOLIGADA WHERE 1=1";
            var parametros = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(campus)) { query += " AND hfc.CODCAMPUS = @Campus"; parametros.Add(new MySqlParameter("@Campus", campus)); }
            if (!string.IsNullOrEmpty(modalidade)) { if (modalidade == "Presencial (sem definição)") query += " AND (s.CURPRESDIST = 'P' OR s.CURPRESDIST IS NULL)"; else { query += " AND s.CURPRESDIST = @Modalidade"; parametros.Add(new MySqlParameter("@Modalidade", modalidade)); } }
            if (turno.HasValue && turno.Value > 0) { query += " AND hf.CODTURNO = @Turno"; parametros.Add(new MySqlParameter("@Turno", turno.Value)); }
            query += " GROUP BY s.CODCURSO, s.NOME ORDER BY QTD_ALUNOS DESC LIMIT 1";
            await using var conn = await AbrirConexaoAsync();
            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddRange(parametros.ToArray());
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) return (reader.GetString("CURSO"), reader.GetInt32("QTD_ALUNOS"));
            return ("N/A", 0);
        }
        // Consultas do Filtro
        public async Task<List<string>> GetCampusDisponiveisAsync()
        {
            var query = "SELECT DISTINCT CODCAMPUS FROM SCAMPUS WHERE ATIVO = 'S' ORDER BY CODCAMPUS";
            var resultado = new List<string>();
            await using var conn = await AbrirConexaoAsync();
            await using var cmd = new MySqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) resultado.Add(reader.GetString("CODCAMPUS"));
            return resultado;
        }
        public async Task<List<(string Codigo, string Nome)>> GetModalidadesDisponiveisAsync(string campus)
        {
            var query = "SELECT DISTINCT COALESCE(s.CURPRESDIST, 'Presencial (sem definição)') AS MODALIDADE FROM SCURSO s INNER JOIN SHABILITACAOFILIAL hf ON s.CODCOLIGADA = hf.CODCOLIGADA AND s.CODCURSO = hf.CODCURSO INNER JOIN SHABILITACAOFILIALCAMPUS hfc ON hf.CODCOLIGADA = hfc.CODCOLIGADA AND hf.IDHABILITACAOFILIAL = hfc.IDHABILITACAOFILIAL WHERE hfc.CODCAMPUS = @campus ORDER BY CASE WHEN s.CURPRESDIST = 'P' THEN 1 WHEN s.CURPRESDIST IS NULL THEN 2 WHEN s.CURPRESDIST = 'D' THEN 3 ELSE 4 END";
            var resultado = new List<(string, string)>();
            await using var conn = await AbrirConexaoAsync();
            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@campus", campus);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) { var codigo = reader.GetString("MODALIDADE"); var nome = codigo == "P" ? "Presencial" : codigo == "D" ? "EAD (À Distância)" : codigo; resultado.Add((codigo, nome)); }
            return resultado;
        }

        public async Task<List<(string Codigo, string Nome)>> GetCursosDisponiveisAsync(string campus, string modalidade = null)
        {
            string whereModalidade = "";
            var parametros = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(modalidade)) { if (modalidade == "Presencial (sem definição)") whereModalidade = " AND (s.CURPRESDIST = 'P' OR s.CURPRESDIST IS NULL)"; else { whereModalidade = " AND s.CURPRESDIST = @modalidade"; parametros.Add(new MySqlParameter("@modalidade", modalidade)); } }
            var query = $"SELECT DISTINCT s.CODCURSO, s.NOME AS NOME_CURSO FROM SCURSO s INNER JOIN SHABILITACAOFILIAL hf ON s.CODCOLIGADA = hf.CODCOLIGADA AND s.CODCURSO = hf.CODCURSO INNER JOIN SHABILITACAOFILIALCAMPUS hfc ON hf.CODCOLIGADA = hfc.CODCOLIGADA AND hf.IDHABILITACAOFILIAL = hfc.IDHABILITACAOFILIAL WHERE hfc.CODCAMPUS = @campus{whereModalidade} ORDER BY s.NOME";
            var resultado = new List<(string, string)>();
            await using var conn = await AbrirConexaoAsync();
            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@campus", campus);
            cmd.Parameters.AddRange(parametros.ToArray());
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) resultado.Add((reader.GetString("CODCURSO"), reader.GetString("NOME_CURSO")));
            return resultado;
        }

        public async Task<List<(int Codigo, string Nome)>> GetTurnosDisponiveisAsync(string campus, string modalidade, string curso)
        {
            string whereModalidade = "";
            var parametros = new List<MySqlParameter>();
            if (modalidade == "Presencial (sem definição)") whereModalidade = " AND (s.CURPRESDIST = 'P' OR s.CURPRESDIST IS NULL)"; else if (!string.IsNullOrEmpty(modalidade)) { whereModalidade = " AND s.CURPRESDIST = @modalidade"; parametros.Add(new MySqlParameter("@modalidade", modalidade)); }
            var query = $"SELECT DISTINCT tur.CODTURNO, tur.NOME AS NOME_TURNO FROM SCURSO s INNER JOIN SHABILITACAOFILIAL hf ON s.CODCOLIGADA = hf.CODCOLIGADA AND s.CODCURSO = hf.CODCURSO INNER JOIN SHABILITACAOFILIALCAMPUS hfc ON hf.CODCOLIGADA = hfc.CODCOLIGADA AND hf.IDHABILITACAOFILIAL = hfc.IDHABILITACAOFILIAL INNER JOIN STURNO tur ON hf.CODCOLIGADA = tur.CODCOLIGADA AND hf.CODTURNO = tur.CODTURNO WHERE hfc.CODCAMPUS = @campus{whereModalidade} AND s.CODCURSO = @curso ORDER BY tur.NOME";
            var resultado = new List<(int, string)>();
            await using var conn = await AbrirConexaoAsync();
            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@campus", campus);
            cmd.Parameters.AddWithValue("@curso", curso);
            cmd.Parameters.AddRange(parametros.ToArray());
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) resultado.Add((reader.GetInt32("CODTURNO"), reader.GetString("NOME_TURNO")));
            return resultado;
        }

        public async Task<List<(string Codigo, string Descricao)>> GetPeriodosDisponiveisAsync(string campus, string modalidade, string curso, int turno)
        {
            string whereModalidade = "";
            var parametros = new List<MySqlParameter>();
            if (modalidade == "Presencial (sem definição)") whereModalidade = " AND (s.CURPRESDIST = 'P' OR s.CURPRESDIST IS NULL)"; else if (!string.IsNullOrEmpty(modalidade)) { whereModalidade = " AND s.CURPRESDIST = @modalidade"; parametros.Add(new MySqlParameter("@modalidade", modalidade)); }
            var query = $"SELECT DISTINCT p.IDPERLET, p.CODPERLET, p.DESCRICAO AS PERIODO_LETIVO, p.DTINICIO, p.DTFIM FROM SCURSO s INNER JOIN SHABILITACAOFILIAL hf ON s.CODCOLIGADA = hf.CODCOLIGADA AND s.CODCURSO = hf.CODCURSO INNER JOIN SHABILITACAOFILIALCAMPUS hfc ON hf.CODCOLIGADA = hfc.CODCOLIGADA AND hf.IDHABILITACAOFILIAL = hfc.IDHABILITACAOFILIAL INNER JOIN STURNO tur ON hf.CODCOLIGADA = tur.CODCOLIGADA AND hf.CODTURNO = tur.CODTURNO INNER JOIN STURMA tu ON hf.CODCOLIGADA = tu.CODCOLIGADA AND hf.IDHABILITACAOFILIAL = tu.IDHABILITACAOFILIAL INNER JOIN SPLETIVO p ON tu.CODCOLIGADA = p.CODCOLIGADA AND tu.IDPERLET = p.IDPERLET WHERE hfc.CODCAMPUS = @campus{whereModalidade} AND s.CODCURSO = @curso AND tur.CODTURNO = @turno ORDER BY p.DTINICIO DESC";
            var resultado = new List<(string, string)>();
            await using var conn = await AbrirConexaoAsync();
            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@campus", campus);
            cmd.Parameters.AddWithValue("@curso", curso);
            cmd.Parameters.AddWithValue("@turno", turno);
            cmd.Parameters.AddRange(parametros.ToArray());
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) { var codigo = reader.GetString("CODPERLET"); var descricao = reader.GetString("PERIODO_LETIVO"); resultado.Add((codigo, $"{codigo} - {descricao}")); }
            return resultado;
        }
        //Consulta Gráfico 5 Campus com mais alunos
        public async Task<List<(string Campus, int Quantidade)>> GetAlunosPorCampusAsync()
        {
            var query = "SELECT c.CODCAMPUS AS NOME_CAMPUS, COUNT(DISTINCT m.RA) AS QTD_ALUNOS FROM SMATRICPL m INNER JOIN STURMA tu ON m.CODCOLIGADA = tu.CODCOLIGADA AND m.CODTURMA = tu.CODTURMA AND m.IDPERLET = tu.IDPERLET INNER JOIN SHABILITACAOFILIAL hf ON tu.IDHABILITACAOFILIAL = hf.IDHABILITACAOFILIAL INNER JOIN SHABILITACAOFILIALCAMPUS hfc ON hf.IDHABILITACAOFILIAL = hfc.IDHABILITACAOFILIAL INNER JOIN SCAMPUS c ON hfc.CODCAMPUS = c.CODCAMPUS GROUP BY c.CODCAMPUS ORDER BY QTD_ALUNOS DESC LIMIT 5";
            var resultado = new List<(string, int)>();
            await using var conn = await AbrirConexaoAsync();
            await using var cmd = new MySqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) resultado.Add((reader.GetString("NOME_CAMPUS"), reader.GetInt32("QTD_ALUNOS")));
            return resultado;
        }

        public async Task<List<(string Modalidade, int Quantidade)>> GetDistribuicaoModalidadeAsync(string campus)
        {
            var query = "SELECT COALESCE(s.CURPRESDIST, 'Presencial (sem definição)') AS MODALIDADE, COUNT(DISTINCT m.RA) AS QTD_ALUNOS FROM SMATRICPL m INNER JOIN STURMA tu ON m.CODCOLIGADA = tu.CODCOLIGADA AND m.CODTURMA = tu.CODTURMA AND m.IDPERLET = tu.IDPERLET INNER JOIN SHABILITACAOFILIAL hf ON tu.IDHABILITACAOFILIAL = hf.IDHABILITACAOFILIAL INNER JOIN SHABILITACAOFILIALCAMPUS hfc ON hf.IDHABILITACAOFILIAL = hfc.IDHABILITACAOFILIAL INNER JOIN SCURSO s ON hf.CODCURSO = s.CODCURSO AND hf.CODCOLIGADA = s.CODCOLIGADA WHERE hfc.CODCAMPUS = @campus GROUP BY MODALIDADE ORDER BY QTD_ALUNOS DESC";
            var resultado = new List<(string, int)>();
            await using var conn = await AbrirConexaoAsync();
            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@campus", campus);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) { var mod = reader.GetString("MODALIDADE"); var modFormatada = mod == "P" ? "Presencial" : mod == "D" ? "EAD" : mod; resultado.Add((modFormatada, reader.GetInt32("QTD_ALUNOS"))); }
            return resultado;
        }
        public async Task<List<(string Curso, int Quantidade)>> GetCursosTopAsync(string? campus = null, string? modalidade = null, int? limit = 5)
        {
            var parametros = new List<MySqlParameter>();
            var where = new List<string>();

            if (!string.IsNullOrEmpty(campus))
            {
                where.Add("hfc.CODCAMPUS = @campus");
                parametros.Add(new MySqlParameter("@campus", campus));
            }

            if (!string.IsNullOrEmpty(modalidade))
            {
                if (modalidade == "Presencial (sem definição)")
                    where.Add("(s.CURPRESDIST = 'P' OR s.CURPRESDIST IS NULL)");
                else
                {
                    where.Add("s.CURPRESDIST = @modalidade");
                    parametros.Add(new MySqlParameter("@modalidade", modalidade));
                }
            }

            var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";

            var limitClause = limit.HasValue ? $"LIMIT {limit.Value}" : "";

            var query = $@"
        SELECT 
            s.NOME AS CURSO, 
            COUNT(DISTINCT m.RA) AS QTD_ALUNOS 
        FROM SMATRICPL m
        INNER JOIN STURMA tu ON m.CODCOLIGADA = tu.CODCOLIGADA AND m.CODTURMA = tu.CODTURMA AND m.IDPERLET = tu.IDPERLET
        INNER JOIN SHABILITACAOFILIAL hf ON tu.IDHABILITACAOFILIAL = hf.IDHABILITACAOFILIAL
        LEFT JOIN SHABILITACAOFILIALCAMPUS hfc ON hf.IDHABILITACAOFILIAL = hfc.IDHABILITACAOFILIAL
        INNER JOIN SCURSO s ON hf.CODCURSO = s.CODCURSO AND hf.CODCOLIGADA = s.CODCOLIGADA
        {whereClause}
        GROUP BY s.CODCURSO, s.NOME
        ORDER BY QTD_ALUNOS DESC
        {limitClause}";

            var resultado = new List<(string, int)>();

            await using var conn = await AbrirConexaoAsync();
            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddRange(parametros.ToArray());

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                resultado.Add((reader.GetString("CURSO"), reader.GetInt32("QTD_ALUNOS")));
            }

            return resultado;
        }
        public async Task<List<(string Turno, int Quantidade)>> GetDistribuicaoTurnoAsync(string campus, string modalidade, string curso)
        {
            var parametros = new List<MySqlParameter>();
            string whereModalidade = "";
            if (modalidade == "Presencial (sem definição)") whereModalidade = " AND (s.CURPRESDIST = 'P' OR s.CURPRESDIST IS NULL)"; else if (!string.IsNullOrEmpty(modalidade)) { whereModalidade = " AND s.CURPRESDIST = @modalidade"; parametros.Add(new MySqlParameter("@modalidade", modalidade)); }
            var query = $"SELECT tur.NOME AS TURNO, COUNT(DISTINCT m.RA) AS QTD_ALUNOS FROM SMATRICPL m INNER JOIN STURMA tu ON m.CODCOLIGADA = tu.CODCOLIGADA AND m.CODTURMA = tu.CODTURMA AND m.IDPERLET = tu.IDPERLET INNER JOIN SHABILITACAOFILIAL hf ON tu.IDHABILITACAOFILIAL = hf.IDHABILITACAOFILIAL INNER JOIN SHABILITACAOFILIALCAMPUS hfc ON hf.IDHABILITACAOFILIAL = hfc.IDHABILITACAOFILIAL INNER JOIN SCURSO s ON hf.CODCURSO = s.CODCURSO AND hf.CODCOLIGADA = s.CODCOLIGADA INNER JOIN STURNO tur ON hf.CODTURNO = tur.CODTURNO AND hf.CODCOLIGADA = tur.CODCOLIGADA WHERE hfc.CODCAMPUS = @campus{whereModalidade} AND s.CODCURSO = @curso GROUP BY tur.CODTURNO, tur.NOME ORDER BY QTD_ALUNOS DESC";
            var resultado = new List<(string, int)>();
            await using var conn = await AbrirConexaoAsync();
            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@campus", campus);
            cmd.Parameters.AddWithValue("@curso", curso);
            cmd.Parameters.AddRange(parametros.ToArray());
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) resultado.Add((reader.GetString("TURNO"), reader.GetInt32("QTD_ALUNOS")));
            return resultado;
        }
        public async Task<List<(string PeriodoLetivo, int Quantidade)>> GetAlunosPorPeriodoLetivoAsync(string campus, string modalidade, string curso, int turno)
        {
            var parametros = new List<MySqlParameter>();
            string whereModalidade = "";

            if (modalidade == "Presencial (sem definição)")
                whereModalidade = " AND (s.CURPRESDIST = 'P' OR s.CURPRESDIST IS NULL)";
            else if (!string.IsNullOrEmpty(modalidade))
            {
                whereModalidade = " AND s.CURPRESDIST = @modalidade";
                parametros.Add(new MySqlParameter("@modalidade", modalidade));
            }

            // QUERY OTIMIZADA - Removendo JOINs desnecessários
            var query = $@"
        SELECT 
            p.CODPERLET,
            COUNT(DISTINCT m.RA) AS QTD_ALUNOS
        FROM SMATRICPL m
        INNER JOIN STURMA tu ON m.CODTURMA = tu.CODTURMA AND m.CODCOLIGADA = tu.CODCOLIGADA AND m.IDPERLET = tu.IDPERLET
        INNER JOIN SPLETIVO p ON tu.IDPERLET = p.IDPERLET AND tu.CODCOLIGADA = p.CODCOLIGADA
        INNER JOIN SHABILITACAOFILIAL hf ON tu.IDHABILITACAOFILIAL = hf.IDHABILITACAOFILIAL
        INNER JOIN SHABILITACAOFILIALCAMPUS hfc ON hf.IDHABILITACAOFILIAL = hfc.IDHABILITACAOFILIAL
        INNER JOIN SCURSO s ON hf.CODCURSO = s.CODCURSO AND hf.CODCOLIGADA = s.CODCOLIGADA
        WHERE hfc.CODCAMPUS = @campus
          {whereModalidade}
          AND s.CODCURSO = @curso
          AND hf.CODTURNO = @turno
        GROUP BY p.CODPERLET
        ORDER BY p.CODPERLET DESC
        LIMIT 10";

            var resultado = new List<(string, int)>();

            await using var conn = await AbrirConexaoAsync();
            await using var cmd = new MySqlCommand(query, conn);
            cmd.CommandTimeout = 60; // ⬅️ AUMENTA O TIMEOUT PARA 60 SEGUNDOS
            cmd.Parameters.AddWithValue("@campus", campus);
            cmd.Parameters.AddWithValue("@curso", curso);
            cmd.Parameters.AddWithValue("@turno", turno);
            cmd.Parameters.AddRange(parametros.ToArray());

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                resultado.Add((reader.GetString("CODPERLET"), reader.GetInt32("QTD_ALUNOS")));

            return resultado;
        }
        // ========================================
        // NOVOS MÉTODOS PARA VISÃO GERAL (SEM FILTRO)
        // ========================================

        public async Task<Dictionary<string, int>> GetDistribuicaoModalidadeGeralAsync()
        {
            var query = @"
        SELECT 
            CASE 
                WHEN s.CURPRESDIST = 'P' OR s.CURPRESDIST IS NULL THEN 'Presencial'
                WHEN s.CURPRESDIST = 'D' THEN 'EAD (À Distância)'
                ELSE COALESCE(s.CURPRESDIST, 'Presencial (sem definição)')
            END AS MODALIDADE,
            COUNT(DISTINCT m.RA) AS QTD_ALUNOS
        FROM SMATRICPL m
        INNER JOIN STURMA tu ON m.CODCOLIGADA = tu.CODCOLIGADA AND m.CODTURMA = tu.CODTURMA AND m.IDPERLET = tu.IDPERLET
        INNER JOIN SHABILITACAOFILIAL hf ON tu.IDHABILITACAOFILIAL = hf.IDHABILITACAOFILIAL
        INNER JOIN SCURSO s ON hf.CODCURSO = s.CODCURSO AND hf.CODCOLIGADA = s.CODCOLIGADA
        GROUP BY MODALIDADE
        ORDER BY QTD_ALUNOS DESC";

            var resultado = new Dictionary<string, int>();

            await using var conn = await AbrirConexaoAsync();
            await using var cmd = new MySqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string modalidade = reader.GetString("MODALIDADE");
                int quantidade = reader.GetInt32("QTD_ALUNOS");
                resultado[modalidade] = quantidade;
            }

            return resultado;
        }
        public async Task<Dictionary<string, int>> GetDistribuicaoTurnoGeralAsync()
        {
            var query = @"
        SELECT 
            tur.NOME AS TURNO,
            COUNT(DISTINCT m.RA) AS QTD_ALUNOS
        FROM SMATRICPL m
        INNER JOIN STURMA tu ON m.CODCOLIGADA = tu.CODCOLIGADA AND m.CODTURMA = tu.CODTURMA AND m.IDPERLET = tu.IDPERLET
        INNER JOIN SHABILITACAOFILIAL hf ON tu.IDHABILITACAOFILIAL = hf.IDHABILITACAOFILIAL
        INNER JOIN STURNO tur ON hf.CODTURNO = tur.CODTURNO AND hf.CODCOLIGADA = tur.CODCOLIGADA
        GROUP BY tur.CODTURNO, tur.NOME
        ORDER BY QTD_ALUNOS DESC";

            var resultado = new Dictionary<string, int>();

            await using var conn = await AbrirConexaoAsync();
            await using var cmd = new MySqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string turno = reader.GetString("TURNO");
                int quantidade = reader.GetInt32("QTD_ALUNOS");
                resultado[turno] = quantidade;
            }

            return resultado;
        }

        // Adicione este método no final da classe AlunosService
        public async Task<List<(string Turma, int Quantidade)>> GetTurmasPorPeriodoAsync(string campus, string modalidade, string curso, int turno, string periodo)
        {
            var parametros = new List<MySqlParameter>();
            string whereModalidade = "";

            if (modalidade == "Presencial (sem definição)")
                whereModalidade = " AND (s.CURPRESDIST = 'P' OR s.CURPRESDIST IS NULL)";
            else if (!string.IsNullOrEmpty(modalidade))
            {
                whereModalidade = " AND s.CURPRESDIST = @modalidade";
                parametros.Add(new MySqlParameter("@modalidade", modalidade));
            }

            var query = $@"
        SELECT 
            tu.CODTURMA AS TURMA,
            COUNT(DISTINCT m.RA) AS QTD_ALUNOS
        FROM SMATRICPL m
        INNER JOIN STURMA tu ON m.CODTURMA = tu.CODTURMA AND m.CODCOLIGADA = tu.CODCOLIGADA AND m.IDPERLET = tu.IDPERLET
        INNER JOIN SPLETIVO p ON tu.IDPERLET = p.IDPERLET AND tu.CODCOLIGADA = p.CODCOLIGADA
        INNER JOIN SHABILITACAOFILIAL hf ON tu.IDHABILITACAOFILIAL = hf.IDHABILITACAOFILIAL
        INNER JOIN SHABILITACAOFILIALCAMPUS hfc ON hf.IDHABILITACAOFILIAL = hfc.IDHABILITACAOFILIAL
        INNER JOIN SCURSO s ON hf.CODCURSO = s.CODCURSO AND hf.CODCOLIGADA = s.CODCOLIGADA
        WHERE hfc.CODCAMPUS = @campus
          {whereModalidade}
          AND s.CODCURSO = @curso
          AND hf.CODTURNO = @turno
          AND p.CODPERLET = @periodo
        GROUP BY tu.CODTURMA
        ORDER BY QTD_ALUNOS DESC";

            var resultado = new List<(string, int)>();

            await using var conn = await AbrirConexaoAsync();
            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@campus", campus);
            cmd.Parameters.AddWithValue("@curso", curso);
            cmd.Parameters.AddWithValue("@turno", turno);
            cmd.Parameters.AddWithValue("@periodo", periodo);
            cmd.Parameters.AddRange(parametros.ToArray());

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string turma = reader.GetString("TURMA");
                int qtd = reader.GetInt32("QTD_ALUNOS");
                resultado.Add((turma, qtd));
            }

            return resultado;
        }

        public async Task<List<(string RA, string Nome)>> GetAlunosDaTurmaAsync(
    string campus, string modalidade, string curso, int turno, string periodo, string turma)
        {
            var parametros = new List<MySqlParameter>();
            string whereModalidade = "";
            if (modalidade == "Presencial (sem definição)")
                whereModalidade = " AND (s.CURPRESDIST = 'P' OR s.CURPRESDIST IS NULL)";
            else if (!string.IsNullOrEmpty(modalidade) && modalidade != "Todos")
            {
                whereModalidade = " AND s.CURPRESDIST = @modalidade";
                parametros.Add(new MySqlParameter("@modalidade", modalidade[0])); // P ou D
            }

            var query = $@"
        SELECT DISTINCT
            m.RA,
            p.NOME AS Nome
        FROM SMATRICPL m
        INNER JOIN SALUNO alu        ON alu.CODCOLIGADA = m.CODCOLIGADA AND alu.RA = m.RA
        INNER JOIN PPESSOA p         ON p.CODIGO = alu.CODPESSOA                  -- <<< AQUI ESTÁ O NOME!!!
        INNER JOIN STURMA tu         ON tu.CODCOLIGADA = m.CODCOLIGADA 
                                        AND tu.CODTURMA = m.CODTURMA 
                                        AND tu.IDPERLET = m.IDPERLET
        INNER JOIN SPLETIVO pl       ON pl.CODCOLIGADA = tu.CODCOLIGADA AND pl.IDPERLET = tu.IDPERLET
        INNER JOIN SHABILITACAOFILIAL hf ON hf.CODCOLIGADA = tu.CODCOLIGADA 
                                            AND hf.IDHABILITACAOFILIAL = tu.IDHABILITACAOFILIAL
        INNER JOIN SHABILITACAOFILIALCAMPUS hfc ON hfc.CODCOLIGADA = hf.CODCOLIGADA 
                                                   AND hfc.IDHABILITACAOFILIAL = hf.IDHABILITACAOFILIAL
        INNER JOIN SCURSO s          ON s.CODCOLIGADA = hf.CODCOLIGADA AND s.CODCURSO = hf.CODCURSO
        WHERE hfc.CODCAMPUS = @campus
          {whereModalidade}
          AND s.CODCURSO = @curso
          AND hf.CODTURNO = @turno
          AND pl.CODPERLET = @periodo
          AND tu.CODTURMA = @turma

        ORDER BY p.NOME";

            var resultado = new List<(string RA, string Nome)>();

            await using var conn = await AbrirConexaoAsync();
            await using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@campus", campus);
            cmd.Parameters.AddWithValue("@curso", curso);
            cmd.Parameters.AddWithValue("@turno", turno);
            cmd.Parameters.AddWithValue("@periodo", periodo);
            cmd.Parameters.AddWithValue("@turma", turma);
            if (parametros.Any()) cmd.Parameters.AddRange(parametros.ToArray());

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                resultado.Add((
                    reader.GetString("RA"),
                    reader.GetString("Nome")
                ));
            }

            return resultado;
        }
    }
}
