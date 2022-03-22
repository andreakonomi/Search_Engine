using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Cmd
{
    public static class Helper
    {
        public static string GetConnectionString(string name)
        {
            try
            {
                return ConfigurationManager.ConnectionStrings[name].ConnectionString;

            }
            catch (Exception)
            {
                throw new Exception($"Connection string {name} is either missing or wrong.");
            }        
        }

        /// <summary>
        /// Checks if the input given uses the standard query indicator
        /// at the beginning of it.
        public static bool IsQueryInput(this string input)
        {
            if (input.Length < 6)
            {
                return false;
            }

            return input.ToLower().StartsWith("query");
        }

        /// <summary>
        /// Checks if the input given uses the standard index indicator
        /// for inserting to the index
        public static bool IsIndexInput(this string input)
        {
            if (input.Length < 7)
            {
                return false;
            }

            return input.ToLower().StartsWith("index");
        }

        /// <summary>
        /// Checks if the input by the user is exit related
        /// </summary>
        public static bool IsExitInput(this string input)
        {
            return input.ToLower() == "exit";
        }

    }
}
