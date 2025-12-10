using Npgsql;
using System.Data;

namespace MyORMLibrary
{
    class PostgreConnectionFactory : DbConnectFactory
    {
        private string connectionString;
        public PostgreConnectionFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public override IDbConnection CreateConnect()
        {
            return new NpgsqlConnection(connectionString);
        }
    }
}