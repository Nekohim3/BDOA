using System;
using System.Collections.Generic;
using System.IO;

namespace BDOA
{
    public class Neuro
    {
        internal Neuro()
        {
            Td = new List<NeuroData>();
            foreach (var q in Properties.Resources.Neuro.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                Td.Add(new NeuroData(q));
        }

        private List<NeuroData> Td { get; }

        private void Add(Sbmp b, int num)
        {
            Td.Add(new NeuroData(b, num));
        }

        internal string Recognize(Sbmp b)
        {
            double proc = 0;
            NeuroData td = null;
            foreach (var t in Td)
            {
                if (t.Width != b.Width || t.Height != b.Height)
                    continue;
                var p = t.Recognize(b);
                if (!(p > proc))
                    continue;
                proc = p;
                td = t;
            }
            return proc >= 95 ? td?.Numb : "-";
        }
    }
    public class NeuroData
    {
        private readonly byte[] _buf;
        public int Height;
        public string Numb;
        public int Width;

        internal NeuroData(Sbmp b, int num)
        {
            Numb = num.ToString();
            Width = b.Width;
            Height = b.Height;
            _buf = new byte[b.Width * b.Height];
            var counter = 0;
            for (var i = 0; i < Width; i++)
                for (var j = 0; j < Height; j++)
                {
                    _buf[counter] = b.GetPixel(i, j).R == 255 ? (byte)0 : (byte)1;
                    counter++;
                }
            Train(num);
        }

        internal NeuroData(string str)
        {
            Numb = str.Split(':')[0];
            Width = Convert.ToInt32(str.Split(':')[1]);
            Height = Convert.ToInt32(str.Split(':')[2]);
            _buf = new byte[Width * Height];
            var data = str.Split(':')[3].Split(new[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries)[0];
            for (var i = 0; i < Width * Height; i++)
            {
                _buf[i] = Convert.ToByte(data[i].ToString());
            }
        }

        internal void Train(int num)
        {
            var fs = new FileStream("traindata", FileMode.Append, FileAccess.Write);
            var sw = new StreamWriter(fs);
            sw.Write(num + ":" + Width + ":" + Height + ":[");
            foreach (var t in _buf)
                sw.Write(t.ToString());
            sw.WriteLine("]");
            sw.Close();
            fs.Close();
        }

        internal double Recognize(Sbmp b)
        {
            if (Width != b.Width || Height != b.Height)
                return -1;
            var counter = 0;
            var scount = 0;
            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                {
                    if (_buf[counter] == (b.GetPixel(i, j).R == 255 ? 0 : 1))
                        scount++;
                    counter++;
                }
            }
            return scount * (double)100 / counter;
        }
    }
}
