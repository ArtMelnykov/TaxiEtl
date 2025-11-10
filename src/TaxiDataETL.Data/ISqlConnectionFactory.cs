using Microsoft.Data.SqlClient;

namespace TaxiDataETL.Data
{
    public interface ISqlConnectionFactory
    {
        SqlConnection Create();
    }
}