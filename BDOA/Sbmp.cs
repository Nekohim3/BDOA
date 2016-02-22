using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BDOA
{
    internal class Sbmp
    {
        private byte[] _buf;
        public int Height;
        public int Width;
        internal enum Type
        {
            Image,
            Mask
        }

        internal Sbmp(DxScreenCapture sc)
        {
            _buf = sc.GetImg();
            Width = Screen.PrimaryScreen.Bounds.Width;
            Height = Screen.PrimaryScreen.Bounds.Height;
        }
        internal Sbmp(byte[] b, int w, int h)
        {
            _buf = b;
            Width = w;
            Height = h;
        }

        internal Sbmp(Bitmap source)
        {
            if (source.PixelFormat != PixelFormat.Format32bppArgb)
                source = Convert32(source);
            Width = source.Width;
            Height = source.Height;
            _buf = new byte[Width*Height*4];
            var rect = new Rectangle(0, 0, Width, Height);
            var bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite, source.PixelFormat);
            Marshal.Copy(bitmapData.Scan0, _buf, 0, _buf.Length);
        }

        internal Bitmap Convert32(Bitmap orig)
        {
            var clone = new Bitmap(orig.Width, orig.Height, PixelFormat.Format32bppPArgb);
            using (var gr = Graphics.FromImage(clone))
                gr.DrawImage(orig, new Rectangle(0, 0, clone.Width, clone.Height));
            return clone;
        }
        internal Color GetPixel(int x, int y)
        {
            if (x < 0 || y < 0)
                return Color.FromArgb(0, 0, 0, 0);
            var i = (y * Width + x) * 4;
            if (i > _buf.Length - 4)
                throw new IndexOutOfRangeException();
            var b = _buf[i];
            var g = _buf[i + 1];
            var r = _buf[i + 2];
            var a = _buf[i + 3];
            return Color.FromArgb(a, r, g, b);
        }
        internal void SetPixel(int x, int y, Color color)
        {
            var i = (y * Width + x) * 4;
            _buf[i] = color.B;
            _buf[i + 1] = color.G;
            _buf[i + 2] = color.R;
            _buf[i + 3] = color.A;
        }
        internal Sbmp Copy() => new Sbmp(_buf.ToArray(), Width, Height);

        internal Sbmp Crop(Rectangle r)
        {
            var bbuf = new byte[r.Width * r.Height * 4];
            for (int i = r.X, ii = 0; i < r.X + r.Width; i++, ii++)
            {
                for (int j = r.Y, jj = 0; j < r.Y + r.Height; j++, jj++)
                {
                    var x = (jj * r.Width + ii) * 4;
                    var color = GetPixel(i, j);
                    bbuf[x] = color.B;
                    bbuf[x + 1] = color.G;
                    bbuf[x + 2] = color.R;
                    bbuf[x + 3] = color.A;
                }
            }
            return new Sbmp(bbuf, r.Width, r.Height);
        }
        internal Bitmap GetBmp()
        {
            var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            var bd = bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            var iptr = bd.Scan0;
            Marshal.Copy(_buf, 0, iptr, _buf.Length);
            bmp.UnlockBits(bd);
            return bmp;
        }

        public bool Compare(Sbmp b, Rectangle r, int threshold, int npc)
        {
            var counter = 0;
            for (int i = 0, ii = r.X; i < Width; i++, ii++)
            {
                for (int j = 0, jj = r.Y; j < Height; j++, jj++)
                {
                    var c = b.GetPixel(ii, jj);
                    var e = GetPixel(i, j);
                    if (Math.Abs(c.R - e.R) <= threshold && Math.Abs(c.G - e.G) <= threshold &&
                        Math.Abs(c.B - e.B) <= threshold)
                        counter++;
                }
            }
            double all = Width * Height;
            var proc = counter * 100 / all;
            return proc >= npc;
        }
        public bool Compare(Sbmp b, Rectangle r)
        {
            for (int i = 0, ii = r.X; i < Width; i++, ii++)
            {
                for (int j = 0, jj = r.Y; j < Height; j++, jj++)
                {
                    var c = b.GetPixel(ii, jj);
                    var e = GetPixel(i, j);
                    if (c != e)
                        return false;
                }
            }
            return true;
        }
        internal void Dispose()
        {
            _buf = null;
        }
    }
}
