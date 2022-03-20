using Dapper;
using Microsoft.Extensions.Configuration;
using SearchEngine.Library.Internal;
using SearchEngine.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Library.DataAccess
{
    public class DocumentData
    {
        //private readonly IConfiguration _config;
        private readonly string _connString;

        public DocumentData(string connectionString)
        {
            //_config = config;
            _connString = connectionString;
        }

        public void CreateDocument(DocumentForCreationModel document)
        {
            // more validation

            if (document is null)
            {
                throw new ArgumentNullException(nameof(document), "Invalid document passed in.");
            }

            //// flow
            // 1. does document exist?
            // 2. yes - delete tokens for old instance
            //    no - insert document only
            // 3. insert tokens for the document

            SqlDataAccess sql = new(_connString);
            string query = "SELECT Id FROM Documents WHERE Id = @Id";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", document.Id, System.Data.DbType.Int32);

            DocumentDbModel doc = sql.LoadData<DocumentDbModel, dynamic>(query, parameters).FirstOrDefault();

            if (doc is null)
            {
                sql.SaveData<dynamic>("dbo.InsertDocument", new { DocumentId = document.Id });
            }
            else
            {
                // delete tokens for old instance
            }
            

            sql.SaveData<dynamic>("dbo.InsertTokensForDocument", new { DocumentId = document.Id, TokensList = document.Tokens });
        }
    }
}
