using Bookify.DataAccess.DataSeeding.Interfaces;
using Bookify.DataAccess.DbContexts.Interfaces;
using Bookify.DataAccess.Entities;
using Bookify.DataAccess.Extensions;
using Bookify.Infrastructure.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bookify.DataAccess.DbContexts
{
    public class BookifyDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>, IDbContext
    {
        private readonly IDataSeed _dataSeed;

        public BookifyDbContext(DbContextOptions<BookifyDbContext> options,
                                IDataSeed dataSeed) 
            : base(options)
        {
            _dataSeed = dataSeed;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailTemplate>().HasData(_dataSeed.GetEmailTemplates());

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IdentityUser>().ToTable("Users");

            modelBuilder.Entity<IdentityRole>().ToTable("Roles").HasData(
                new IdentityRole { Id = "ed7e02e1-578c-423e-8fe7-2a4512e85242", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "7fd7460e-1d81-4ccb-8500-d5b0579037f2", Name = "User", NormalizedName = "USER" });
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            modelBuilder.NamesToSnakeCase();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var environmentName = EnvironmentHelper.GetCurrentEnvironmentVariableName();
            var isSensitiveDataLogging = environmentName != EnvironmentName.Production;
            optionsBuilder.EnableSensitiveDataLogging(isSensitiveDataLogging);
        }

        public string FullName()
        {
            return nameof(BookifyDbContext);
        }

        public new DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }

        #region DbSets
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        #endregion
    }
}
