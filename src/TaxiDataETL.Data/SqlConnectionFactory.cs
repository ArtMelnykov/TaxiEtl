using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TaxiDataETL.Data
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("TaxiDb")
                                 ?? throw new InvalidOperationException(
                                     "Connection string 'TaxiDb' is not configured");
        }

        public SqlConnection Create() => new SqlConnection(_connectionString);
    }
}