﻿using BookifyApi.DataAccess.DbContexts;
using BookifyApi.Infrastructure.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            services.AddDbContext<BookifyApiDbContext>(options => 
            options.UseNpgsql(Configuration.GetConnectionString("BookifyApiConnection")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<BookifyApiDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<BookifyApiDbContext>();

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

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
