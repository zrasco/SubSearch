using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using ProviderPluginTypes;
using SubSearchUI.Services.Abstract;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace SubSearchUI
{
    public class Plugin
    {
        public string Name { get; set; }
        public string File { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public IProviderPlugin Interface { get; set; } = null;
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public bool Loaded {
            get
            {
                if (Interface == null)
                    return false;
                else
                    return true;
            }
        }
    }
    public class AppSettings
    {
        public AppSettings()
        {
            Plugins = new List<Plugin>();
        }
        public int MaxBackgroundJobs { get; set; }
        public int SchedulerQuantum { get; set; }
        public string RootDirectory { get; set; }
        public string DefaultLanguage { get; set; }
        public string VideoExtensions { get; set; }
        public List<Plugin> Plugins { get; set; }
    }
}
