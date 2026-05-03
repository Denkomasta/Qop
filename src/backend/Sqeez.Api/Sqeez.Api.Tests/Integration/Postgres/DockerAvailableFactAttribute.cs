namespace Sqeez.Api.Tests.Integration.Postgres
{
    public sealed class DockerAvailableFactAttribute : FactAttribute
    {
        public DockerAvailableFactAttribute()
        {
            if (!string.Equals(
                    Environment.GetEnvironmentVariable("SQEEZ_RUN_POSTGRES_TESTS"),
                    "true",
                    StringComparison.OrdinalIgnoreCase))
            {
                Skip = "Requires Docker. Set SQEEZ_RUN_POSTGRES_TESTS=true to run PostgreSQL Testcontainers tests.";
            }
        }
    }
}
