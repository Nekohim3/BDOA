using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BDOA
{
    internal static class ImgHelpers
    {
        internal static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }
        internal static Sbmp Threshold(Sbmp bq)
        {
            var b = bq.Copy();
            for (var i = 0; i < b.Width; i++)
            {
                for (var j = 0; j < b.Height; j++)
                {
                    var c = b.GetPixel(i, j);
                    if (Math.Abs(c.R - c.G) < 120 && Math.Abs(c.G - c.B) < 120 && Math.Abs(c.R - c.B) < 120)
                    {
                        b.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    }
                    else
                    {
                        b.SetPixel(i, j, Color.FromArgb(255, 0, 0, 0));
                    }
                }
            }
            return b;
        }

        internal static Rectangle GetRect(Sbmp b)
        {
            var x = -1;
            var w = -1;
            for (var i = 0; i < b.Width; i++)
            {
                var counter = 0;
                for (var j = 0; j < b.Height; j++)
                {
                    if (b.GetPixel(i, j).R != 255 && x == -1)
                    {
                        x = i;
                        w = 0;
                        break;
                    }
                    if (x != -1 && b.GetPixel(i, j).R == 255)
                    {
                        counter++;
                    }
                }
                if (counter != b.Height)
                    w++;
                else
                    break;
            }
            var y = -1;
            var h = -1;
            for (var j = 0; j < b.Height; j++)
            {
                var counter = 0;
                for (var i = 0; i < b.Width; i++)
                {
                    if (b.GetPixel(i, j).R != 255 && y == -1)
                    {
                        y = j;
                        h = 0;
                        break;
                    }
                    if (y != -1 && b.GetPixel(i, j).R == 255)
                    {
                        counter++;
                    }
                }
                if (counter != b.Width)
                    h++;
                else
                    break;
            }
            return new Rectangle(x, y, w, h);
        }
    }
}
