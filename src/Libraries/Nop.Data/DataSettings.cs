﻿using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Nop.Data.Data;

namespace Nop.Data
{
    /// <summary>
    /// Represents the data settings
    /// </summary>
    public partial class DataSettings : IDataSettings
    {
        #region Ctor

        public DataSettings() : this(DataProviderType.Unknown)
        {

        }

        public DataSettings(DataProviderType dataProviderType)
        {
            DataProvider = dataProviderType;
            RawDataSettings = new Dictionary<string, string>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a data provider
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DataProviderType DataProvider { get; set; }

        /// <summary>
        /// Gets or sets a connection string
        /// </summary>
        public string DataConnectionString { get; set; }

        /// <summary>
        /// Gets or sets a raw settings
        /// </summary>
        public IDictionary<string, string> RawDataSettings { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the information is entered
        /// </summary>
        /// <returns></returns>
        [JsonIgnore]
        public bool IsValid => DataProvider != DataProviderType.Unknown && !string.IsNullOrEmpty(DataConnectionString);

        #endregion
    }
}