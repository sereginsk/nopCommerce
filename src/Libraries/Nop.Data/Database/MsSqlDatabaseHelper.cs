using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using Nop.Core.Infrastructure;
using Nop.Data.Data;

namespace Nop.Data.Database
{
    public class MsSqlDatabaseHelper : IDatabaseHelper
    {
        protected readonly INopFileProvider _fileProvider;

        #region Ctor

        public MsSqlDatabaseHelper(INopFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Check whether backups are supported
        /// </summary>
        protected void CheckBackupSupported()
        {
            if (!BackupSupported)
                throw new DataException("This database does not support backup");
        }

        protected SqlConnectionStringBuilder GetConnectionStringBuilder(string databaseName = null)
        {
            var connectionString = DataSettingsManager.LoadSettings().DataConnectionString;
            var builder = new SqlConnectionStringBuilder(connectionString);

            if (!string.IsNullOrEmpty(databaseName))
            {
                builder.InitialCatalog = databaseName;
            }

            return builder;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize database
        /// </summary>
        public virtual void InitializeDatabase()
        {
            //var context = EngineContext.Current.Resolve<IDbContext>();

            ////check some of table names to ensure that we have nopCommerce 2.00+ installed
            //var tableNamesToValidate = new List<string> { "Customer", "Discount", "Order", "Product", "ShoppingCartItem" };
            //var existingTableNames = context
            //    .QueryFromSql<StringQueryType>("SELECT table_name AS Value FROM INFORMATION_SCHEMA.TABLES WHERE table_type = 'BASE TABLE'")
            //    .Select(stringValue => stringValue.Value).ToList();
            //var createTables = !existingTableNames.Intersect(tableNamesToValidate, StringComparer.InvariantCultureIgnoreCase).Any();
            //if (!createTables)
            //    return;

            //var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();

            ////create tables
            ////EngineContext.Current.Resolve<IRelationalDatabaseCreator>().CreateTables();
            ////(context as DbContext).Database.EnsureCreated();
            //context.ExecuteSqlScript(context.GenerateCreateScript());

            ////create indexes
            //context.ExecuteSqlScriptFromFile(fileProvider.MapPath(NopDataDefaults.SqlServerIndexesFilePath));

            ////create stored procedures 
            //context.ExecuteSqlScriptFromFile(fileProvider.MapPath(NopDataDefaults.SqlServerStoredProceduresFilePath));
        }

        /// <summary>
        /// Creates a backup of the database
        /// </summary>
        public virtual void BackupDatabase(string fileName)
        {
            CheckBackupSupported();
            //var fileName = _fileProvider.Combine(GetBackupDirectoryPath(), $"database_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}_{CommonHelper.GenerateRandomDigitCode(10)}.{NopCommonDefaults.DbBackupFileExtension}");

            using (var connection = new SqlConnection(GetConnectionStringBuilder().ConnectionString))
            {
                var command = new SqlCommand($"BACKUP DATABASE [{connection.Database}] TO DISK = '{fileName}' WITH FORMAT", connection);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Restores the database from a backup
        /// </summary>
        /// <param name="backupFileName">The name of the backup file</param>
        public virtual void RestoreDatabase(string backupFileName)
        {
            CheckBackupSupported();
            using (var connection = new SqlConnection(GetConnectionStringBuilder().ConnectionString))
            {
                var commandText = string.Format(
                "DECLARE @ErrorMessage NVARCHAR(4000)\n" +
                "ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE\n" +
                "BEGIN TRY\n" +
                "RESTORE DATABASE [{0}] FROM DISK = '{1}' WITH REPLACE\n" +
                "END TRY\n" +
                "BEGIN CATCH\n" +
                "SET @ErrorMessage = ERROR_MESSAGE()\n" +
                "END CATCH\n" +
                "ALTER DATABASE [{0}] SET MULTI_USER WITH ROLLBACK IMMEDIATE\n" +
                "IF (@ErrorMessage is not NULL)\n" +
                "BEGIN\n" +
                "RAISERROR (@ErrorMessage, 16, 1)\n" +
                "END",
                connection.Database,
                backupFileName);

                var command = new SqlCommand(commandText, connection);
                command.Connection.Open();

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Re-index database tables
        /// </summary>
        public virtual void ReIndexTables()
        {
            using (var connection = new SqlConnection(GetConnectionStringBuilder().ConnectionString))
            {
                var commandText = $@"
                        DECLARE @TableName sysname 
                        DECLARE cur_reindex CURSOR FOR
                        SELECT table_name
                        FROM [{connection.Database}].information_schema.tables
                        WHERE table_type = 'base table'
                        OPEN cur_reindex
                        FETCH NEXT FROM cur_reindex INTO @TableName
                        WHILE @@FETCH_STATUS = 0
                            BEGIN
                          exec('ALTER INDEX ALL ON [' + @TableName + '] REBUILD')
                                FETCH NEXT FROM cur_reindex INTO @TableName
                            END
                        CLOSE cur_reindex
                        DEALLOCATE cur_reindex";

                var command = new SqlCommand(commandText, connection);
                command.Connection.Open();

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Checks if the specified database exists, returns true if database exists
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Returns true if the database exists.</returns>
        public virtual bool SqlServerDatabaseExists()
        {
            try
            {
                using (var connection = new SqlConnection(GetConnectionStringBuilder().ConnectionString))
                {
                    //just try to connect
                    connection.Open();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public virtual void CreateDatabase(string collation, int triesToConnect = 10)
        {
            if (SqlServerDatabaseExists())
                return;

            var builder = GetConnectionStringBuilder();

            //gets database name
            var databaseName = builder.InitialCatalog;

            //now create connection string to 'master' dabatase. It always exists.
            builder.InitialCatalog = "master";

            using (var connection = new SqlConnection(builder.ConnectionString))
            {
                var query = $"CREATE DATABASE [{databaseName}]";
                if (!string.IsNullOrWhiteSpace(collation))
                    query = $"{query} COLLATE {collation}";

                var command = new SqlCommand(query, connection);
                command.Connection.Open();

                command.ExecuteNonQuery();
            }

            //try connect
            if (triesToConnect <= 0)
                return;

            //sometimes on slow servers (hosting) there could be situations when database requires some time to be created.
            //but we have already started creation of tables and sample data.
            //as a result there is an exception thrown and the installation process cannot continue.
            //that's why we are in a cycle of "triesToConnect" times trying to connect to a database with a delay of one second.
            for (var i = 0; i <= triesToConnect; i++)
            {
                if (i == triesToConnect)
                    throw new Exception("Unable to connect to the new database. Please try one more time");

                if (!SqlServerDatabaseExists())
                    Thread.Sleep(1000);
                else
                    break;
            }
        }

        public virtual void SaveConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            var builder = new SqlConnectionStringBuilder(connectionString);

            DataSettingsManager.SaveSettings(new DataSettings()
            {
                DataProvider = DataProviderType.SqlServer,
                DataConnectionString = builder.ConnectionString
            }, _fileProvider);

            //reset cache
            DataSettingsManager.ResetCache();
        }

        public virtual void SaveConnectionString(INopConnectionString nopConnectionString)
        {
            if (nopConnectionString is null)
                throw new ArgumentNullException(nameof(nopConnectionString));

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = nopConnectionString.ServerName,
                InitialCatalog = nopConnectionString.DatabaseName,
                PersistSecurityInfo = false
            };

            if (!nopConnectionString.IntegratedSecurity)
            {
                builder.UserID = nopConnectionString.Username;
                builder.Password = nopConnectionString.Password;
            }

            SaveConnectionString(builder.ConnectionString);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this data provider supports backup
        /// </summary>
        public bool BackupSupported { get; } = true;

        #endregion
    }
}
