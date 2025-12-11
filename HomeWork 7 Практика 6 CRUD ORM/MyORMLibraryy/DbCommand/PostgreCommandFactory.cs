using Npgsql;
using System.Data;

namespace MyORMLibrary
{
    internal class PostgreCommandFactory : DbCommandFactory
    {
        public override IDbCommand CreateCommand()
        {
            return new NpgsqlCommand();
        }
    }
}
