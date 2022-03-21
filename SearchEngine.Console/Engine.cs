using SearchEngine.Library.DataAccess;
using SearchEngine.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Cmd
{
    public static class Engine
    {
        public static void Run()
        {
            string input;
            input = PromptUser();

            while (input.ToLower() != "Exit".ToLower())
            {
                Console.WriteLine(ProccessInput(input));
                input = PromptUser();
            }

            Console.WriteLine("Your data is safe with us, see you again!");
        }

        private static string PromptUser()
        {
            Console.Write("Press 'Exit' or insert a token: ");
            return Console.ReadLine();
        }

        private static string ProccessInput(string input)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    return "You didn't input anything.";
                }

                // move this
                string connString = Helper.GetConnectionString("Demo_Db");

                var document = ParseDocument(input);
                var docData = new DocumentData(connString);

                docData.CreateDocument(document);

                return $"index ok {document.Id}";
            }
            catch (Exception ex)
            {
                return $"index error {ex.Message}";
            }
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
