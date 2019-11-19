using Nop.Core;
using Nop.Data.Data;
using Nop.Data.DataProviders;

namespace Nop.Data
{
    /// <summary>
    /// Represents the Entity Framework data provider manager
    /// </summary>
    public partial class DataProviderManager : IDataProviderManager
    {
        #region Properties

        /// <summary>
        /// Gets data provider
        /// </summary>
        public IDataProvider DataProvider
        {
            get
            {
                var dataSettings = DataSettingsManager.LoadSettings();


                switch (dataSettings.DataProvider)
                {
                    case DataProviderType.SqlServer:
                        return new MsSqlDataProvider(dataSettings.DataConnectionString);
                    default:
                        throw new NopException($"Not supported data provider name: '{dataSettings.DataProvider}'");
                }
            }
        }

        #endregion
    }
}
