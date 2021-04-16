using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI536
{
    public class Plugin
    {
        public virtual void Init()
        {
            throw new NotImplementedException("Init() is not defined in plugin.");
        }

        public virtual string getName()
        {
            return "";
        }
    }
}
