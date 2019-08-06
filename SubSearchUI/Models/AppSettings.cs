using System;
using System.Collections.Generic;
using System.Text;

namespace SubSearchUI
{
    public class Plugin
    {
        public string Name { get; set; }
        public string File { get; set; }
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
        public List<Plugin> Plugins { get; }
    }
}
