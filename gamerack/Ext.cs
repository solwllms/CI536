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
            edit.Show();
        }

        public static void Hide(this GameEntry entry)
        {
            throw new NotImplementedException();
        }
    }
}
