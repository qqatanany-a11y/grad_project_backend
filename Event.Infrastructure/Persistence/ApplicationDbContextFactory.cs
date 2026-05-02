using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace events.Infrastructure.Persistence
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var candidatePaths = new[]
            {
                Directory.GetCurrentDirectory(),
                Path.Combine(Directory.GetCurrentDirectory(), "events"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "events")
            }
            .Select(Path.GetFullPath)
            .Distinct()
            .ToList();

            var configurationBasePath = candidatePaths.FirstOrDefault(path =>
                File.Exists(Path.Combine(path, "appsettings.json")));

            if (configurationBasePath == null)
                throw new InvalidOperationException("Could not locate appsettings.json for design-time DbContext creation.");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(configurationBasePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("The DefaultConnection string is missing.");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
