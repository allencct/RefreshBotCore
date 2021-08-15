using Microsoft.EntityFrameworkCore;
using RefreshBot.Models;

namespace RefreshBot.DataAccess
{
    public class EntityContext : DbContext
    {
        public EntityContext(DbContextOptions<EntityContext> options) : base(options)
        {
        }

        public DbSet<TargetPage> TargetPages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }

    }
}
