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

    }
}
