using Microsoft.Extensions.Configuration;
using Npgsql;

namespace events.Infrastructure.Persistence
{
    public static class DatabaseConnectionStringResolver
    {
        public static string Resolve(IConfiguration configuration)
        {
            var databaseUrl = configuration["DATABASE_URL"];

            if (!string.IsNullOrWhiteSpace(databaseUrl))
            {
                return BuildConnectionStringFromDatabaseUrl(databaseUrl);
            }

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection is not configured. Set ConnectionStrings:DefaultConnection locally or DATABASE_URL in production.");
            }

            return connectionString;
        }

        private static string BuildConnectionStringFromDatabaseUrl(string databaseUrl)
        {
            if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var databaseUri) ||
                !IsPostgresScheme(databaseUri.Scheme))
            {
                return databaseUrl;
            }

            var credentials = databaseUri.UserInfo.Split(':', 2, StringSplitOptions.TrimEntries);
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.IsDefaultPort ? 5432 : databaseUri.Port,
                Database = databaseUri.AbsolutePath.Trim('/'),
                Username = credentials.Length > 0 ? DecodeValue(credentials[0]) : string.Empty,
                Password = credentials.Length > 1 ? DecodeValue(credentials[1]) : string.Empty
            };

            var queryParameters = ParseQueryParameters(databaseUri.Query);

            if (TryGetQueryValue(queryParameters, out var sslMode, "sslmode", "ssl mode") &&
                Enum.TryParse<SslMode>(NormalizeEnumValue(sslMode), ignoreCase: true, out var parsedSslMode))
            {
                connectionStringBuilder.SslMode = parsedSslMode;
            }

            return connectionStringBuilder.ConnectionString;
        }

        private static bool IsPostgresScheme(string scheme) =>
            string.Equals(scheme, "postgres", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(scheme, "postgresql", StringComparison.OrdinalIgnoreCase);

        private static Dictionary<string, string> ParseQueryParameters(string query)
        {
            var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(query))
            {
                return parameters;
            }

            var segments = query.TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var segment in segments)
            {
                var parts = segment.Split('=', 2, StringSplitOptions.TrimEntries);
                var key = DecodeValue(parts[0]);
                var value = parts.Length > 1 ? DecodeValue(parts[1]) : string.Empty;

                parameters[key] = value;
            }

            return parameters;
        }

        private static bool TryGetQueryValue(
            IReadOnlyDictionary<string, string> queryParameters,
            out string value,
            params string[] keys)
        {
            foreach (var key in keys)
            {
                if (queryParameters.TryGetValue(key, out var candidate) &&
                    !string.IsNullOrWhiteSpace(candidate))
                {
                    value = candidate;
                    return true;
                }
            }

            value = string.Empty;
            return false;
        }

        private static string NormalizeEnumValue(string value) =>
            value.Replace("-", string.Empty)
                .Replace("_", string.Empty)
                .Replace(" ", string.Empty);

        private static string DecodeValue(string value) =>
            Uri.UnescapeDataString(value.Replace("+", " ", StringComparison.Ordinal));
    }
}
