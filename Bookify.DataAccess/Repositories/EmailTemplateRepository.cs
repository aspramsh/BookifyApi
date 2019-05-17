using Bookify.DataAccess.DbContexts;
using Bookify.DataAccess.Entities;
using Bookify.DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bookify.DataAccess.Repositories
{
    public class EmailTemplateRepository : Repository<EmailTemplate>, IEmailTemplateRepository
    {
        public EmailTemplateRepository(BookifyDbContext dbContext, ILoggerFactory loggerFactory)
            : base(dbContext, loggerFactory.CreateLogger<EmailTemplateRepository>())
        {

        }
    }
}
