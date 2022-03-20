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
        //private readonly IConfiguration _config;
        private readonly string _connString;

        public SqlDataAccess(string connectionString)
        {
            _connString = connectionString;
        }

        public List<T> LoadData<T, U>(string storedProcedure, U parameters)
        {
            using IDbConnection connection = new SqlConnection(_connString);
            List<T> rows = connection.Query<T>(storedProcedure, parameters,
                commandType: CommandType.StoredProcedure).ToList();

            return rows;
        }

        // refactor with above method partially?
        public void SaveData<T>(string storedProcedure, T parameters)
        {
            using IDbConnection connection = new SqlConnection(_connString);
            connection.Execute(storedProcedure, parameters,
                commandType: CommandType.StoredProcedure);
        }

    }
}
