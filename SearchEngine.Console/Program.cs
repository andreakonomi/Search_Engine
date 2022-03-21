using Microsoft.Extensions.Configuration;
using SearchEngine.Library.DataAccess;
using SearchEngine.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchEngine.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Insert a token: ");

            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("You didn't input anything.");
            }

            // throws nullrefExc if not matching 
            string connString = Helper.GetConnectionString("Demo_Db");

            var document = ParseDocument(input);
            var docData = new DocumentData(connString);

            docData.CreateDocument(document);
        }

        static DocumentForCreationModel ParseDocument(string input)
        {
            var docCreation = new DocumentForCreationModel();
            var tokensArray = input.Split(' ');

            bool ok = int.TryParse(tokensArray[0], out int id);

            docCreation.Id = id;
            docCreation.Tokens = ParseContent(tokensArray);

            return docCreation;
        }

        static ICollection<TokenForCreationModel> ParseContent(string[] token)
        {
            string tokenContent = "";
            bool valid = true;

            List<TokenForCreationModel> tokens = new();
            for (int i = 1; i < token.Length; i++)
            {
                tokenContent = token[i];
                valid = CheckTokenIfValid(tokenContent);

                if (!valid)
                {
                    throw new FormatException("All tokens need to be alphanumerical.");
                }

                tokens.Add(new TokenForCreationModel { Content = token[i] });
            }

            return tokens;
        }

        static bool CheckTokenIfValid(string token)
        {
            return token.All(x => char.IsLetterOrDigit(x));
        }
    }
}
