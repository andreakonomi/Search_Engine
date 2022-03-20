using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Library.Models
{
    /// <summary>
    /// Representation of a document to be created.
    /// </summary>
    public class DocumentForCreationModel
    {
        public int Id { get; set; }

        /// <summary>
        /// Collection of Tokens linked to the document
        /// </summary>
        public ICollection<TokenForCreationModel> Tokens { get; set; }
            = new List<TokenForCreationModel>();
    }
}
