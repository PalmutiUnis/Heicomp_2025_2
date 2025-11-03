using MySqlConnector;

namespace MauiApp1.Services
{
    public class TurnoverRepository
    {
        private readonly string _connectionString;

        public TurnoverRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<decimal> GetTurnoverAsync()
        {
            string query = @"
                SELECT 
                    ROUND(
                        (
                            (
                                (SELECT COUNT(*) 
                                 FROM rhsenior_heicomp.rhdataset
                                 WHERE `Admissão` IS NOT NULL
                                   AND `Admissão` != ''
                                   AND STR_TO_DATE(`Admissão`, '%d/%m/%Y') >= DATE_SUB(CURDATE(), INTERVAL 1 YEAR)
                                )
                                +
                                (SELECT COUNT(*) 
                                 FROM rhsenior_heicomp.rhdataset
                                 WHERE `Descrição (Situação)` LIKE 'Dem%'
                                   AND `Data Afastamento` IS NOT NULL
                                   AND `Data Afastamento` != ''
                                   AND STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y') >= DATE_SUB(CURDATE(), INTERVAL 1 YEAR)
                                )
                            ) / 2
                        ) 
                        / 
                        (SELECT COUNT(*) 
                         FROM rhsenior_heicomp.rhdataset
                         WHERE `Admissão` IS NOT NULL
                           AND `Admissão` != ''
                        ) 
                        * 100
                    , 2) AS Turnover";

            await using var connection = new MySqlConnection(_connectionString);
            await using var command = new MySqlCommand(query, connection);
            
            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            
            return result != DBNull.Value && result != null ? Convert.ToDecimal(result) : 0;
        }

        public async Task<int> GetAdmissoesUltimoAnoAsync()
        {
            string query = @"
                SELECT COUNT(*) 
                FROM rhsenior_heicomp.rhdataset
                WHERE `Admissão` IS NOT NULL
                  AND `Admissão` != ''
                  AND STR_TO_DATE(`Admissão`, '%d/%m/%Y') >= DATE_SUB(CURDATE(), INTERVAL 1 YEAR)";

            await using var connection = new MySqlConnection(_connectionString);
            await using var command = new MySqlCommand(query, connection);
            
            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<int> GetDesligamentosUltimoAnoAsync()
        {
            string query = @"
                SELECT COUNT(*) 
                FROM rhsenior_heicomp.rhdataset
                WHERE `Descrição (Situação)` LIKE 'Dem%'
                  AND `Data Afastamento` IS NOT NULL
                  AND `Data Afastamento` != ''
                  AND STR_TO_DATE(`Data Afastamento`, '%d/%m/%Y') >= DATE_SUB(CURDATE(), INTERVAL 1 YEAR)";

            await using var connection = new MySqlConnection(_connectionString);
            await using var command = new MySqlCommand(query, connection);
            
            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<int> GetTotalColaboradoresAsync()
        {
            string query = @"
                SELECT COUNT(*) 
                FROM rhsenior_heicomp.rhdataset
                WHERE `Admissão` IS NOT NULL
                  AND `Admissão` != ''";

            await using var connection = new MySqlConnection(_connectionString);
            await using var command = new MySqlCommand(query, connection);
            
            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }
}