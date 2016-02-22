using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BDOA
{
    internal class Mask
    {
        public Mask(string fn)
        {
            Types = new List<Color>();
            var bmp = new Bitmap(fn);
            Colors = new int[bmp.Width, bmp.Height];
            for (var i = 0; i < bmp.Width; i++)
                for (var j = 0; j < bmp.Height; j++)
                    if (Types.Count(x => x == bmp.GetPixel(i, j)) == 0)
                        Types.Add(bmp.GetPixel(i, j));
            for (var i = 0; i < bmp.Width; i++)
                for (var j = 0; j < bmp.Height; j++)
                    for (var q = 0; q < Types.Count; q++)
                        if (bmp.GetPixel(i, j) == Types[q])
                            Colors[i, j] = q;
            Width = bmp.Width;
            Height = bmp.Height;
            bmp.Dispose();
        }


        public Mask(Bitmap bmp)
        {
            Types = new List<Color>();
            Colors = new int[bmp.Width, bmp.Height];
            for (var i = 0; i < bmp.Width; i++)
                for (var j = 0; j < bmp.Height; j++)
                    if (Types.Count(x => x == bmp.GetPixel(i, j)) == 0)
                        Types.Add(bmp.GetPixel(i, j));
            for (var i = 0; i < bmp.Width; i++)
                for (var j = 0; j < bmp.Height; j++)
                    for (var q = 0; q < Types.Count; q++)
                        if (bmp.GetPixel(i, j) == Types[q])
                            Colors[i, j] = q;
            Width = bmp.Width;
            Height = bmp.Height;
            bmp.Dispose();
        }

        private List<Color> Types { get; }

        private int[,] Colors { get; set; }

        private int Width { get; }

        private int Height { get; }

        public bool Compare(Sbmp b, int x, int y)
        {
            if (x + Width > b.Width || y + Height > b.Height)
                return false;
            for (int i = x, ii = 0; ii < Width; i++, ii++)
            {
                for (int j = y, jj = 0; jj < Height; j++, jj++)
                {
                    var c = b.GetPixel(i, j);
                    if (Types[Colors[ii, jj]].A == 0)
                    {
                        if (Types.Count(v => v == c) != 0)
                            return false;
                    }
                    else
                    {
                        if (c != Types[Colors[ii, jj]])
                            return false;
                    }
                }
            }
            return true;
        }
        public bool Compare(Sbmp b, Rectangle r)
        {
            if (r.X + Width > b.Width || r.Y + Height > b.Height)
                return false;
            for (int i = r.X, ii = 0; ii < Width; i++, ii++)
            {
                for (int j = r.Y, jj = 0; jj < Height; j++, jj++)
                {
                    var c = b.GetPixel(i, j);
                    if (Types[Colors[ii, jj]].A == 0)
                    {
                        if (Types.Count(v => v == c) != 0)
                            return false;
                    }
                    else
                    {
                        if (c != Types[Colors[ii, jj]])
                            return false;
                    }
                }
            }
            return true;
        }
        public bool Compare(Sbmp b, Rectangle r, out Point pt)
        {
            for (var i = r.X; i <= r.X + r.Width - Width; i++)
                for (var j = r.Y; j <= r.Y + r.Height - Height; j++)
                    if (Compare(b, i, j))
                    {
                        pt = new Point(i, j);
                        return true;
                    }
            pt = new Point(-1, -1);
            return false;
        }
        public bool Compare(Sbmp b, int x, int y, bool qwe)
        {
            if (x + Width > b.Width || y + Height > b.Height)
                return false;
            for (int i = x, ii = 0; ii < Width; i++, ii++)
            {
                for (int j = y, jj = 0; jj < Height; j++, jj++)
                {
                    var c = b.GetPixel(i, j);
                    if (Types[Colors[ii, jj]].A == 0)
                        continue;
                    if (c != Types[Colors[ii, jj]])
                        return false;
                }
            }
            return true;
        }

        public bool Compare(Sbmp b, int x, int y, int threshold, int npc)
        {
            if (x + Width > b.Width || y + Height > b.Height)
                return false;
            var counter = 0;
            for (int i = x, ii = 0; ii < Width; i++, ii++)
            {
                for (int j = y, jj = 0; jj < Height; j++, jj++)
                {
                    var c = b.GetPixel(i, j);
                    var e = Types[Colors[ii, jj]];
                    if (Math.Abs(c.R - e.R) <= threshold && Math.Abs(c.G - e.G) <= threshold &&
                        Math.Abs(c.B - e.B) <= threshold)
                        counter++;
                }
            }
            double all = Width * Height;
            var proc = counter * 100 / all;
            return proc >= npc;
        }

        public void Dispose()
        {
            Types.Clear();
            Colors = null;
        }
    }
}
