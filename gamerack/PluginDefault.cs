using CI536;
using ModernWpf.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace plugin
{
    class PluginDefault : Plugin
    {

        public PluginDefault()
        {
            ImportMethod main = new ImportMethod("Local Disk", "Import game executables from local disk.", ImportGames, new SymbolIcon(Symbol.NewFolder));
            RegisterImportMethod(main);
        }

        public void ImportGames()
        {
            int i = 0;
        }

        public override async Task<bool> Load()
        {
            return true;
        }

        public override async Task Authenticate()
        {

        }

        public override async Task Sync()
        {

        }

        public override string getName() { return "Gamerack"; }
        public override string getAuthor() { return "Group B - CI536"; }
        public override string getVersion() { return "v1.0"; }
        public override string getSummary() { return "Default Gamerack import options."; }
    }
}
