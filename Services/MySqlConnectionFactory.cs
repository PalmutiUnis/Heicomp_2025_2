using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace MauiApp1.Services
{
    public interface IMySqlConnectionFactory
    {
        MySqlConnection GetConnection(string databaseKey = "RHSenior");
        Task<MySqlConnection> OpenConnectionAsync(string databaseKey = "RHSenior", CancellationToken ct = default);
    }

    public class MySqlConnectionFactory : IMySqlConnectionFactory
    {
        private readonly IConfiguration _config;

        public MySqlConnectionFactory(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        private string BuildConnectionString(string databaseKey)
        {
            // Primeiro tenta pegar a ConnectionString completa
            var connStr = _config[$"MySql:{databaseKey}:ConnectionString"];
            if (!string.IsNullOrWhiteSpace(connStr))
                return connStr;

            // Monta manualmente se necessário
            string prefix = $"MySql:{databaseKey}";

            string host = _config[$"{prefix}:Host"] ?? "10.0.2.2";
            string port = _config[$"{prefix}:Port"] ?? "3306";
            string db = _config[$"{prefix}:Database"] ?? "";
            string user = _config[$"{prefix}:User"] ?? "root";
            string pwd = _config[$"{prefix}:Password"] ?? "";
            string ssl = _config[$"{prefix}:SslMode"] ?? "Preferred";
            string allowPk = _config[$"{prefix}:AllowPublicKeyRetrieval"] ?? "true";

            return $"Server={host};Port={port};Database={db};User ID={user};Password={pwd};SslMode={ssl};AllowPublicKeyRetrieval={allowPk}";
        }

        public MySqlConnection GetConnection(string databaseKey = "RHSenior")
        {
            var connectionString = BuildConnectionString(databaseKey);
            return new MySqlConnection(connectionString);
        }

        public async Task<MySqlConnection> OpenConnectionAsync(string databaseKey = "RHSenior", CancellationToken ct = default)
        {
            var connectionString = BuildConnectionString(databaseKey);

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException($"Nenhuma string de conexão encontrada para '{databaseKey}'.");

            var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync(ct);

            return conn;
        }
    }
}
