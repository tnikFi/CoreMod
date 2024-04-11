using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Contexts;

public class ApplicationDbContext : DbContext
{
    public DbSet<GuildSettings> GuildSettings { get; set; }
    public DbSet<Moderation> Moderations { get; set; }
    public DbSet<PublicRole> PublicRoles { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}