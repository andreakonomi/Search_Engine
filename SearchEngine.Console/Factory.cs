using SearchEngine.Library.DataAccess;
using SearchEngine.Library.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Cmd
{
    internal static class Factory
    {
        public static IDocumentData CreateDocumentData(string connectionString)
        {
            return new DocumentData(connectionString);
        }

        public static IDocumentDto CreateDocumentDto()
        {
            return new DocumentDto();
        }

        public static ITokenDto CreateToken(string content)
        {
            return new TokenDto { Content = content };
        }

        public static ICollection<ITokenDto> CreateTokensCollection()
        {
            return new List<ITokenDto>();
        }
    }
}
