using System;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace SueChef.Services
{
    public static class ConnectionStringResolver
    {
        public static string ResolveNpgsql(IConfiguration config)
        {
            // 1) Render/Heroku style
            var url = Environment.GetEnvironmentVariable("DATABASE_URL")
                   ?? Environment.GetEnvironmentVariable("RENDER_DATABASE_URL");
            if (!string.IsNullOrWhiteSpace(url))
                return FromDatabaseUrl(url);

            // 2) Build from env vars (default to suechef_test)
            var host = Environment.GetEnvironmentVariable("DB_HOST");
            var db   = Environment.GetEnvironmentVariable("DB_NAME");
            var user = Environment.GetEnvironmentVariable("DB_USER");
            var pass = Environment.GetEnvironmentVariable("DB_PASSWORD");
            var port = Environment.GetEnvironmentVariable("DB_PORT");

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Database = db,
                Username = user,
                Password = pass
            };
            if (int.TryParse(port, out var p)) builder.Port = p;

            return builder.ToString();
        }

        private static string FromDatabaseUrl(string dbUrl)
        {
            var uri = new Uri(dbUrl);
            var parts = uri.UserInfo.Split(':', 2);
            var b = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Username = Uri.UnescapeDataString(parts[0]),
                Password = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "",
                Database = uri.AbsolutePath.Trim('/'),
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true
            };
            return b.ToString();
        }
    }
}
