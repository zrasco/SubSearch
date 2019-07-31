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
            Simple = new List<string>();
            Plugins = new List<Plugin>();
        }

        public string RootDirectory { get; set; }
        public string DefaultLanguage { get; set; }
        public string VideoExtensions { get; set; }
        public List<string> Simple { get; }
        public List<Plugin> Plugins { get; }
        
    }
}
