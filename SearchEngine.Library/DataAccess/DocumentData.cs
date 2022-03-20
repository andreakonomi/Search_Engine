using Dapper;
using Microsoft.Extensions.Configuration;
using SearchEngine.Library.Internal;
using SearchEngine.Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Library.DataAccess
{
    public class DocumentData
    {
        private readonly string _connString;

        public DocumentData(string connectionString)
        {
            _connString = connectionString;
        }

        public void CreateDocument(DocumentForCreationModel document)
        {
            //more validation

            if (document is null)
            {
                throw new ArgumentNullException(nameof(document), "Invalid document passed in.");
            }

            SqlDataAccess sql = new(_connString);
            int Id = document.Id;

            string query = "SELECT Id FROM Documents WHERE Id = @Id";

            DocumentDbModel doc = sql.LoadData<DocumentDbModel, dynamic>(query, new {Id}).FirstOrDefault();

            if (doc is null)
            {
                sql.SaveData<dynamic>("INSERT INTO Documents(Id) VALUES(@Id)", new { Id });
            }
            else
            {
                sql.SaveData<dynamic>("DELETE FROM Tokens WHERE DocumentId = @Id", new { Id });
            }

            query = "INSERT INTO Tokens(Content, DocumentId) VALUES (@Content, @docId)";
            sql.SaveTokens(query, document.Tokens, Id);
            
        }
    }
}
