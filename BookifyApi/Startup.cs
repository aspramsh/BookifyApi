using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Bookify.Business;
using Bookify.Business.Mapping;
using Bookify.Business.Services;
using Bookify.Business.Services.Interfaces;
using Bookify.Business.Settings;
using Bookify.DataAccess.DataSeeding;
using Bookify.DataAccess.DataSeeding.Interfaces;
using Bookify.DataAccess.DbContexts;
using Bookify.DataAccess.Repositories;
using Bookify.DataAccess.Repositories.Interfaces;
using Bookify.Infrastructure.Enums;
using Bookify.Infrastructure.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using System.Linq;

namespace BookifyApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddMvcCore()
                .AddAuthorization(
                    options =>
                    {
                        options.AddPolicy(RolesEnum.Admin.ToString(), policyAdmin =>
                        {
                            policyAdmin.RequireRole(RolesEnum.Admin.ToString());
                        });
                        options.AddPolicy(RolesEnum.User.ToString(), policyUser =>
                        {
                            policyUser.RequireRole(RolesEnum.User.ToString());
                        });
                    })
                .AddJsonFormatters();

            services.AddDirectoryBrowser();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddSingleton<IDataSeed>(new DataSeed());

            services.AddDbContext<BookifyDbContext>(options => 
            options.UseNpgsql(Configuration.GetConnectionString("BookifyConnection")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<BookifyDbContext>()
                .AddDefaultTokenProviders();

            #region Services Injection
            services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();

            services.AddScoped<IUserService, UserService>();
            #endregion

            // Add email sender
            services.AddTransient<IEmailSender, EmailSender>(i =>
                new EmailSender(
                    Configuration["EmailSender:Host"],
                    Configuration.GetValue<int>("EmailSender:Port"),
                    Configuration.GetValue<bool>("EmailSender:EnableSSL"),
                    Configuration["EmailSender:UserName"],
                    Configuration["EmailSender:Password"]
                )
            );

            services.Configure<EmailVerificationSettings>(Configuration.GetSection("EmailVerificationSettings"));

            services.AddMvc();
            services.AddAutoMapper(typeof(Startup));

            #region mapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();
                cfg.AddProfile(new MappingProfile());
            });

            var mapper = config.CreateMapper();
            services.AddSingleton(mapper);
            #endregion

            #region swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Bookify API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme { In = "header", Description = "Please enter JWT with Bearer into field", Name = "Authorization", Type = "apiKey" });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> {
                    { "Bearer", Enumerable.Empty<string>() },
                });
            });
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
                              IHostingEnvironment env,
                              ILoggerFactory loggerFactory,
                              IApplicationLifetime appLifetime)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<BookifyDbContext>();

                context.Database.EnsureCreated();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Add Postgres support later
            #region Serilog
            var loggerConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            loggerFactory.AddSerilog(loggerConfig);

            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            #endregion

            app.UseCors("CorsPolicy");

            #region swagger
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bookify API V1");
            });

            #endregion

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}