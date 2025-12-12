using System.Data;

namespace MyORMLibrary
{
    public abstract class DbCommandFactory
    {
        public abstract IDbCommand CreateCommand();
    }
}
