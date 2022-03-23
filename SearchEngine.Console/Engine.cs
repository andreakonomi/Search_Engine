using SearchEngine.Library.DataAccess;
using SearchEngine.Library.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Cmd
{
    public static class Engine
    {
        private static Dictionary<string, List<int>> DocumentsIdCache = new();

        public static void Run()
        {
            string response = "";
            string input = PromptUser();

            while (!input.IsExitInput())
            {
                response = ProcessInput(input);

                Console.WriteLine(response);
                input = PromptUser();
            }

            Console.WriteLine("Your data is safe with us, see you again!");
        }

        private static string ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "An empty input is not valid.";
            }

            if (input.IsIndexInput())
            {
                input = RemoveInputPrefix(input);
                return InsertInput(input);
            }

            if (input.IsQueryInput())
            {
                input = RemoveInputPrefix(input);
                return SearchData(input);
            }

            return "No valid input has been provided.";
        }

        /// <summary>
        /// Removes the index or query prefix given by the input
        /// </summary>
        private static string RemoveInputPrefix(string input)
        {
            return input.Remove(0, 5);
        }

        private static string PromptUser()
        {
            Console.Write("Press 'Exit' or insert a token: ");
            return Console.ReadLine();
        }

        private static string InsertInput(string input)
        {
            try
            {
                string connString = Helper.GetConnectionString("Demo_Db");

                DocumentDto document = ParseDocument(input);
                var docData = new DocumentData(connString);

                docData.CreateDocument(document);

                // cache no longer validn after input
                DocumentsIdCache.Clear();

                return $"index ok {document.Id}";
            }
            catch (Exception ex)
            {
                return $"index error {ex.Message}";
            }
        }

        private static string SearchData(string query)
        {
            string valuesFound;
            List<int> response;

            try
            {
                query = query.Trim();

                response = CheckForCachedResponse(query);
                if (response is null)
                {
                    string connString = Helper.GetConnectionString("Demo_Db");

                    var docData = new DocumentData(connString);

                    response = docData.SearchByTokens(query);
                    if (response is null)
                    {
                        return "query error Invalid query!";
                    }

                    AddToCache(query, response);
                }

                valuesFound = ConvertListToString(response);
            }
            catch (Exception ex)
            {
                return $"query error {ex.Message}";
            }

            return $"query results {valuesFound}";
        }

        private static List<int> CheckForCachedResponse(string filterExpression)
        {
            DocumentsIdCache.TryGetValue(filterExpression, out List<int> idsFound);

            return idsFound;
        }

        private static void AddToCache(string queryExpression, List<int> ids)
        {
            if (DocumentsIdCache.Count == 100)
            {
                DocumentsIdCache.Clear();
            }

            DocumentsIdCache.Add(queryExpression, ids);
        }

        private static string ConvertListToString(List<int> list)
        {
            StringBuilder builder = new();

            foreach (var item in list)
            {
                builder.Append($" {item}");
            }

            return builder.ToString();
        }

        static DocumentDto ParseDocument(string input)
        {
            var docCreation = new DocumentDto();
            var tokensArray = input.Split(' ');

            // index 0 is empty string from method
            bool ok = int.TryParse(tokensArray[1], out int id);

            if (!ok)
            {
                throw new ArgumentException("The id provided is not in the correct format. It needs to be numeric.");
            }

            docCreation.Id = id;
            docCreation.Tokens = ParseContent(tokensArray);

            return docCreation;
        }

        /// <summary>
        /// Parses the string array provided for content of tokens to a TokenDtoCollection.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        static ICollection<TokenDto> ParseContent(string[] tokens)
        {
            string tokenContent = "";
            bool valid = true;

            List<TokenDto> tokensList = new();
            for (int i = 2; i < tokens.Length; i++)
            {
                tokenContent = tokens[i];
                valid = CheckTokenIfValid(tokenContent);

                if (!valid)
                {
                    throw new FormatException($"Incorrect input! The value {tokenContent} is not alphanumerical.");
                }

                tokensList.Add(new TokenDto { Content = tokenContent });
            }

            return tokensList;
        }

        /// <summary>
        /// Checks if the format of the token contact is valid.
        /// </summary>
        /// <param name="token">Content to check.</param>
        /// <returns>true if content is ok, false if not.</returns>
        static bool CheckTokenIfValid(string token)
        {
            return token.All(x => char.IsLetterOrDigit(x));
        }

    }

}
