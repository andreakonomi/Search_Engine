using SearchEngine.Library.Dtos;
using System.Collections.Generic;

namespace SearchEngine.Library.DataAccess
{
    public interface IDocumentData
    {
        void CreateDocument(IDocumentDto document);
        List<int> SearchByTokensContent(string queryExpression);
    }
}