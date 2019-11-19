using System.Collections.Generic;
using Nop.Data.Data;

namespace Nop.Data
{
    public interface IDataSettings
    {
        /// <summary>
        /// Gets or sets a data provider
        /// </summary>
        DataProviderType DataProvider { get; set; }

        /// <summary>
        /// Gets or sets a connection string
        /// </summary>
        string DataConnectionString { get; }

        /// <summary>
        /// Gets or sets a raw settings
        /// </summary>
        IDictionary<string, string> RawDataSettings { get; }
    }
}
