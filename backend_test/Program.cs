using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CI536
{
    class Program
    {
        static void Main(string[] args)
        {
            UserConfig.Init();
            Library.Init();

            Library.AddGameEntry("Minecraft", new GameEntry())
            .AddConfig(new LaunchConfig()
            {
                Type = "local",
                LaunchCommand = "C:/"
            });
            Library.SaveChanges();

            Console.WriteLine("Backend Test!");
            Console.WriteLine("Finding dlls..");

            List<Plugin> plugins = new List<Plugin>();
            List<string> dlls = new List<string>();
            foreach (string d in Directory.GetDirectories(Directory.GetCurrentDirectory() + "/plugins/"))
            {
                dlls.AddRange(Directory.GetFiles(d, "*.dll"));
            }

            foreach (string path in dlls)
            {
                //Console.WriteLine("Loading: " + path);

                try
                {
                    Assembly dll = Assembly.LoadFrom(path);
                    Plugin plugin = (Plugin)dll.CreateInstance("plugin.PluginDLL");

                    if (plugin == null)
                    {
                        //Console.WriteLine("Plugin failed to load: Could not find entry point.");
                    }
                    else
                    {
                        Console.WriteLine("Loading: " + path);
                        plugin.Init();
                        plugins.Add(plugin);

                        Console.WriteLine("Registered: " + plugin.getName());
                    }
                }
                catch (BadImageFormatException)
                {
                    Console.WriteLine("Skipping invalid dll..");
                }
            }

            Console.WriteLine("Press any key.");
            Console.ReadKey();
        }
    }
}
