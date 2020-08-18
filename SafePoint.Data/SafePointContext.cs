using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SafePoint.Data.Entities;

namespace SafePoint.Data
{
    public class SafePointContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public SafePointContext(DbContextOptions options, IOptions<OperationalStoreOptions> operationalStoreOptions) 
            : base(options, operationalStoreOptions)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Shelter>().Property(o => o.Id)
                .HasIdentityOptions(startValue: 100).UseIdentityAlwaysColumn();
            builder.Entity<ShelterUsers>();
        }

        public DbSet<Shelter> Shelters { get; set; }

        public DbSet<ShelterUsers> ShelterUsers { get; set; }
    }
}
