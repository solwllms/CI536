using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CI536
{
    class WPFUtil
    {
        // https://stackoverflow.com/questions/18435829/showing-image-in-wpf-using-the-url-link-from-database
        public static BitmapImage GetImageFromURL(string url, int width, int height, bool cache)
        {
            if (url == null) return null;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.DecodePixelWidth = width;
            bitmap.DecodePixelHeight = height;
            bitmap.CacheOption = cache ? BitmapCacheOption.OnLoad : BitmapCacheOption.None;
            bitmap.UriCachePolicy = new RequestCachePolicy(cache ? RequestCacheLevel.CacheIfAvailable : RequestCacheLevel.BypassCache);
            bitmap.UriSource = new Uri(url, UriKind.Absolute);
            bitmap.EndInit();

            bitmap.DownloadCompleted += (s, e) => {
                bitmap.Freeze();
            };

            return bitmap;
        }
    }
}
