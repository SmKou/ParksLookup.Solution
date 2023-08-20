using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ParksApi.Models;

public class ParksContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Park> Parks { get; set; }
    public DbSet<UserPark> UserParks { get; set; }

    public ParksContext(DbContextOptions options) : base(options) { }
}