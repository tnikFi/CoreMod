using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Contexts;

public class ApplicationDbContext : DbContext
{
    public DbSet<GuildSettings> GuildSettings { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GuildSettings>();
    }
}