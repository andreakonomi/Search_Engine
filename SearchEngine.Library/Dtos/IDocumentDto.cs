using System.Collections.Generic;

namespace SearchEngine.Library.Dtos
{
    public interface IDocumentDto
    {
        int Id { get; set; }
        ICollection<ITokenDto> Tokens { get; set; }
    }
}