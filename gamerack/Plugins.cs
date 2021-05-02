﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CI536
{
    class Plugins
    {
        static Dictionary<string ,Plugin> plugins;

        public static Plugin GetPlugin(string key)
        {
            if (!plugins.ContainsKey(key)) return null;

            return plugins[key];
        }

        public static void LoadPlugins()
        {
            Debug.WriteLine("Finding dlls..");

            plugins = new Dictionary<string, Plugin>();
            List<string> dlls = new List<string>();
            foreach (string d in Directory.GetDirectories(Directory.GetCurrentDirectory() + "/plugins/"))
            {
                dlls.AddRange(Directory.GetFiles(d, "*.dll"));
            }

            foreach (string path in dlls)
            {
                //Debug.WriteLine("Loading: " + path);

                try
                {
                    Assembly dll = Assembly.LoadFrom(path);
                    Plugin plugin = (Plugin)dll.CreateInstance("plugin.PluginDLL");

                    if (plugin == null)
                    {
                        //Debug.WriteLine("Plugin failed to load: Could not find entry point.");
                    }
                    else
                    {
                        if (plugins.ContainsKey(plugin.getName().ToLower())) continue;

                        Debug.WriteLine("Loading: " + path);
                        Task setup = new Task(async () =>
                        {
                            bool loaded = await plugin.Load();
                            plugin.isLoaded = loaded;
                            await plugin.Sync();
                        });
                        setup.Start();
                        plugins.Add(plugin.getName().ToLower(), plugin);

                        Debug.WriteLine("Registered: " + plugin.getName());
                    }
                }
                catch (BadImageFormatException)
                {
                    Debug.WriteLine("Skipping invalid dll..");
                }
            }
        }
    }
}