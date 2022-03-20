using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Library.Models
{
    class TokenDbModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int DocumentId { get; set; }
    }
}
