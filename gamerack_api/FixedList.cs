using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI536
{
    public class FixedList<T>
    {
        List<T> l = new List<T>();
        private object lockObject = new object();

        public int Limit { get; set; }

        public FixedList(int size)
        {
            Limit = size;
        }

        public T[] ToArray()
        {
            return l.ToArray();
        }

        public void Enqueue(T obj)
        {
            if (l.Contains(obj))
                l.Remove(obj);

            l.Insert(0, obj);
            lock (lockObject)
            {
                while (l.Count > Limit) l.RemoveAt(Limit - 1);
            }
        }
    }
}
