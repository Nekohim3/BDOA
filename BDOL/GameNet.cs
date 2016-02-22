using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace BDOL
{
    internal class GameNet
    {
        public static string GetHwid()
        {
            var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\GGS\QGNA");
            if (key != null)
            {
                var v1 = key.GetValue("userId");
                var v2 = key.GetValue("second_userId");
                key.Close();
                if (v1 != null && v2 == null)
                    return v1.ToString();
                if (v1 == null)
                    MessageBox.Show(@"Войдите в аккаунт GN и перезапустите бота");
                if (v2 != null)
                    MessageBox.Show(@"Выйдите из второго аккаунта GN и перезапустите бота");
                Process.GetCurrentProcess().Kill();
                return "";
            }
            MessageBox.Show(@"Black Desert не установлена");
            Process.GetCurrentProcess().Kill();
            return "";
        }
    }
}
