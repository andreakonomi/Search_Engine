﻿using SearchEngine.Library.Helpers;
using SearchEngine.Library.Internal;
using SearchEngine.Library.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using SearchEngine.Library.Entities;
using Dapper;

namespace SearchEngine.Library.DataAccess
{
    public class DocumentData
    {
        private readonly string _connString;

        public DocumentData(string connectionString)
        {
            _connString = connectionString ?? throw new ArgumentNullException(nameof(connectionString), "Connection string can't be null");
        }

        public void CreateDocument(DocumentDto document)
        {
            bool valid = CheckIfDocumentIsValid(document);
            if (!valid)
            {
                throw new ArgumentException("The content provided is invalid, all tokens need to be alphanumerical!");
            }

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

        private bool CheckIfDocumentIsValid(DocumentDto document)
        {
            if (document is null)
            {
                return false;
            }

            return CheckIfTokensAreValid(document.Tokens);
        }

        /// <summary>
        /// Checks if tokens collection is valid.
        /// </summary>
        private bool CheckIfTokensAreValid(ICollection<TokenDto> tokens)
        {
            return tokens.All(x => CheckTokenContentIfValid(x.Content));
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
            DynamicParameters dynPars = new();
            var query = CreateQueryForData(queryExpression, dynPars);



            //queryExpression = FormatFieldValuesForSqlQuery(queryExpression);

            //queryExpression = queryExpression
            //    .Replace("|", " or Content =", StringComparison.OrdinalIgnoreCase)
            //    .Replace("&", " and Content =", StringComparison.OrdinalIgnoreCase)
            //    .Replace("(", "(Content = ", StringComparison.OrdinalIgnoreCase);

            //string query = "SELECT DISTINCT DocumentId from Tokens WHERE ";

            //if (queryExpression.StartsWith('('))
            //{
            //    query += queryExpression;
            //}
            //else
            //{
            //    query += "Content = ";
            //    query += queryExpression;
            //}

            //var newQuery = CheckQueryForAndCondition(query);

            var result = sql.LoadData<int, dynamic>(query, dynPars);
            return result;
        }

        private string CreateQueryForData(string query, DynamicParameters dynPars)
        {
            //a
            //a & b
            //a & (b | c)
            string finalQuery;
            var splitted = query.Split(" ");
            int count = splitted.Count();

            string parToPass = "";
            int index = 1;

            parToPass = GiveNewParName(ref index);
            dynPars.Add(parToPass, splitted[0]);

            // add check for each token if alphanumerical??
            finalQuery = $"SELECT DocumentId FROM Tokens WHERE Content = {parToPass}";

            if(count == 3)
            {
                parToPass = GiveNewParName(ref index);
                dynPars.Add(parToPass, splitted[2]);
                string boolOperator = splitted[1];

                if (boolOperator == "|")
                {
                    finalQuery += $" OR Content = {parToPass}";
                }
                else if(boolOperator == "&")
                {
                    finalQuery += $"\nINTERSECT\nSELECT DocumentId FROM Tokens WHERE Content = {parToPass}";
                }
            }
            else
            {
                // triple query
            }
            {

            }

            return finalQuery;
        }

        private string GiveNewParName(ref int index)
        {
            return $"@par{index++}";
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

        /// <summary>
        /// Inserts the tokens of the provided document
        /// </summary>
        private void InsertTokensForDocument(DocumentDto document, SqlDataAccess sql)
        {
            string query = "INSERT INTO Tokens(Content, DocumentId) VALUES(@Content, @docId)";
            int Id = document.Id;

            try
            {
                sql.SaveTokens(query, document.Tokens, Id);
            }
            catch (Exception)
            {
                throw new Exception($"Was not possible inserting the tokens for the document with id: {Id} to the database.");
            }
        }

        /// <summary>
        /// Inserts the document to the database.
        /// </summary>
        /// <param name="Id">Id of the document</param>
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

        /// <summary>
        /// Deletes the specified document given by the id provided.
        /// </summary>
        /// <param name="Id">Document id to delete</param>
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
