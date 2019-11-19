using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Data.Data;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Models.Install
{
    public partial class InstallModel
    {
        public InstallModel()
        {
            AvailableLanguages = new List<SelectListItem>();
            AvailableDataProviders = new List<SelectListItem>();
        }
        
        public string AdminEmail { get; set; }
        [NoTrim]
        [DataType(DataType.Password)]
        public string AdminPassword { get; set; }
        [NoTrim]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        public string Collation { get; set; }

        public bool CreateDatabaseIfNotExists { get; set; }
        public bool DisableSampleDataOption { get; set; }
        public bool InstallSampleData { get; set; }
        public bool ConnectionStringRaw { get; set; }
        public string ConnectionString { get; set; }

        public List<SelectListItem> AvailableLanguages { get; set; }
        public DataProviderType DataProvider { get; set; }

        public ConnectionStringModel NopConnectionString
        {
            get; set;
        }

        public List<SelectListItem> AvailableDataProviders { get; set; }
        public IDictionary<string, string> RawDataSettings => new Dictionary<string, string>();
    }
}