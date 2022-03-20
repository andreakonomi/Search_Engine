using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Library.Internal
{
    /// <summary>
    /// Implements the low level interaction with the db. This will be hidden from
    /// the consumer of the dll.
    /// </summary>
    internal class SqlDataAccess
    {
        private readonly IConfiguration _config;

        public SqlDataAccess(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Gets the connection string with the specified key from
        /// the configurations provided.
        /// </summary>
        /// <param name="name">The key of the connection string on the
        /// configurations.</param>
        /// <returns>The connection string to be used.</returns>
        public string GetConnectionString(string name)
        {
            return _config.GetConnectionString(name);
        }

        public List<T> LoadData<T, U>(string storedProcedure, U parameters, string connectionStringName)
        {
            string connectionString = GetConnectionString(connectionStringName);

            using IDbConnection connection = new SqlConnection(connectionString);
            List<T> rows = connection.Query<T>(storedProcedure, parameters,
                commandType: CommandType.StoredProcedure).ToList();

            return rows;
        }

        // refactor with above method partially?
        public void SaveData<T>(string storedProcedure, T parameters, string connectionStringName)
        {
            string connectionString = GetConnectionString(connectionStringName);

            using IDbConnection connection = new SqlConnection(connectionString);
            connection.Execute(storedProcedure, parameters,
                commandType: CommandType.StoredProcedure);
        }

    }
}
