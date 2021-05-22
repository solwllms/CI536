using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CI536
{
    public class Plugin
    {
        public bool isLoaded;

        public string Name => getName();
        public string Author => getAuthor();
        public string Summary => getSummary();
        public string Version => getVersion();

        public List<ImportMethod> ImportMethods => importMethods;

        private List<ImportMethod> importMethods;

        public Plugin()
        {
            importMethods = new List<ImportMethod>();
        }

        protected void RegisterImportMethod(ImportMethod im)
        {
            importMethods.Add(im);
        }

        public virtual async Task<bool> Load()
        {
            throw new NotImplementedException($"Load() is not defined in loaded plugin '{ getName() }'.");
        }

        public virtual async Task Authenticate()
        {
            throw new NotImplementedException($"Authenticate() is not defined in loaded plugin '{ getName() }'.");
        }

        public virtual async Task Sync()
        {
            throw new NotImplementedException($"Sync() is not defined in loaded plugin '{ getName() }'.");
        }

        public virtual string getName()
        {
            return "";
        }

        public virtual string getAuthor()
        {
            return "";
        }

        public virtual string getSummary()
        {
            return "";
        }
        public virtual string getVersion()
        {
            return "";
        }
    }

    public class ImportMethod
    {
        public string title;
        public string summary;
        public FrameworkElement icon;

        public string Title => title;
        public string Summary => summary;
        public FrameworkElement Icon => icon;

        public Action handler;

        public ImportMethod(string title, string summary, Action handler, FrameworkElement icon = null)
        {
            this.title = title;
            this.summary = summary;
            this.handler = handler;
            this.icon = icon;
        }
    }
}
