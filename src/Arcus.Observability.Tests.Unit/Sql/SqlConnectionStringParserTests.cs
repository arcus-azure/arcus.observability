using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Unit.Sql
{
    public class SqlConnectionStringParserTests
    {
        private readonly ITestOutputHelper _outputWriter;
        private static readonly Faker Bogus = new Faker();

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlConnectionStringParserTests" /> class.
        /// </summary>
        public SqlConnectionStringParserTests(ITestOutputHelper outputWriter)
        {
            _outputWriter = outputWriter;
        }

        public static IEnumerable<object[]> ConnectionString =>
            Enumerable.Range(0, 100)
                      .Select(_ => new object[] { RandomConnectionString() });

        [Theory]
        [MemberData(nameof(ConnectionString))]
        public void ParseOriginal_WithGeneratedConnectionString_Succeeds(string connectionString)
        {
            _outputWriter.WriteLine("Parse: {0}", connectionString);
            var builder = new SqlConnectionStringBuilder(connectionString);
        }

        [Theory]
        [MemberData(nameof(ConnectionString))]
        public void Parse_WithGeneratedConnectionString_Succeeds(string connectionString)
        {
            // Arrange
            _outputWriter.WriteLine("Parsing: {0}", connectionString);
            var logger = new TestLogger();
            bool isSuccessful = Bogus.Random.Bool();
            DateTimeOffset startTime = Bogus.Date.RecentOffset();
            TimeSpan duration = Bogus.Date.Timespan();
            var dependencyId = Bogus.Random.Guid().ToString();
            var sqlCommand = "GET something FROM something";
            var operationName = "Get somethings";

            // Act
            logger.LogSqlDependencyWithConnectionString(connectionString, sqlCommand, operationName, isSuccessful, startTime, duration, dependencyId);

            // Assert
            (string dataSource, string initialCatalog) = DetermineSqlProperties(connectionString);
            Assert.StartsWith($"Sql {initialCatalog}/{operationName} {sqlCommand} named {dataSource}", logger.WrittenMessage);
        }

        private static (string dataSource, string initialCatalog) DetermineSqlProperties(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);

            string initialCatalog = builder.InitialCatalog;
            if (string.IsNullOrEmpty(initialCatalog))
            {
                initialCatalog = "<not-available>";
            }

            return (builder.DataSource, initialCatalog);
        }

        [Fact]
        public void Parse_WithoutSqlPropertyAssignment_Fails()
        {
            // Arrange
            var connectionString = Bogus.Lorem.Sentence();
            var logger = new TestLogger();
            bool isSuccessful = Bogus.Random.Bool();
            DateTimeOffset startTime = Bogus.Date.RecentOffset();
            TimeSpan duration = Bogus.Date.Timespan();
            var dependencyId = Bogus.Random.Guid().ToString();
            var sqlCommand = "GET something FROM something";
            var operationName = "Get somethings";

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependencyWithConnectionString(connectionString, sqlCommand, operationName, isSuccessful, startTime, duration, dependencyId));
        }

        private static string RandomConnectionString()
        {
            string applicationName = CreateOptionalProperty(Bogus.PickRandom("Application Name", "app"), CreateWordValue());
            string connectTimeout = CreateOptionalProperty(Bogus.PickRandom("Connect Timeout", "Connection Timeout"), Bogus.Random.Int(min: 0));
            
            string connectLifetime = CreateOptionalProperty("Connection Lifetime", Bogus.Random.Int(min: 0));
            string currentLanguage = CreateOptionalProperty("Current Language", CreateWordValue());
            string encrypt = CreateOptionalProperty("Encrypt", CreateBooleanValue());
            string enlist = CreateOptionalProperty("Enlist", CreateBooleanValue());
            string failOverPartner = CreateOptionalProperty("Failover Partner", CreateWordValue());
            string loadBalanceTimeout = CreateOptionalProperty("Load Balance Timeout", Bogus.Random.Int(min: 0));
            string multiActiveResultSets = CreateOptionalProperty("MultipleActiveResultSets", CreateBooleanValue());
            string maxPoolSize = CreateOptionalProperty("Max Pool Size", Bogus.Random.Int(min: 0));
            string minPoolSize = CreateOptionalProperty("Min Pool Size", Bogus.Random.Int(min: 0));
            string packetSize = CreateOptionalProperty("Packet Size", Bogus.Random.Int(min: 512, max: 32768));
            
            string persistSecurityInfo = CreateOptionalProperty("Persist Security Info", CreateBooleanValue());
            string pooling = CreateOptionalProperty("Pooling", CreateBooleanValue());
            string replication = CreateOptionalProperty("Replication", CreateBooleanValue());
            string transactionBinding = CreateOptionalProperty("Transaction Binding", Bogus.PickRandom("Implicit Unbind", "Explicit Unbind"));
            string trustServerCertificate = CreateOptionalProperty("Trust Server Certificate", CreateBooleanValue());
            string typeSystemVersion = CreateOptionalProperty("Type System Version", Bogus.PickRandom("SQL Server 2000", "SQL Server 2005", "SQL Server 2008"));
            string workstationId = CreateOptionalProperty("Workstation ID", CreateWordValue());

            string userAuthentication = CombineProperties(
                CreateProperty("User ID", CreateWordValue()),
                CreateProperty(Bogus.PickRandom("Password", "Pwd"), CreateWordValue()));
            
            string integratedSecurity = CombineProperties(
                CreateProperty(Bogus.PickRandom("Integrated Security", "Trusted_Connection"), CreateBooleanValue()),
                CreateOptionalProperty("User Instance", CreateBooleanValue()));

            string authentication = Bogus.PickRandom(userAuthentication, integratedSecurity);

            string dataSource = CreateProperty(
                Bogus.PickRandom("Data Source", "Server", "Addr", "Network Address"),
                Bogus.PickRandom(
                    CreateWordValue(),
                    Bogus.Internet.Ip(),
                    Bogus.System.FilePath()));

            string attachDbFilename = CreateProperty(
                Bogus.PickRandom("AttachDBFilename", "extended properties", "Initial File Name"),
                Bogus.System.FilePath());
            
            string initialCatalog = CreateProperty(
                Bogus.PickRandom("Initial Catalog", "Database"),
                CreateWordValue());

            string database = Bogus.PickRandom(attachDbFilename, initialCatalog);

            return CombineProperties(
                applicationName,
                connectTimeout,
                connectLifetime,
                currentLanguage,
                encrypt,
                enlist,
                failOverPartner,
                loadBalanceTimeout,
                multiActiveResultSets,
                maxPoolSize,
                minPoolSize,
                packetSize,
                persistSecurityInfo,
                pooling,
                replication,
                transactionBinding,
                trustServerCertificate,
                typeSystemVersion,
                workstationId,
                authentication,
                dataSource,
                database);
        }

        private static string CreateWordValue()
        {
            string sentence = Bogus.Random.AlphaNumeric(Bogus.Random.Int(2, 100));
            
            int index = Bogus.Random.Int(min: 1, max: sentence.Length - 1);
            sentence = sentence.Insert(index, "=");

            return Bogus.PickRandom(
                sentence,
                RandomAppendDoubleQuotes(sentence),
                RandomAppendSingleQuotes(sentence),
                RandomAppendBothQuotes(sentence));
        }

        private static string RandomAppendSingleQuotes(string value)
        {
            IEnumerable<int> indexes = Bogus.Make(2, () => Bogus.Random.Int(min: 1, max: value.Length - 1));
            return RandomAppendCharacter(value, "\'", indexes);
        }

        private static string RandomAppendDoubleQuotes(string value)
        {
            IEnumerable<int> indexes = Bogus.Make(2, () => Bogus.Random.Int(min: 1, max: value.Length - 1));
            return RandomAppendCharacter(value, "\"", indexes);
        }

        private static string RandomAppendBothQuotes(string value)
        {
            IList<int> firstQuoteIndexes = Bogus.Make(2, () => Bogus.Random.Int(min: 1, max: value.Length - 3));
            int firstQuoteIndexStart = firstQuoteIndexes[0];

            IEnumerable<int> secondQuoteIndexes = Bogus.Make(2, () => Bogus.Random.Int(min: firstQuoteIndexStart + 1, max: value.Length - 1));

            return Bogus.PickRandom(
                RandomAppendCharacter(RandomAppendCharacter(value, "\'", firstQuoteIndexes), "\"", secondQuoteIndexes),
                RandomAppendCharacter(RandomAppendCharacter(value, "\"", firstQuoteIndexes), "\'", secondQuoteIndexes));
        }

        private static string RandomAppendCharacter(string value, string character, IEnumerable<int> indexes)
        {
            Assert.All(indexes, i => value = value.Insert(i, character));
            return value;
        }

        private static string CreateBooleanValue()
        {
            return RandomCase(Bogus.PickRandom("true", "false", "yes", "no"));
        }

        private static string CombineProperties(params string[] properties)
        {
            return Bogus.Random.Shuffle(properties.Where(prop => prop != null))
                        .Aggregate("", (acc, prop) => RandomWhiteSpace() + prop + RandomWhiteSpace() + OneOrManySemicolon() + RandomWhiteSpace() + acc + RandomWhiteSpace());
        }

        private static string OneOrManySemicolon()
        {
            return string.Join("", Bogus.Make(Bogus.Random.Int(1, 10), () => ";"));
        }

        private static string CreateProperty(string key, object value)
        {
            return RandomCase(key) + RandomWhiteSpace() + "=" + RandomWhiteSpace() + value;
        }

        private static string CreateOptionalProperty(string key, object value)
        {
            string name = RandomCase(key).OrNull(Bogus);
            if (name is null)
            {
                return null;
            }

            return name + RandomWhiteSpace() + "=" + RandomWhiteSpace() + value;
        }

        private static string RandomCase(string item)
        {
            return string.Join("", item.ToCharArray().Select(ch =>
            {
                if (Bogus.Random.Bool())
                {
                    return char.ToUpper(ch);
                }

                return char.ToLower(ch);
            }));
        }

        private static string RandomWhiteSpace()
        {
            return string.Join("", Bogus.Make(Bogus.Random.Int(min: 0, 10), () =>
            {
                int code = Bogus.PickRandom(9, 10, 11, 12, 13, 32, 133, 160, 5760, 8192, 8193, 8194, 8195, 8196, 8197, 8198, 8199, 8200, 8201, 8202, 8232, 8233, 8239, 8287, 12288);
                return char.ConvertFromUtf32(code);
            }));
        }
    }
}
