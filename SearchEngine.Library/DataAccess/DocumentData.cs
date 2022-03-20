using Microsoft.Extensions.Configuration;
using SearchEngine.Library.Internal;
using SearchEngine.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Library.DataAccess
{
    public class DocumentData
    {
        private readonly IConfiguration _config;

        public DocumentData(IConfiguration config)
        {
            _config = config;
        }

        public void CreateDocument(DocumentForCreationModel document, string dbKeyName = "")
        {
            // more validation

            if (document is null)
            {
                throw new ArgumentNullException(nameof(document), "Invalid document passed in.");
            }

            //// flow
            // 1. does document exist?
            // 2. yes - delete tokens for old instance
            //    no - insert document only
            // 3. insert tokens for the document

            SqlDataAccess sql = new(_config);
            DocumentDbModel doc = sql.LoadData<DocumentDbModel, dynamic>("dbo.GetDocument", new { document.Id }, dbKeyName).FirstOrDefault();

            if (doc is null)
            {
                sql.SaveData<dynamic>("dbo.InsertDocument", new { document.Id }, dbKeyName);
            }
            else
            {
                // delete tokens for old instance
            }
            
            sql.SaveData<dynamic>("dbo.InsertTokensForDocument", new { document.Id, TokensList = document.Tokens }, dbKeyName);
        }
    }
}
