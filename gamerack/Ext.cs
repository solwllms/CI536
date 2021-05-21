using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI536
{
    static internal class Ext
    {
        public static void EditInfo(this GameEntry entry)
        {
            EditGameInfo edit = new EditGameInfo(entry);
            //edit.Parent = MainWindow.instance;
            edit.ShowAsync();
        }

        public static void ToggleVisibility(this GameEntry entry)
        {
            entry.Hidden = !entry.Hidden;
        }
    }
}
