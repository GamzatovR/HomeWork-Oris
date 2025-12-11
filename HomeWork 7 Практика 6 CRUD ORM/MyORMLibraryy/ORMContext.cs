using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MyORMLibrary
{
    public class ORMContext
    {
        private readonly string _connectionString;

        public IDbConnection ConnectionOverride { get; set; }

        public ORMContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        public T Create<T>(T entity, string tableName) where T : class
        {
            if (!Regex.IsMatch(tableName, @"^[a-zA-Z0-9_]+$"))
                throw new Exception("Invalid table name");

            PropertyInfo[] properties = typeof(T).GetProperties().Where(p => !string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase)).ToArray();

            using (IDbConnection connection = new PostgreConnectionFactory(_connectionString).CreateConnect())
            {
                connection.Open();

                List<string> propersName = properties.Select(p => p.Name).ToList();
                List<string> propersNameValue = propersName.Select(p => "@" + p).ToList();

                string columnNames = string.Join(',', propersName.Select(p => $"{p.ToLower()}"));

                string sql = $"INSERT INTO {tableName.ToLower()} ({columnNames}) " +
                    $"VALUES ({string.Join(',', propersNameValue)}) RETURNING id;";

                using (IDbCommand command = new PostgreCommandFactory().CreateCommand())
                {
                    command.CommandText = sql;
                    command.Connection = connection;

                    foreach (var prop in properties)
                    {
                        IDbDataParameter parametr = command.CreateParameter();
                        parametr.ParameterName = "@" + prop.Name;
                        parametr.Value = prop.GetValue(entity) ?? DBNull.Value;
                        command.Parameters.Add(parametr);
                    }
                    int id = Convert.ToInt32(command.ExecuteScalar());

                    PropertyInfo idProp = typeof(T).GetProperty("id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    idProp.SetValue(entity, id);
                }
            }
            return entity;
        }
        
        public List<T> Read<T>(string tableName, Expression<Func<T, bool>> predicate = null) where T : class
        {
            if (!Regex.IsMatch(tableName, @"^[a-zA-Z0-9_]+$"))
                throw new Exception("Invalid table name");

            List<T> users = new List<T>();
            var properties = typeof(T).GetProperties();

            using (IDbConnection connection = new PostgreConnectionFactory(_connectionString).CreateConnect())
            {
                connection.Open();

                var sqlBuilder = new SqlExpressionBuilder();

                string whereSql = "";
                Dictionary<string, object> parameters = new();

                if (predicate != null)
                {
                    whereSql = sqlBuilder.Build(predicate);
                    parameters = sqlBuilder.Parameters;
                }

                string sql = $"SELECT * FROM {tableName.ToLower()}";
                if (!string.IsNullOrEmpty(whereSql))
                {
                    sql += $" WHERE {whereSql}";
                }

                IDbCommand command = new PostgreCommandFactory().CreateCommand();
                command.CommandText = sql;
                command.Connection = connection;
                
                foreach (var param in parameters)
                {
                    IDbDataParameter parametr = command.CreateParameter();
                    parametr.ParameterName = param.Key;
                    parametr.Value = param.Value ?? DBNull.Value;
                    command.Parameters.Add(parametr);
                }
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var instance = Support.CycleforReaders<T>(reader, properties);
                        users.Add(instance);
                    }
                }
            }
            return users;
        }

        public T ReadById<T>(int id, string tableName) where T : class
        {
            if (!Regex.IsMatch(tableName, @"^[a-zA-Z0-9_]+$"))
                throw new Exception("Invalid table name");

            T instance = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties();

            using (IDbConnection connection = new PostgreConnectionFactory(_connectionString).CreateConnect())
            {
                connection.Open();

                string sql = $"SELECT * FROM {tableName.ToLower()} Where Id = @id";

                IDbCommand command = new PostgreCommandFactory().CreateCommand();
                command.CommandText = sql;
                command.Connection = connection;

                IDbDataParameter param = command.CreateParameter();
                param.ParameterName = "@id";
                param.Value = id;

                command.Parameters.Add(param);

                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        instance = Support.CycleforReaders<T>(reader, properties);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            return instance;
        }

        public List<T> ReadByAll<T>(string tableName) where T : class
        {
            if (!Regex.IsMatch(tableName, @"^[a-zA-Z0-9_]+$"))
                throw new Exception("Invalid table name");

            List<T> users = new List<T>();
            var properties = typeof(T).GetProperties();
            using (IDbConnection connection = ConnectionOverride ?? new PostgreConnectionFactory(_connectionString).CreateConnect())
            {
                connection.Open();
                string sql = $"SELECT * FROM {tableName.ToLower()}";
                IDbCommand command = new PostgreCommandFactory().CreateCommand();
                command.CommandText = sql;
                command.Connection = connection;

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var instance = Support.CycleforReaders<T>(reader, properties);
                        users.Add(instance);
                    }
                }
            }
            return users;
        }

        public void Update<T>(int id, T entity, string tableName) where T: class
        {
            if (!Regex.IsMatch(tableName, @"^[a-zA-Z0-9_]+$"))
                throw new Exception("Invalid table name");

            T instanceUpdates = ReadById<T>(id, tableName);
            if (instanceUpdates == null)
            {
                throw new Exception($"Entity with id={id} not found");
            }
            PropertyInfo[] property = typeof(T).GetProperties().Where(p => !string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase)).ToArray();
            
            Dictionary<string, object> forUpdateProps = new Dictionary<string, object>();

            foreach (var prop in property)
            {
                if (!Equals(prop.GetValue(instanceUpdates), prop.GetValue(entity)))
                {
                    forUpdateProps.Add(prop.Name, prop.GetValue(entity));
                }
            }
            if(!forUpdateProps.Any())
            {
                return;
            }
            List<string> forUpdatePropsNamesAndValues = forUpdateProps.Select(p => $"{p.Key.ToLower()} = @{p.Key}").ToList();

            Type entityType = typeof(T);
            using (IDbConnection connection = new PostgreConnectionFactory(_connectionString).CreateConnect())
            {
                connection.Open();
                string sql = $"UPDATE {tableName.ToLower()}" +
                    $" SET {string.Join(',', forUpdatePropsNamesAndValues)} " +
                    $"WHERE Id = @id";
                using (IDbCommand command = new PostgreCommandFactory().CreateCommand())
                {
                    command.CommandText = sql;
                    command.Connection = connection;

                    IDbDataParameter parametr = command.CreateParameter();
                    parametr.ParameterName = "@id";
                    parametr.Value = id;
                    command.Parameters.Add(parametr);

                    foreach (var param in forUpdateProps)
                    {
                        IDbDataParameter parametry = command.CreateParameter();
                        parametry.ParameterName = $"@{param.Key}";
                        parametry.Value = param.Value;
                        command.Parameters.Add(parametry);
                    }
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id, string tableName)
        {
            if (!Regex.IsMatch(tableName, @"^[a-zA-Z0-9_]+$"))
                throw new Exception("Invalid table name");
            using (IDbConnection connection = new PostgreConnectionFactory(_connectionString).CreateConnect())
            {
                connection.Open();
                string sql = $"DELETE FROM {tableName.ToLower()} WHERE Id = @id";
                using (IDbCommand command = new PostgreCommandFactory().CreateCommand())
                {
                    command.CommandText = sql;
                    command.Connection = connection;

                    IDbDataParameter parametr = command.CreateParameter();
                    parametr.ParameterName = "@id";
                    parametr.Value = id;
                    command.Parameters.Add(parametr);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
