using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sqeez.Api.Data;

public class SqeezDbContextFactory : IDesignTimeDbContextFactory<SqeezDbContext>
{
    public SqeezDbContext CreateDbContext(string[] args)
    {
        DotNetEnv.Env.Load();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                               ?? "Host=dummy;Database=dummy;";

        var optionsBuilder = new DbContextOptionsBuilder<SqeezDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new SqeezDbContext(optionsBuilder.Options);
    }
}