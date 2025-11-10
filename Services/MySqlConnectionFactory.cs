using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace MauiApp1.Services
{
	public interface IMySqlConnectionFactory
	{
		MySqlConnection GetConnection();
		Task<MySqlConnection> OpenConnectionAsync(CancellationToken ct = default);
	}

	public class MySqlConnectionFactory : IMySqlConnectionFactory
	{
		private readonly IConfiguration _config;
		private readonly string _connectionString;

		public MySqlConnectionFactory(IConfiguration config)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));
			_connectionString = BuildConnectionString();
		}

		private string BuildConnectionString()
		{
			// Prefer an explicit ConnectionString, else assemble from parts.
			var connStr = _config["MySql:ConnectionString"];
			if (!string.IsNullOrWhiteSpace(connStr)) return connStr!;

			string host = _config["MySql:Host"] ?? "localhost";
			string port = _config["MySql:Port"] ?? "3306";
			string db = _config["MySql:Database"] ?? "heicomp";
			string user = _config["MySql:User"] ?? "root";
			string pwd = _config["MySql:Password"] ?? string.Empty;
			string ssl = _config["MySql:SslMode"] ?? "Preferred";
			string allowPk = _config["MySql:AllowPublicKeyRetrieval"] ?? "true";
			return $"Server={host};Port={port};Database={db};User ID={user};Password={pwd};SslMode={ssl};AllowPublicKeyRetrieval={allowPk}";
		}

		public MySqlConnection GetConnection() => new MySqlConnection(_connectionString);

		public async Task<MySqlConnection> OpenConnectionAsync(CancellationToken ct = default)
		{
			var conn = GetConnection();
			await conn.OpenAsync(ct);
			return conn;
		}
	}
}