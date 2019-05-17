using AutoMapper;
using Bookify.Business.Models;
using Bookify.Business.Services.Interfaces;
using Bookify.DataAccess.Entities;
using Bookify.DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bookify.Business.Services
{
    public class EmailTemplateService : Service<EmailTemplate, EmailTemplateModel>, IEmailTemplateService
    {
        public EmailTemplateService(IMapper mapper, ILoggerFactory loggerFactory, IEmailTemplateRepository emailTemplateRepository)
            : base(mapper, loggerFactory.CreateLogger<EmailTemplateService>(), emailTemplateRepository)
        {
        }
    }
}
