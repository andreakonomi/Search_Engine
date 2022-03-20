using Microsoft.Extensions.Configuration;
using SearchEngine.Library.DataAccess;
using SearchEngine.Library.Models;
using System;
using System.Collections.Generic;

namespace SearchEngine.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Insert a token: ");

            string token = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("Empty tokens are not accepted!");
            }

            // throws nullrefExc if not matching 
            string connString = Helper.GetConnectionString("Demo_Db");

            var docData = new DocumentData(connString);
            docData.CreateDocument(CreateDocument(token));
        }

        static DocumentForCreationModel CreateDocument(string input)
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
            List<TokenForCreationModel> tokens = new();
            for (int i = 1; i < token.Length; i++)
            {
                tokens.Add(new TokenForCreationModel { Content = token[i] });
            }

            return tokens;
        }
    }
}
