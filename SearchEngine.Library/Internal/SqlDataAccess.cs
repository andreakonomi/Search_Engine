using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
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
        //private readonly IConfiguration _config;
        private readonly string _connString;

        public SqlDataAccess(string connectionString)
        {
            _connString = connectionString;
        }

        public List<T> LoadData<T, U>(string query, U parameters)
        {
            using IDbConnection connection = new SQLiteConnection(_connString);
            List<T> rows = connection.Query<T>(query, parameters).ToList();

            return rows;
        }

        // refactor with above method partially?
        public void SaveData<T>(string query, T parameters)
        {
            using IDbConnection connection = new SQLiteConnection(_connString);
            connection.Execute(query, parameters);
        }

    }
}
