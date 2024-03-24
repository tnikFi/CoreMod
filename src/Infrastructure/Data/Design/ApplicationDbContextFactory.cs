using System.Xml.Linq;
using Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data.Design;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Get the configuration base path by the Web project's location
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Web");
        var secretId = GetUserSecretsId(basePath, "Web");

        // Get the db connection string from the Web project's appsettings.json
        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json");
        
        if (secretId is not null)
            builder.AddUserSecrets(secretId);
        
        var configuration = builder.Build();

        // Configure the DbContext
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    /// <summary>
    ///     Get the UserSecretsId from the given project path and project name.
    ///     Needed to get the UserSecrets from the project without including a reference to it to avoid circular
    ///     dependencies.
    /// </summary>
    /// <param name="projectPath"></param>
    /// <param name="projectName"></param>
    /// <returns></returns>
    private static string? GetUserSecretsId(string projectPath, string projectName)
    {
        var projectFilePath = Path.Combine(projectPath, $"{projectName}.csproj");
        var projectFile = File.ReadAllText(projectFilePath);
        var csproj = XDocument.Parse(projectFile);

        // Get the UserSecretsId from the project file
        var userSecretsId = csproj.Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "UserSecretsId")?.Value;

        return userSecretsId;
    }
}