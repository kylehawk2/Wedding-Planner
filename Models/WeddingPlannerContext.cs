using Microsoft.EntityFrameworkCore;

namespace Wedding_Planner.Models
{
    public class WeddingPlannerContext : DbContext
    {
        public WeddingPlannerContext(DbContextOptions<WeddingPlannerContext> options) : base(options) { } 
        public DbSet<User> Users {get;set;}
        public DbSet<Wedding> Weddings {get;set;}
        public DbSet<RSVP> RSVPs {get;set;}
    }
}