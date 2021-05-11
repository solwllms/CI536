using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CI536
{
    class WPFUtil
    {
        // https://stackoverflow.com/questions/18435829/showing-image-in-wpf-using-the-url-link-from-database
        public static BitmapImage GetImageFromURL(string url)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(url, UriKind.Absolute);
            bitmap.EndInit();

            return bitmap;
        }
    }
}
