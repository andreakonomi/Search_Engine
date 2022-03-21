using Dapper;
using Microsoft.Extensions.Configuration;
using SearchEngine.Library.Helpers;
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
            _connString = connectionString ?? throw new ArgumentNullException(nameof(connectionString), "Connection string can't be null");
        }

        public void CreateDocument(DocumentForCreationModel document)
        {
            CheckIfDocumentIsValid(document);

            SqlDataAccess sql = new(_connString);
            int Id = document.Id;

            DocumentDbModel doc = GetDocument(sql, Id);

            if (doc is null)
            {
                InsertDocument(Id, sql);
            }
            else
            {
                DeleteDocument(Id, sql);
            }

            InsertTokensForDocument(document, sql);
        }

        public List<int> SearchByTokens(string queryExpression)
        {
            if (string.IsNullOrWhiteSpace(queryExpression))
            {
                return null;
            }

            //string query = queryExpression
            //    .Replace("|", " or Content = ", StringComparison.OrdinalIgnoreCase)
            //    .Replace("&", " and Content = ", StringComparison.OrdinalIgnoreCase)
            //    .Replace("(", "(Content = ", StringComparison.OrdinalIgnoreCase);

            SqlDataAccess sql = new SqlDataAccess(_connString);
            var results = SearchDocuments(sql, queryExpression.Trim());

            return results;
        }

        private void CheckIfDocumentIsValid(DocumentForCreationModel document)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document), "Invalid document passed in.");
            }

            CheckIfTokensAreValid(document.Tokens);
        }

        private void CheckIfTokensAreValid(ICollection<TokenForCreationModel> tokens)
        {
            bool tokensValid = tokens.All(x => CheckTokenContentIfValid(x.Content));
            if (!tokensValid)
            {
                throw new FormatException("One of the tokens is not alphanumerical.");
            }
        }

        /// <summary>
        /// Validates if the content of a token is valid
        /// </summary>
        /// <returns>true if content is valid, otherwise false</returns>
        private bool CheckTokenContentIfValid(string content)
        {
            return content.All(c => char.IsLetterOrDigit(c));
        }

        /// <summary>
        /// Gets the Document matching the given id.
        /// </summary>
        /// <param name="sql">DataAccess provider</param>
        /// <param name="Id">Id of the document</param>
        /// <returns>Instance of the found document or null if none found</returns>
        private DocumentDbModel GetDocument(SqlDataAccess sql, int Id)
        {
            string query = "SELECT Id FROM Documents WHERE Id = @Id";
            try
            {
                return sql.LoadData<DocumentDbModel, dynamic>(query, new { Id }).FirstOrDefault();

            }
            catch (Exception)
            {
                throw new Exception($"An error occurred while reading the document with id:{Id} from the database.");
            }
        }

        private List<int> SearchDocuments(SqlDataAccess sql, string queryExpression)
        {
            queryExpression = FormatFieldValuesForSqlQuery(queryExpression);

            queryExpression = queryExpression
                .Replace("|", " or Content = ", StringComparison.OrdinalIgnoreCase)
                .Replace("&", " and Content = ", StringComparison.OrdinalIgnoreCase)
                .Replace("(", "(Content = ", StringComparison.OrdinalIgnoreCase);

            string query = "SELECT DISTINCT DocumentId from Tokens WHERE ";

            if (queryExpression.StartsWith('('))
            {
                query += queryExpression;
            }
            else
            {
                query += "Content = ";
                query += queryExpression;
            }

            var result = sql.LoadData<int, dynamic>(query, new { });
            return result;
        }

        private string FormatFieldValuesForSqlQuery(string query)
        {
            string removedChars = query
                .Replace("(", null)
                .Replace(")", null)
                .Replace("|", null)
                .Replace("&", null);

            var tokens = removedChars.Split(" ");

            foreach (var value in tokens)
            {
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                query = query.ReplaceWholeWord(value, $"'{value}'");
            }

            return query;
        }

        //private string FormatFieldValuesForSqlQuery2(string query)
        //{
        //    // (a | b) & c


        //}

        private void InsertTokensForDocument(DocumentForCreationModel document, SqlDataAccess sql)
        {
            string query = "INSERT INTO Tokens(Content, DocumentId) VALUES(@Content, @docId)";
            int Id = document.Id;

            try
            {
                sql.SaveTokens(query,document.Tokens, Id);
            }
            catch (Exception)
            {
                throw new Exception($"Was not possible inserting the tokens for the document with id: {Id} to the database.");
            }        
        }

        private void InsertDocument(int Id, SqlDataAccess sql)
        {
            try
            {
                sql.SaveData<dynamic>("INSERT INTO Documents(Id) VALUES(@Id)", new { Id });

            }
            catch (Exception)
            {
                throw new Exception($"Was not possible inserting document with id: {Id} to the database.");
            }
        }

        private void DeleteDocument(int Id, SqlDataAccess sql)
        {
            try
            {
                sql.DeleteData<dynamic>("DELETE FROM Tokens WHERE DocumentId = @Id", new { Id });

            }
            catch (Exception)
            {
                throw new Exception($"Was not deleting the document with id: {Id} from the database.");
            }
        }
    }
}
