using Bookify.DataAccess.Entities;

namespace Bookify.DataAccess.DataSeeding.Interfaces
{
    public interface IDataSeed
    {
        EmailTemplate[] GetEmailTemplates();
    }
}
