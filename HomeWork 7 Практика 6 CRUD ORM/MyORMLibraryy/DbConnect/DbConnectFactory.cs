using System.Data;

namespace MyORMLibrary
{
    abstract class DbConnectFactory
    {
        public abstract IDbConnection CreateConnect();
    }
}
