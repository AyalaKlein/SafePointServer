using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SafePoint.Data.Entities;

namespace SafePoint.Data
{
    public class SafePointContext : DbContext
    {
        public SafePointContext (DbContextOptions<SafePointContext> options)
            : base(options)
        {
        }

        public DbSet<Shelter> Shelters { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }
    }
}
