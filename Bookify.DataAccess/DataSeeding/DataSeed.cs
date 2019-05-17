using Bookify.DataAccess.DataSeeding.Interfaces;
using Bookify.DataAccess.Entities;
using Newtonsoft.Json;
using System.IO;

namespace Bookify.DataAccess.DataSeeding
{
    public class DataSeed : IDataSeed
    {
        public EmailTemplate[] GetEmailTemplates()
        {
            return JsonConvert.DeserializeObject<EmailTemplate[]>(File.ReadAllText("DataSeed\\EmailTemplates.json"));
        }
    }
}
