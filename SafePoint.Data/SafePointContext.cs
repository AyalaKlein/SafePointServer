using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SafePoint.Data
{
    public class SafePointContext : DbContext
    {
        public SafePointContext (DbContextOptions<SafePointContext> options)
            : base(options)
        {
        }

        public DbSet<Shelter> Shelter { get; set; }
    }
}
