using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace upd
{
    class Program
    {
        private static string _url = "http://angelhci.bget.ru/FILES/";
        static void Main(string[] args)
        {
            while (Process.GetProcessesByName("BDOL").Length != 0)
            {
                try
                {
                    Process.GetProcessesByName("BDOL")[0].Kill();
                }
                catch
                {
                    
                }
            }
            var wc = new WebClient();
            var files = BDOLFiles();
            foreach (var q in files)
                wc.DownloadFile(_url + "BDOL/" + q, q);
            File.WriteAllText("version", BDOLvers());
            Process.Start("BDOL.exe");
        }
        private static string BDOLvers()
        {
            return new WebClient().DownloadString(_url + "BDOL/version.txt");
        }
        private static List<string> BDOLFiles()
        {
            return new WebClient().DownloadString(_url + "BDOL/FILES.txt")
                .Replace("\n", "")
                .Replace("\r", "")
                .Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

        }
    }
}
