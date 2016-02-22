using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BDOA
{
    public class License
    {
        private static bool _lic;
        // ReSharper disable once NotAccessedField.Local
        private System.Threading.Timer _timer;
        // ReSharper disable once NotAccessedField.Local
        private System.Threading.Timer _timerhw;
        public License(string url)
        {
            _url = url;
            _hwid = GameNet.GetHwid();
            CheckLicense();
            _timer = new System.Threading.Timer(CheckLicense, null, 1000 * 60, 1000 * 60);
            _timerhw = new System.Threading.Timer(HwidCheck, null, 1000 * 30, 1000 * 30);
        }

        private static void HwidCheck(object o = null) => GameNet.GetHwid();
        internal string GetHwid() => GameNet.GetHwid();

        private static string _url;
        private static string _hwid;
        public bool GetLic() => _lic;
        private static DateTime _dt;
        public DateTime GetLicInfo()
        {
            //CheckLicense();
            return _dt;
        }
        
        private static string GetSignedText(string[] response)
        {
            var returned = string.Empty;
            for (var i = 0; i < response.Length - 1; i++)
            {
                returned += response.GetValue(i) + "\r\n";
            }
            return returned;
        }
        private static string Info(int token1, string preKey)
        {
            string tokenString = $"token={DigitalSign.EncryptString(token1.ToString(), preKey)}&key={_hwid}";
            return tokenString;
        }
        private static string GetRequest(string post)
        {
            var request = (HttpWebRequest)WebRequest.Create(_url);
            var buffer = Encoding.UTF8.GetBytes(post);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = buffer.Length;
            request.Method = "POST";
            var newStream = request.GetRequestStream();
            newStream.Write(buffer, 0, buffer.Length);
            newStream.Close();
            var response = (HttpWebResponse)request.GetResponse();
            // ReSharper disable once AssignNullToNotNullAttribute
            var strReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1251));
            var workingPage = strReader.ReadToEnd();
            response.Close();
            return workingPage;
        }

        private static string RandomStringWithNumbers(int maxlength, Random rn)
        {
            var sb = new StringBuilder();
            var allowedChars = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            for (var i = 0; i < maxlength; i++)
            {
                var n = rn.Next(0, allowedChars.Length);
                if (char.IsLetter(allowedChars[n]))
                {
                    if (rn.Next(0, 2) == 0)
                    {
                        sb.Append(allowedChars[n].ToString().ToUpper());
                    }
                    else
                    {
                        sb.Append(allowedChars[n]);
                    }
                }
                else
                {
                    sb.Append(allowedChars[n]);
                }
            }
            return sb.ToString();
        }

        private static void CheckLicense(object o = null)
        {
            try
            {
                const string pubKeyNotXoRed =
                    "<RSAKeyValue><Modulus>wzzpNEbNjAWLI3AgFLb0YZ8gjqqmIgD1rTU3xy25IuetNsA5QKJxP7whwKq2LF4ul56LhxG3M3Wxhr/kZOgXWbPjEG1zyB7Lx83B65z6CFRIV5llzZziq6/uZP2cEHfwF62/letOD+tUaVvsi4/jvVv5NEanfeoXIQ1cbSBuWaEXfNpra48lmAZpTSLu93l9o+ooJsyrwBBHwWrS3/ljE/19PF4SetqNvUi7FpsrtB5emsG26M7AYiEv/lN2oJlOMU4+yJa/na+LZEdjSsG7yUiSTu2r23ixQlfKbGLwCDtP3VLgf2wrptq7KCIaYrQIznl/kON7CeHT10kKvcjAPQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
                var curRandom = new Random();
                var preKey = RandomStringWithNumbers(curRandom.Next(15, 21), curRandom);
                var xoRkey = curRandom.Next(1, int.MaxValue);
                var pubKey = DigitalSign.Xor(pubKeyNotXoRed, xoRkey);

                var token = curRandom.Next(1000000, int.MaxValue);
                var infoXoRed = DigitalSign.Xor(Info(token, preKey), xoRkey);
                var responseXoRed =
                    DigitalSign.Xor(
                        GetRequest(DigitalSign.Xor(infoXoRed, xoRkey)), xoRkey);
                var responseSplitted = DigitalSign.Xor(responseXoRed, xoRkey)
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (responseSplitted[0].Split('=')[1] == "1")
                {
                    if (int.Parse(DigitalSign.DecryptString(responseSplitted[4], preKey)) != token)
                        return;
                    if (_hwid != Encoding.UTF8.GetString(
                        Convert.FromBase64String(
                            responseSplitted[1].Split(new[] { '[', ']' },
                                StringSplitOptions.RemoveEmptyEntries)[0])))
                        return;
                    if (!DigitalSign.CompareRsaMethod(GetSignedText(responseSplitted),
                        responseSplitted[responseSplitted.Length - 1].Split(new[] { '[', ']' },
                            StringSplitOptions.RemoveEmptyEntries)[0], DigitalSign.Xor(pubKey, xoRkey)))
                        return;
                    var currentTime = DateTime.Parse(responseSplitted[2].Split('=')[1]);
                    var endTime = DateTime.Parse(responseSplitted[3].Split('=')[1]);
                    var activatedTime = endTime.Subtract(currentTime);
                    var timeOffset = currentTime.Subtract(DateTime.Now);
                    _dt = endTime - timeOffset;
                    _lic = activatedTime >= TimeSpan.Zero;
                }
                else
                {
                    Register("");
                }
            }
            catch(Exception e)
            {
                if (!_lic)
                {
                    MessageBox.Show(@"Не удается связвться с сервером авторизации");
                    Process.GetCurrentProcess().Kill();
                }
            }
        }
        private static void Register(string nick)
        {
            try
            {
                const string pubKeyNotXoRed =
                    "<RSAKeyValue><Modulus>wzzpNEbNjAWLI3AgFLb0YZ8gjqqmIgD1rTU3xy25IuetNsA5QKJxP7whwKq2LF4ul56LhxG3M3Wxhr/kZOgXWbPjEG1zyB7Lx83B65z6CFRIV5llzZziq6/uZP2cEHfwF62/letOD+tUaVvsi4/jvVv5NEanfeoXIQ1cbSBuWaEXfNpra48lmAZpTSLu93l9o+ooJsyrwBBHwWrS3/ljE/19PF4SetqNvUi7FpsrtB5emsG26M7AYiEv/lN2oJlOMU4+yJa/na+LZEdjSsG7yUiSTu2r23ixQlfKbGLwCDtP3VLgf2wrptq7KCIaYrQIznl/kON7CeHT10kKvcjAPQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
                var curRandom = new Random();
                var preKey = RandomStringWithNumbers(curRandom.Next(15, 21), curRandom);
                var xoRkey = curRandom.Next(1, int.MaxValue);
                var pubKey = DigitalSign.Xor(pubKeyNotXoRed, xoRkey);

                var token = curRandom.Next(1000000, int.MaxValue);
                var infoXoRed = DigitalSign.Xor(Info(token, preKey), xoRkey);
                var responseXoRed =
                    DigitalSign.Xor(
                        GetRequest(DigitalSign.Xor(infoXoRed, xoRkey)), xoRkey);
                var responseSplitted = DigitalSign.Xor(responseXoRed, xoRkey)
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (responseSplitted[0].Split('=')[1] != "2")
                    return;
                if (int.Parse(DigitalSign.DecryptString(responseSplitted[4], preKey)) != token)
                    return;
                if (GameNet.GetHwid() != Encoding.UTF8.GetString(
                    Convert.FromBase64String(
                        responseSplitted[1].Split(new[] { '[', ']' },
                            StringSplitOptions.RemoveEmptyEntries)[0])))
                    return;
                if (!DigitalSign.CompareRsaMethod(GetSignedText(responseSplitted),
                    responseSplitted[responseSplitted.Length - 1].Split(new[] { '[', ']' },
                        StringSplitOptions.RemoveEmptyEntries)[0], DigitalSign.Xor(pubKey, xoRkey)))
                    return;
                _lic = false;
            }
            catch (Exception e)
            {
                if (!_lic)
                {
                    MessageBox.Show(@"Не удается связвться с сервером авторизации");
                    Process.GetCurrentProcess().Kill();
                }
            }
        }
    }
}
