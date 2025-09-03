namespace Arcus.Observability.Telemetry.Core.Sql
{
    /// <summary>
    /// Represents the result of the <see cref="SqlConnectionStringParser"/> when parsing a SQL connection string.
    /// </summary>
    internal class SqlConnectionStringParserResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlConnectionStringParserResult" /> class.
        /// </summary>
        /// <param name="dataSource">The data source SQL property parsed from the SQL connection string.</param>
        /// <param name="initialCatalog">The database name SQL property parsed from the SQL connection string.</param>
        internal SqlConnectionStringParserResult(string dataSource, string initialCatalog)
        {
            DataSource = dataSource ?? string.Empty;
            InitialCatalog = initialCatalog ?? string.Empty;
        }

        /// <summary>Gets or sets the name or network address of the instance of SQL Server to connect to.</summary>
        /// <value>The value of the <see cref="P:Microsoft.Data.SqlClient.SqlConnectionStringBuilder.DataSource" /> property, or <see langword="String.Empty" /> if none has been supplied.</value>
        /// <remarks>
        ///             <format type="text/markdown"><![CDATA[
        /// 
        /// ## Remarks
        ///  This property corresponds to the "Data Source", "server", "address", "addr", and "network address" keys within the connection string. Regardless of which of these values has been supplied within the supplied connection string, the connection string created by the `SqlConnectionStringBuilder` will use the well-known "Data Source" key.
        /// 
        /// The port number can be specified after the server name: `server=tcp:servername, portnumber`.
        /// 
        /// When specifying a local instance, always use (local). To force a protocol, add one of the following prefixes:`np:(local),  tcp:(local), lpc:(local)`.
        /// 
        /// You can also connect to a LocalDB database as follows: `server=(localdb)\\myInstance`. For more information about LocalDB, see [SqlClient Support for LocalDB](/sql/connect/ado-net/sql/sqlclient-support-localdb).
        /// **Data Source** must use the TCP format or the Named Pipes format. TCP format is as follows:
        /// 
        /// -   tcp:\<host name>\\<instance name\>
        /// -   tcp:\<host name>,\<TCP/IP port number>
        /// 
        /// The TCP format must start with the prefix "tcp:" and is followed by the database instance, as specified by a host name and an instance name. This format is not applicable when connecting to Azure SQL Database. TCP is automatically selected for connections to Azure SQL Database when no protocol is specified.
        /// 
        /// The host name MUST be specified in one of the following ways:
        /// -   NetBIOSName
        /// -   IPv4Address
        /// -   IPv6Address
        /// 
        /// The instance name is used to resolve to a particular TCP/IP port number on which a database instance is hosted. Alternatively, specifying a TCP/IP port number directly is also allowed. If both instance name and port number are not present, the default database instance is used.
        /// 
        /// The Named Pipes format is as follows:
        /// -   np:\\\\<host name\>\pipe\\<pipe name\>
        /// 
        /// The Named Pipes format MUST start with the prefix "np:" and is followed by a named pipe name.
        /// 
        /// The host name MUST be specified in one of the following ways:
        /// 
        /// -   NetBIOSName
        /// -   IPv4Address
        /// -   IPv6Address
        /// 
        /// The pipe name is used to identify the database instance to which the .NET application will connect.
        /// 
        /// If the value of the **Network** key is specified, the prefixes "tcp:" and "np:" should not be specified. **Note:**  You can force the use of TCP instead of shared memory, either by prefixing **tcp:** to the server name in the connection string, or by using **localhost**.
        /// 
        /// 
        /// ## Examples
        ///  The following example demonstrates that the <xref:Microsoft.Data.SqlClient.SqlConnectionStringBuilder> class converts synonyms for the "Data Source" connection string key into the well-known key:
        /// 
        ///  [!code-csharp[SqlConnectionStringBuilder_DataSource#1](~/../sqlclient/doc/samples/SqlConnectionStringBuilder_DataSource.cs#1)]
        /// 
        ///  ]]></format>
        ///             </remarks>
        /// <exception cref="T:System.ArgumentNullException">To set the value to null, use <see cref="F:System.DBNull.Value" />.</exception>
        internal string DataSource { get; }

        /// <summary>Gets or sets the name of the database associated with the connection.</summary>
        /// <value>The value of the <see cref="P:Microsoft.Data.SqlClient.SqlConnectionStringBuilder.InitialCatalog" /> property, or <see langword="String.Empty" /> if none has been supplied.</value>
        /// <remarks>
        ///             <format type="text/markdown"><![CDATA[
        /// 
        /// ## Remarks
        ///  This property corresponds to the "Initial Catalog" and "database" keys within the connection string.
        /// 
        ///  The database name can be 128 characters or less.
        /// 
        /// ## Examples
        ///  The following example creates a simple connection string and then uses the <xref:Microsoft.Data.SqlClient.SqlConnectionStringBuilder> class to add the name of the database to the connection string. The code displays the contents of the <xref:Microsoft.Data.SqlClient.SqlConnectionStringBuilder.InitialCatalog%2A> property, just to verify that the class was able to convert from the synonym ("Database") to the appropriate property value.
        /// 
        ///  [!code-csharp[SqlConnectionStringBuilder_InitialCatalog#1](~/../sqlclient/doc/samples/SqlConnectionStringBuilder_InitialCatalog.cs#1)]
        /// 
        ///  ]]></format>
        ///             </remarks>
        /// <exception cref="T:System.ArgumentNullException">To set the value to null, use <see cref="F:System.DBNull.Value" />.</exception>
        internal string InitialCatalog { get; }
    }
}