using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CI536
{
    public class Debug
    {
        public static void WriteLine(object line)
        {
            string prefix = "";
            if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
            {
                prefix = $"{Assembly.GetCallingAssembly().GetName().Name}:";
            }

            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {prefix} {line}");
        }

        public static void Write(object line)
        {
            Console.Write($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {line}");
        }
    }
}
