using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI536
{
    static internal class Ext
    {

        public static void ToggleVisibility(this GameEntry entry)
        {
            entry.Hidden = !entry.Hidden;
        }
    }
}
