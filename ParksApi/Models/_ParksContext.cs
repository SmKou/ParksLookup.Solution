using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ParksApi.Models;

public class ParksContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Park> Parks { get; set; }
    public DbSet<VisitorCenter> Centers { get; set; }

    public ParksContext(DbContextOptions options) : base(options) { }
}