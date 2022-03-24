using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Library.Dtos
{
    /// <summary>
    /// Representation of a document to be created.
    /// </summary>
    public class DocumentDto : IDocumentDto
    {
        public int Id { get; set; }

        /// <summary>
        /// Collection of Tokens linked to the document
        /// </summary>
        public ICollection<ITokenDto> Tokens { get; set; }
            = new List<ITokenDto>();
    }
}
