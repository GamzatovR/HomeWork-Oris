using System.Data;

namespace MyORMLibrary
{
    abstract class DbCommandFactory
    {
        public abstract IDbCommand CreateCommand();
    }
}
