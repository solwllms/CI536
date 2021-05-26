using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
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

    // https://stackoverflow.com/questions/41062844/bind-to-resource-text-file/41063668#41063668
    public class TextExtension : MarkupExtension
    {
        private readonly string fileName;

        public TextExtension(string fileName)
        {
            this.fileName = fileName;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Error handling omitted
            var uri = new Uri("pack://application:,,,/" + fileName);
            using (var stream = Application.GetResourceStream(uri).Stream)
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
