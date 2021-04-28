using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI536
{
    public class Plugin
    {
        public bool isLoaded;

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
    }
}
