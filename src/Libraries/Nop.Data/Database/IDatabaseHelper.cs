using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Database
{
    public interface IDatabaseHelper
    {
        void CreateDatabase(string collation, int triesToConnect = 10);

        /// <summary>
        /// Initialize database
        /// </summary>
        void InitializeDatabase();

        /// <summary>
        /// Checks if the specified database exists, returns true if database exists
        /// </summary>
        /// <returns>Returns true if the database exists.</returns>
        bool SqlServerDatabaseExists();

        /// <summary>
        /// Creates a backup of the database
        /// </summary>
        void BackupDatabase(string fileName);

        /// <summary>
        /// Restores the database from a backup
        /// </summary>
        /// <param name="backupFileName">The name of the backup file</param>
        void RestoreDatabase(string backupFileName);

        /// <summary>
        /// Re-index database tables
        /// </summary>
        void ReIndexTables();

        void SaveConnectionString(string connectionString);

        void SaveConnectionString(INopConnectionString nopConnectionString);
    }
}
