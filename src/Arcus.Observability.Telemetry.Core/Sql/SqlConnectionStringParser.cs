using System;
using System.Collections.Generic;
using System.Linq;

namespace Arcus.Observability.Telemetry.Core.Sql
{
    /// <summary>
    /// Represents the instance to parse the SQL connection string to a strongly-typed <see cref="SqlConnectionStringParserResult"/>.
    /// </summary>
    internal static class SqlConnectionStringParser
    {
        private enum SqlProperties { DataSource, InitialCatalog }

        /// <summary>
        /// Parses the incoming SQL <paramref name="connectionString"/> to a strongly-typed result of SQL properties.
        /// </summary>
        /// <param name="connectionString">The SQL connection string that needs to be parsed into separate SQL properties.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="connectionString"/> is blank.</exception>
        internal static SqlConnectionStringParserResult Parse(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            string[] parts =
                connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                .Select(part => part.Trim())
                                .ToArray();

            string dataSource = FindProperty(parts, SqlProperties.DataSource);
            string initialCatalog = FindProperty(parts, SqlProperties.InitialCatalog);

            return new SqlConnectionStringParserResult(dataSource, initialCatalog);
        }

        private static string FindProperty(IEnumerable<string> parts, SqlProperties propertyName)
        {
            string[] aliases = GetSqlPropertyAliases(propertyName);
            string potentialFind = parts.FirstOrDefault(part =>
            {
                return aliases.Any(alias => part.StartsWith(alias, StringComparison.OrdinalIgnoreCase));
            });

            if (potentialFind is null)
            {
                return null;
            }

            return string.Join("", potentialFind?.SkipWhile(ch => ch != '=').Skip(1)).Trim();
        }

        private static string[] GetSqlPropertyAliases(SqlProperties propertyName)
        {
            switch (propertyName)
            {
                case SqlProperties.DataSource: return new[] { "Data Source", "Server", "Addr", "Address", "Network Address" };
                case SqlProperties.InitialCatalog: return new[] { "Initial Catalog", "Database" };
                default:
                    throw new ArgumentOutOfRangeException(nameof(propertyName), propertyName, "Unknown keyword with no known SQL property aliases");
            }
        }
    }
}
