using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Interceptor;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace BDOL
{
    /// <summary>
    /// Логика взаимодействия для TestW.xaml
    /// </summary>
    public partial class TestW
    {
        public TestW()
        {
            InitializeComponent();
        }

        Thread th;
        private void TestW_OnLoaded(object sender, RoutedEventArgs e)
        {
            IClose.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.close);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            _sc = new DxScreenCapture();
            _is = new Input();
            _is = new Input { KeyboardFilterMode = KeyboardFilterMode.All };
            _is.Load();
            Start();
            th = new Thread(Check);
            th.Start();
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
        private void Start()
        {
            _mMouseListener = new MouseHookListener(new GlobalHooker()) { Enabled = true };
            _mMouseListener.MouseDownExt += AMouseDown;
            _mMouseListener.MouseMove += MMouseListenerOnMouseMove;
            _mKeyListener = new KeyboardHookListener(new GlobalHooker()) { Enabled = true };
            _mKeyListener.KeyDown += AKeyDown;
        }
        private void MMouseListenerOnMouseMove(object sender, MouseEventArgs e)
        {
            p3 = true;
            Dispatcher.Invoke(() =>
            {
                P3.Content = "[Подготовка] Подвигайте мышкой...OK";
                P3.Foreground = new SolidColorBrush(Color.FromArgb(255, 20, 150, 20));
            });
        }
        private void AMouseDown(object sender, MouseEventArgs e)
        {
            p2 = true;
            Dispatcher.Invoke(() =>
            {
                P2.Content = "[Подготовка] Нажмите клавишу мыши...OK";
                P2.Foreground = new SolidColorBrush(Color.FromArgb(255, 20, 150, 20));
            });
        }
        private void AKeyDown(object sender, KeyEventArgs e)
        {
            p1 = true;
            Dispatcher.Invoke(() =>
            {
                P1.Content = "[Подготовка] Нажмите клавишу клавиатуры...OK";
                P1.Foreground = new SolidColorBrush(Color.FromArgb(255, 20, 150, 20));
            });
        }
        private void Deactivate()
        {
            _mMouseListener.Dispose();
            _mKeyListener.Dispose();
        }

        bool p1;
        bool p2;
        bool p3;
        bool focus;
        private KeyboardHookListener _mKeyListener;
        private MouseHookListener _mMouseListener;
        private Process  BdoP;
        DxScreenCapture _sc;
        private readonly Mask _mfiltr = new Mask(Properties.Resources.filtr);
        internal Input _is;
        private int time = 5000;
        private bool Prepare()
        {
            Dispatcher.Invoke(() =>
            {
                P1.Content = "[Подготовка] Нажмите клавишу клавиатуры...";
                P1.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 20));
                P2.Content = "[Подготовка] Нажмите клавишу мыши...";
                P2.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 20));
                P3.Content = "[Подготовка] Подвигайте мышкой...";
                P3.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 20));
            });
            var sw = new Stopwatch();
            sw.Start();
            while ((!p1 || !p2 || !p3) && sw.ElapsedTicks < TimeSpan.FromMilliseconds(time).Ticks)
            {

                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                    new Action(delegate { }));
            }
            sw.Stop();
            if (p1)
            {
                Dispatcher.Invoke(() =>
                {
                    P1.Content = "[Подготовка] Нажмите клавишу клавиатуры...OK";
                    P1.Foreground = new SolidColorBrush(Color.FromArgb(255, 20, 150, 20));
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    P1.Content = "[Подготовка] Нажмите клавишу клавиатуры...FAILED";
                    P1.Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 20, 20));
                });

            }
            if (p2)
            {
                Dispatcher.Invoke(() =>
                {
                    P2.Content = "[Подготовка] Нажмите клавишу мыши...OK";
                    P2.Foreground = new SolidColorBrush(Color.FromArgb(255, 20, 150, 20));
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    P2.Content = "[Подготовка] Нажмите клавишу мыши...FAILED";
                    P2.Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 20, 20));
                });
            }
            if (p3)
            {
                Dispatcher.Invoke(() =>
                {
                    P3.Content = "[Подготовка] Подвигайте мышкой...OK";
                    P3.Foreground = new SolidColorBrush(Color.FromArgb(255, 20, 150, 20));
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    P3.Content = "[Подготовка] Подвигайте мышкой...FAILED";
                    P3.Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 20, 20));
                });
            }
            return p1 && p2 && p3;
        }
        private void Check()
        {
            Dispatcher.Invoke(() =>
            {
                Topmost = true;
                Top = 0;
                Left = 0;
            });
            if (!Prepare()) return;
            Dispatcher.Invoke(() =>
            {
                Topmost = true;
                Top = 0;
                Left = 0;
                C1.Content = "Проверка процесса...";
                C1.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 20));
            });
            var ch1 = Ch1();
            if (ch1)
            {
                Dispatcher.Invoke(() =>
                {
                    C1.Content = "Проверка процесса...OK";
                    C1.Foreground = new SolidColorBrush(Color.FromArgb(255, 20, 150, 20));
                    C2.Content = "Проверка активного процесса...";
                    C2.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 20));
                });
                if (Ch2())
                {
                    Dispatcher.Invoke(() =>
                    {
                        C2.Content = "Проверка активного процесса...OK";
                        C2.Foreground = new SolidColorBrush(Color.FromArgb(255, 20, 150, 20));
                        C3.Content = "Проверка DirectX...";
                        C3.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 20));
                    });
                    if (Ch3())
                    {
                        Dispatcher.Invoke(() =>
                        {
                            C3.Content = "Проверка DirectX...OK";
                            C3.Foreground = new SolidColorBrush(Color.FromArgb(255, 20, 150, 20));
                            C4.Content = "Проверка игровых настроек...";
                            C4.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 20));
                        });
                        if (Ch4())
                        {
                            Dispatcher.Invoke(() =>
                            {
                                C4.Content = "Проверка игровых настроек...OK";
                                C4.Foreground = new SolidColorBrush(Color.FromArgb(255, 20, 150, 20));
                                C5.Content = "Проверка драйвера [Движение мыши]...";
                                C5.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 20));
                            });
                            if (Ch5())
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    C5.Content = "Проверка драйвера [Движение мыши]...OK";
                                    C5.Foreground = new SolidColorBrush(Color.FromArgb(255, 20, 150, 20));
                                    C6.Content = "Проверка драйвера [Нажатие мыши]...";
                                    C6.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 20));
                                });
                                if (Ch6())
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        C6.Content = "Проверка драйвера [Нажатие мыши]...OK";
                                        C6.Foreground = new SolidColorBrush(Color.FromArgb(255, 20, 150, 20));
                                        C7.Content = "Проверка драйвера [Нажатие клавиатуры]...";
                                        C7.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 20));
                                    });
                                    if (Ch7())
                                    {
                                        Dispatcher.Invoke(() =>
                                        {
                                            C7.Content = "Проверка драйвера [Нажатие клавиатуры]...OK";
                                            C7.Foreground = new SolidColorBrush(Color.FromArgb(255, 20, 150, 20));
                                        });
                                    }
                                    else
                                    {
                                        Dispatcher.Invoke(() =>
                                        {
                                            C7.Content = "Проверка драйвера [Нажатие клавиатуры]...FAILED";
                                            C7.Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 20, 20));
                                        });
                                    }
                                }
                                else
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        C6.Content = "Проверка драйвера [Нажатие мыши]...FAILED";
                                        C6.Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 20, 20));
                                    });
                                }
                            }
                            else
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    C5.Content = "Проверка драйвера [Движение мыши]...FAILED";
                                    C5.Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 20, 20));
                                });
                            }
                        }
                        else
                        {
                            Dispatcher.Invoke(() =>
                            {
                                C4.Content = "Проверка DirectX...FAILED";
                                C4.Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 20, 20));
                            });
                        }
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            C3.Content = "Проверка DirectX...FAILED";
                            C3.Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 20, 20));
                        });
                    }
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        C2.Content = "Проверка активного процесса...FAILED";
                        C2.Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 20, 20));
                    });
                }
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    C1.Content = "Проверка процесса...FAILED";
                    C1.Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 20, 20));
                });
            }
            Deactivate();
            _sc.Dispose();
            _is.Unload();
        }
        private Rectangle AbsC(UIElement e)
        {
            var p = new System.Windows.Point(0, 0);
            Dispatcher.Invoke(() => 
            {
                p = e.PointToScreen(new System.Windows.Point(0, 0));
            });
            return new Rectangle((int)p.X, (int)p.Y, (int)e.RenderSize.Width, (int)e.RenderSize.Height);
        }
        private bool Ch1()
        {
            var res = false;
            var sw = new Stopwatch();
            sw.Start();
            while (!res && sw.ElapsedTicks < TimeSpan.FromMilliseconds(time).Ticks)
            {
                var P = Process.GetProcessesByName("BlackDesert64");
                if (P.Length != 0)
                {
                    res = true;
                    BdoP = P[0];
                    break;
                }
                P = Process.GetProcessesByName("BlackDesert32");
                if (P.Length != 0)
                {
                    res = true;
                    BdoP = P[0];
                    break;
                }
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }
            sw.Stop();
            return res;
        }
        private bool Ch2()
        {
            var res = false;
            var sw = new Stopwatch();
            sw.Start();
            while (!res && sw.ElapsedTicks < TimeSpan.FromMilliseconds(time).Ticks)
            {
                if (Imports.GetForegroundWindow() == BdoP.MainWindowHandle)
                {
                    res = true;
                    break;
                }
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }
            sw.Stop();
            return res;
        }
        private bool Ch3()
        {
            var res = false;
            var sw = new Stopwatch();
            sw.Start();
            while (!res && sw.ElapsedTicks < TimeSpan.FromMilliseconds(time).Ticks)
            {
                var s = new Sbmp(_sc);
                if (s.GetPixel(0, 0).A != 0)
                {
                    res = true;
                    s.Dispose();
                    break;
                }
                s.Dispose();
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }
            sw.Stop();
            return res;
        }
        private bool Ch4()
        {
            var res = false;
            var sw = new Stopwatch();
            var r = Imports.NativeMethods.GetAbsoluteClientRect(BdoP.MainWindowHandle);
            var X = r.X;
            var Y = r.Y;
            var CAX = X + r.Width / 2;
            var CAY = Y + r.Height / 2;
            sw.Start();
            while (!res && sw.ElapsedTicks < TimeSpan.FromMilliseconds(time).Ticks)
            {
                var b = new Sbmp(_sc);
                
                var Filtr = new System.Drawing.Rectangle(CAX - 409, CAY - 216, 38, 10);
                if (_mfiltr.Compare(b, Filtr))
                {
                    res = true;
                    b.Dispose();
                    break;
                }
                b.Dispose();
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }
            sw.Stop();
            return res;
        }
        private bool Ch5()
        {
            Imports.SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
            _is.MoveMouseTo(0, 0);
            Point pt;
            Imports.GetCursorPos(out pt);
            if (pt.X != 0 || pt.Y != 0)
                return false;
            var r = AbsC(TBtest);
            _is.MoveMouseTo(r.X + r.Width / 2, r.Y + r.Height / 2);
            Imports.GetCursorPos(out pt);
            return pt.X == r.X + r.Width / 2 && pt.Y == r.Y + r.Height / 2;
        }
        private bool Ch6()
        {
            var sw = new Stopwatch();
            sw.Start();
            while (!focus && sw.ElapsedTicks < TimeSpan.FromMilliseconds(time).Ticks)
            {
                _is.SendLeftClick();
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }
            sw.Stop();
            return focus;
        }
        private bool Ch7()
        {
            var sw = new Stopwatch();
            sw.Start();
            var str = "";
            while (!(str.ToLower() == "test" || str.ToLower() == "еуые") && sw.ElapsedTicks < TimeSpan.FromMilliseconds(time).Ticks)
            {
                Dispatcher.Invoke(() => { TBtest.Text = ""; });
                _is.SendKeys(Interceptor.Keys.T);
                _is.SendKeys(Interceptor.Keys.E);
                _is.SendKeys(Interceptor.Keys.S);
                _is.SendKeys(Interceptor.Keys.T);
                Thread.Sleep(50);
                Dispatcher.Invoke(() => { str = TBtest.Text; });
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }
            sw.Stop();
            return str.ToLower() == "test" || str.ToLower() == "еуые";
        }
        private void TBtest_OnGotFocus(object sender, RoutedEventArgs e)
        {
            focus = true;
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            th.Abort();
            Close();
        }

        private void IClose_OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            IClose.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.closea);
        }
        private void IClose_OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            IClose.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.close);
        }
        private void IClose_OnPreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }
        private bool clicado = false;
        private System.Drawing.Point lm = new System.Drawing.Point();
        private void UIElement_OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            clicado = true;
            Imports.GetCursorPos(out lm);
        }

        private void UIElement_OnMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            clicado = false;
        }

        private void UIElement_OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
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
