using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;
// ReSharper disable EmptyGeneralCatchClause

namespace BDOL
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string _url = "http://angelhci.bget.ru/FILES/";
        private WebClient _wc = new WebClient();
        License _lic;
        private static bool CheckDriver()
        {
               return File.Exists(@"C:/Windows/Sysnative/drivers/mouse.sys") &&
                File.Exists(@"C:/Windows/Sysnative/drivers/keyboard.sys");
        }

        private bool BDOL()
        {
            var cver = File.ReadAllText("version");
            var ver = BDOLvers();
            if (ver == cver) return false;
            _wc.DownloadFile(_url + "BDOL/upd.exe", "upd.exe");
            Process.Start("upd.exe");
            return true;
        }

        private string BDOLvers()
        {
            return _wc.DownloadString(_url + "BDOL/version.txt");
        }
        private void BDOA()
        {
            var vers = BDOAVers();
            CBbdoaVers.Items.Clear();
            CBbdoaVers.Items.Add("-");
            while (_lic == null)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }
            if(_lic.GetHwid() == "400001000139167141")
            foreach (var q in vers)
                CBbdoaVers.Items.Add(q);
            else
                foreach (var q in vers.Where(x => x.Split('.').Last() != "0"))
                    CBbdoaVers.Items.Add(q);
            if (!Directory.Exists("BDOA"))
            {
                Directory.CreateDirectory("BDOA");
                var files = BDOAFiles(CBbdoaVers.Items[1].ToString());
                foreach (var q in files)
                    _wc.DownloadFile(_url + "BDOA/" + CBbdoaVers.Items[1] + "/" + q, "BDOA/" + q);
                File.WriteAllText("BDOA/version", CBbdoaVers.Items[1].ToString());
                CBbdoaVers.SelectedIndex = 1;
            }
            else
            {
                try
                {
                    var cvers = File.ReadAllText("BDOA/version");
                    if (CBbdoaVers.Items.Contains(cvers))
                    {
                        CBbdoaVers.SelectedItem = cvers;
                    }
                    else
                    {
                        var f = Directory.GetFiles("BDOA");
                        foreach (var q in f)
                            File.Delete(q);
                        var files = BDOAFiles(CBbdoaVers.Items[1].ToString());
                        foreach (var q in files)
                            _wc.DownloadFile(_url + "BDOA/" + CBbdoaVers.Items[1] + "/" + q, "BDOA/" + q);
                        File.WriteAllText("BDOA/version", CBbdoaVers.Items[1].ToString());
                        CBbdoaVers.SelectedIndex = 1;
                    }
                }
                catch
                {
                    var f = Directory.GetFiles("BDOA");
                    foreach (var q in f)
                        File.Delete(q);
                    Directory.Delete("BDOA");
                    CBbdoaVers.SelectedIndex = 0;
                }
            }
        }
        private List<string> BDOAVers()
        {
            return _wc.DownloadString(_url + "BDOA/versions.txt")
                .Replace("\n", "")
                .Replace("\r", "")
                .Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

        }
        private List<string> BDOAFiles(string vers)
        {
            return _wc.DownloadString(_url + "BDOA/" + vers + "/FILES.txt")
                .Replace("\n", "")
                .Replace("\r", "")
                .Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

        }
        private void Test()
        {
            if (MessageBox.Show("Неоиходимо открыть окно аукциона в игре. Начать теcтирование?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var f = new TestW();
                f.Show();
            }
        }
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            IClose.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.close);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            if (BDOL()) return;
            if (CheckDriver())
            {
                Bdrv.Content = "Удалить драйвер";
                Btest.IsEnabled = true;
            }
            else
            {
                Bdrv.Content = "Установить драйвер";
                Btest.IsEnabled = false;
            }
            if (!Properties.Settings.Default.fr)
            {
                MessageBox.Show("Необходимо установить драйвер, после этого запустить тестирование. Если тестирование пройдет успешно, можете приступать к оплате.");
                Properties.Settings.Default.fr = true;
                Properties.Settings.Default.Save();
            }
            _lic = new License("http://angelhci.bget.ru/LBDOA.php");
            new Thread(PCheck).Start();
            if (_lic.GetLic())
                BDOA();
            else
                BbdoaRun.Content = "Купить BDOA";
        }
        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("SlimDX"))
            {
                var asm = Assembly.LoadFrom("0.dll");
                return asm;
            }
            if (args.Name.StartsWith("MouseKeyboardActivityMonitor"))
            {
                var asm = Assembly.LoadFrom("1.dll");
                return asm;
            }
            if (args.Name.StartsWith("Interceptor"))
            {
                var asm = Assembly.LoadFrom("2.dll");
                return asm;
            }
            return null;
        }
       private  void CBbdoaVers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(_s != State.NLic)
            if (Directory.Exists("BDOA"))
            {
                var f = Directory.GetFiles("BDOA");
                foreach (var q in f)
                    File.Delete(q);
                if (CBbdoaVers.SelectedIndex == 0)
                {
                    Directory.Delete("BDOA");
                }
                else
                {
                    var files = BDOAFiles(CBbdoaVers.SelectedItem.ToString());
                    foreach (var q in files)
                        _wc.DownloadFile(_url + "BDOA/" + CBbdoaVers.SelectedItem + "/" + q, "BDOA/" + q);
                    File.WriteAllText("BDOA/version", CBbdoaVers.SelectedItem.ToString());
                }
            }
            else
            {
                if (CBbdoaVers.SelectedIndex != 0)
                {
                    Directory.CreateDirectory("BDOA");
                    var files = BDOAFiles(CBbdoaVers.SelectedItem.ToString());
                    foreach (var q in files)
                        _wc.DownloadFile(_url + "BDOA/" + CBbdoaVers.SelectedItem + "/" + q, "BDOA/" + q);
                    File.WriteAllText("BDOA/version", CBbdoaVers.SelectedItem.ToString());
                }
            }
        }
        private void Bdrv_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckDriver())
            {
                _wc.DownloadFile(_url + "driver.exe", "driver.exe");
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "driver.exe",
                        Arguments = "/install",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                proc.Start();
                var line = "";
                while (!proc.StandardOutput.EndOfStream)
                    line += proc.StandardOutput.ReadLine();
                var p = Process.GetProcessesByName("driver");
                while (p.Length != 0)
                    try
                    {
                        p[0].Kill();
                    }
                    catch
                    {
                    }
                while (File.Exists("driver.exe"))
                    try
                    {
                        File.Delete("driver.exe");
                    }
                    catch
                    {
                    }
                if (
                    line.IndexOf(@"Interception successfully installed. You must reboot for it to take effect.",
                        StringComparison.Ordinal) != -1)
                {
                    MessageBox.Show("Драйвер успешно установлен. Перезапустите компьютер для корректной работы драйвера.");
                }
                else
                {
                    MessageBox.Show("Не удалось установить драйвер. Перезапустите компьютер и попробуйте еще раз.");
                }
            }
            else
            {
                _wc.DownloadFile(_url + "driver.exe", "driver.exe");
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "driver.exe",
                        Arguments = "/uninstall",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                proc.Start();
                var line = "";
                while (!proc.StandardOutput.EndOfStream)
                    line += proc.StandardOutput.ReadLine();
                var p = Process.GetProcessesByName("driver");
                while (p.Length != 0)
                    try
                    {
                        p[0].Kill();
                    }
                    catch
                    {
                    }
                while (File.Exists("driver.exe"))
                    try
                    {
                        File.Delete("driver.exe");
                    }
                    catch
                    {
                    }
                if (
                    line.IndexOf(@"Interception uninstalled. You must reboot for this to take effect.",
                        StringComparison.Ordinal) != -1)
                {
                    MessageBox.Show("Драйвер успешно удален. Перезапустите компьютер для полного удаления.");
                }
                else
                {

                    MessageBox.Show("Не удалось удалить драйвер. Перезапустите компьютер и попробуйте еще раз.");
                }
            }
        }
        private void Btest_Click(object sender, RoutedEventArgs e)
        {
            Test();
        }
        private void BbdoaRun_Click(object sender, RoutedEventArgs e)
        {
            if (_s == State.NLic)
            {
                Process.Start("http://бдобот.рф/?page=buy&bot_id=" + _lic.GetHwid() + "&bot_type=1");
            }
            if (_s == State.Off)
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "BDOA/BDOA.exe",
                        Arguments = _lic.GetHwid(),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        WorkingDirectory = "BDOA"
                    }
                };
                proc.Start();
            }
            if (_s == State.On)
            {
                try
                {
                    Process.GetProcessesByName("BDOA")[0].Kill();
                }
                catch (Exception)
                {
                    
                }
            }
        }
        private void PCheck()
        {
            while (true)
            {
                if (_lic.GetLic())
                {
                    if (Process.GetProcessesByName("BDOA").Length != 0)
                    {
                        Dispatcher.Invoke(() => 
                        {
                            BbdoaRun.Content = "Закрыть BDOA";
                            BbdoaRun.IsEnabled = true;
                            CBbdoaVers.IsEnabled = false;
                        });
                        _s = State.On;
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            BbdoaRun.Content = "Запустить BDOA";
                            if (CBbdoaVers.SelectedIndex == 0)
                                BbdoaRun.IsEnabled = false;
                            else
                                BbdoaRun.IsEnabled = true;
                            CBbdoaVers.IsEnabled = true;
                        });
                        _s = State.Off;
                    }
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        BbdoaRun.Content = "Купить BDOA";
                        BbdoaRun.IsEnabled = true;
                        CBbdoaVers.IsEnabled = true;
                    });
                    _s = State.NLic;
                }
                Thread.Sleep(500);
            }
        }
        State _s = State.NLic;
        enum State
        {
            NLic,
            On,
            Off
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }
        private void IClose_OnMouseEnter(object sender, MouseEventArgs e)
        {
            IClose.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.closea);
        }
        private void IClose_OnMouseLeave(object sender, MouseEventArgs e)
        {
            IClose.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.close);
        }
        private void IClose_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }
        private bool clicado = false;
        private System.Drawing.Point lm = new System.Drawing.Point();
        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            clicado = true;
            Imports.GetCursorPos(out lm);
        }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            clicado = false;
        }

        private void UIElement_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (clicado)
            {
                System.Drawing.Point pos;
                Imports.GetCursorPos(out pos);
                Left += (pos.X - lm.X);
                Top += (pos.Y - lm.Y);
                lm = pos;
            }
        }
    }
}
