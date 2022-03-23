using SearchEngine.Library.Helpers;
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
            var query = CreateQueryForData(queryExpression, ref dynPars);



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

        private string CreateQueryForData(string query, ref DynamicParameters dynPars)
        {
            //a
            //a & b
            //a & (b | c)
            string finalQuery;
            var splitted = query.Split(" ");
            int count = splitted.Count();

            string parToPass = "";
            int index = 1;

            dynPars.Add("@par1", splitted[0]);

            // add check for each token if alphanumerical??
            finalQuery = $"SELECT DocumentId FROM Tokens WHERE Content = @par1";

            if(count == 3)
            {
                dynPars.Add("@par2", splitted[2]);
                string boolOperator = splitted[1];

                if (boolOperator == "|")
                {
                    finalQuery = AddOrClauseForNextArg(finalQuery, parToPass);
                }
                else if(boolOperator == "&")
                {
                    finalQuery = AddAndClauseForNextArg(finalQuery, parToPass);
                }
            }
            else
            {
                finalQuery = HandleMultipleParameters(query, ref dynPars);
            }

            return finalQuery;
        }

        private string HandleMultipleParameters(string initialQuery, ref DynamicParameters dynPars)
        {
            bool onlyAnds = !initialQuery.Contains('|');
            bool onlyOrs = !initialQuery.Contains('&');
            string finalQuery = "";

            // add check if longer splitted return error, not valid query

            if (onlyAnds || onlyOrs)
            {
                initialQuery = RemoveOperatorsSymbolsfromQuery(initialQuery);

                var splittedArray = initialQuery.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                string par1 = splittedArray[0];
                string par2 = splittedArray[1];
                string par3 = splittedArray[2];

                if (onlyAnds)
                {
                    finalQuery = GiveOnlyDoubleAndsQuery(ref dynPars, par1, par2, par3);
                }

                if (onlyOrs)
                {
                    finalQuery = GiveOnlyDoubleOrsQuery(ref dynPars, par1, par2, par3);
                }
            }
            else
            {
                finalQuery = CalculateMixedOperatorsQuery(ref dynPars, initialQuery);
            }

            return finalQuery;
        }

        private string CalculateMixedOperatorsQuery(ref DynamicParameters dynPars,string initialQuery)
        {
            string query = "";
            string par1 = null, par2 = null, par3 = null, par4 = null;
            string a, b, c;

            query = @"
                SELECT DocumentId from Tokens
                WHERE Content IN (@par1, @par2)
                INTERSECT
                SELECT DocumentId from Tokens
                WHERE Content IN (@par3, @par4)
                ";

            int indexOfAndOperator = initialQuery.IndexOf('&');
            int indexOfOrOperator = initialQuery.IndexOf('|');
            int indexOfOpeningBrace = initialQuery.IndexOf('(');
            int indexOfClosingBrace = initialQuery.IndexOf(')');

            initialQuery = RemoveOperatorsSymbolsfromQuery(initialQuery);
            var arrayArguments = initialQuery.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            a = arrayArguments[0];
            b = arrayArguments[1];
            c = arrayArguments[2];

            // mode (a & b) | c ; mode a | (b & c)
            if (indexOfAndOperator > indexOfOpeningBrace && indexOfAndOperator < indexOfClosingBrace)
            {
                if (indexOfClosingBrace < indexOfOrOperator)
                {
                    // mode (a & b) | c = (a | c) & (b | c)
                    par1 = a;
                    par2 = c;
                    par3 = b;
                    par4 = c;
                }
                else
                {
                    // mode a | (b & c) = (a | b) & (a | c)
                    par1 = a;
                    par2 = b;
                    par3 = a;
                    par4 = c;
                }
            }
            else
            {
                // mode (a | b) & c ; // mode a & (b | c)
                if (indexOfClosingBrace < indexOfAndOperator)
                {
                    // mode (a | b) & (c | d) ; d is null
                    par1 = a;
                    par2 = b;
                    par3 = c;
                }
                else
                {
                    // mode (a | b) & (b | c) ; b is null
                    par1 = a;
                    par3 = b;
                    par4 = c;
                }
            }

            dynPars.Add("@par1", par1);
            dynPars.Add("@par2", par2);
            dynPars.Add("@par3", par3);
            dynPars.Add("@par4", par4);

            return query;
        }

        private string RemoveOperatorsSymbolsfromQuery(string query)
        {
            return query.Replace("&", null).Replace("|", null).Replace("(", null).Replace(")", null);
        }

        private string GiveOnlyDoubleAndsQuery(ref DynamicParameters dynPars, string par1, string par2, string par3)
        {
            dynPars.Add("@par1", par1);
            dynPars.Add("@par2", par2);
            dynPars.Add("@par3", par3);

            return
                $@"
            select DocumentId from Tokens where Content = @par1
            intersect
            select DocumentId from Tokens where Content = @par2
            intersect
            select DocumentId from Tokens where Content = @par3
                ";
        }

        private string GiveOnlyDoubleOrsQuery(ref DynamicParameters dynPars, string par1, string par2, string par3)
        {
            dynPars.Add("@par1", par1);
            dynPars.Add("@par2", par2);
            dynPars.Add("@par3", par3);

            return
                $@"
            select DocumentId from Tokens 
            where Content IN (@par1, @par2, @par3)";
        }

        private string AddAndClauseForNextArg(string initialQuery, string parToPass)
        {
            return $"{initialQuery} \nINTERSECT\nSELECT DocumentId FROM Tokens WHERE Content = @par2";
        }

        private string AddOrClauseForNextArg(string initialQuery, string parToPass)
        {
            return $"{initialQuery} OR Content = @par2";
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
