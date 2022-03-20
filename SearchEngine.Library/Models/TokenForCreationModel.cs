using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Library.Models
{
    /// <summary>
    /// Representation of a token to be created.
    /// </summary>
    public class TokenForCreationModel
    {
        /// <summary>
        /// An alphanumeric string that holds the value of the token.
        /// </summary>
        public string Content { get; set; }
    }
}
