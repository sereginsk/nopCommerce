using System;
using System.Data;
using System.Linq;
using LinqToDB;
using LinqToDB.Data;
using Nop.Core;

namespace Nop.Data.DataProviders
{
    public partial class MsSqlDataProvider : IDataProvider
    {

        private readonly string _connectionString;
        public MsSqlDataProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region Utilities

        /// <summary>
        /// Check whether backups are supported
        /// </summary>
        protected void CheckBackupSupported()
        {
            if (!BackupSupported)
                throw new DataException("This database does not support backup");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get a support database parameter object (used by stored procedures)
        /// </summary>
        /// <returns>Parameter</returns>
        public DataParameter GetParameter()
        {
            return new DataParameter();
        }

        /// <summary>
        /// Get the current identity value
        /// </summary>
        /// <typeparam name="T">Entity</typeparam>
        /// <returns>Integer identity; null if cannot get the result</returns>
        public virtual int? GetTableIdent<T>() where T : BaseEntity
        {
            using (var dataConnection = new DataConnection(ProviderName.SqlServer, _connectionString))
            {
                var tableName = dataConnection.GetTable<T>().TableName;

                var result = dataConnection.Query<decimal?>($"SELECT IDENT_CURRENT('[{tableName}]') as Value")
                    .FirstOrDefault();

                return result.HasValue ? Convert.ToInt32(result) : 1;
            }
        }

        /// <summary>
        /// Set table identity (is supported)
        /// </summary>
        /// <typeparam name="T">Entity</typeparam>
        /// <param name="ident">Identity value</param>
        public virtual void SetTableIdent<T>(int ident) where T : BaseEntity
        {
            var currentIdent = GetTableIdent<T>();
            if (!currentIdent.HasValue || ident <= currentIdent.Value)
                return;

            using (var dataConnection = new DataConnection(ProviderName.SqlServer, _connectionString))
            {
                var tableName = dataConnection.GetTable<T>().TableName;
                dataConnection.Execute($"DBCC CHECKIDENT([{tableName}], RESEED, {ident})");
            }
        }

        /// <summary>
        /// Loads the original copy of the entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Copy of the passed entity</returns>
        public TEntity LoadOriginalCopy<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            using (var dataConnection = new DataConnection(ProviderName.SqlServer, _connectionString))
            {
                var entities = dataConnection.GetTable<TEntity>();
                return entities.FirstOrDefault(e => e.Id == Convert.ToInt32(entity.Id));
            }
        }

        #endregion

        #region Properties

        public string[] ConnectionStringOptions => new string[] { "Trusted_Connection", "MultipleActiveResultSets", "Connection Timeout", "Persist Security Info", "MultipleActiveResultSets" };

        /// <summary>
        /// Gets a value indicating whether this data provider supports backup
        /// </summary>
        public bool BackupSupported { get; } = true;

        /// <summary>
        /// Gets a maximum length of the data for HASHBYTES functions, returns 0 if HASHBYTES function is not supported
        /// </summary>
        public int SupportedLengthOfBinaryHash { get; } = 8000; //for SQL Server 2008 and above HASHBYTES function has a limit of 8000 characters.

        #endregion
    }
}
