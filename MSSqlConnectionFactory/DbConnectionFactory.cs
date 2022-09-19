using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSqlConnectionFactory
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IDictionary<string?, string?> _connectionStringsDictionary;

        public DbConnectionFactory(IDictionary<string?, string?> connectionStringsDictionary)
        {
            _connectionStringsDictionary = connectionStringsDictionary;
        }

        private string GetConnectionString(string? connectionName)
        {
            _connectionStringsDictionary.TryGetValue(connectionName, out string? connectionString);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception(string.Format("Connection string {0} was not found", connectionName));
            }
            return connectionString;
        }

        private IDbConnection CreateDbConnection(string? connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception(string.Format("Connection string {0} was not found", connectionString));
            }
            var connection = new SqlConnection(connectionString);
            return connection;
        }

        public IDbConnection MSSqlConnection(string? connectionName)
        {
            return CreateDbConnection(GetConnectionString(connectionName));
        }
    }
}
