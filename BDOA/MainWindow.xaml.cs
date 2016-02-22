using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using static System.Environment;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using Keys = System.Windows.Forms.Keys;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxOptions = System.Windows.Forms.MessageBoxOptions;
using Point = System.Windows.Point;

namespace BDOA
{
    public partial class MainWindow
    {
        private readonly Neuro _tr = new Neuro();
        private BdoWindow _bdoWindow;
        private readonly List<AItem> _bitems = new List<AItem>();
        private bool _bmax;
        private BState _bstate = BState.Stop;
        private DControl _dControl;
        private bool _exec;
        private bool _getImg;
        private License _lic;
        private bool _listByuing;
        private KeyboardHookListener _mKeyListener;
        private MouseHookListener _mMouseListener;
        private DxScreenCapture _sc;
        private int _speed = 10;
        private readonly Mask _mbuyed = new Mask(Properties.Resources.buyed);
        private State _state = State.On;

        public MainWindow()
        {
            InitializeComponent();
        }

        private int GetLotPos(int x, int y)
        {
            if (x > _bdoWindow.CAX - 280 && x < _bdoWindow.CAX + 409 &&
                y > _bdoWindow.CAY - 165 + 0*62 && y < _bdoWindow.CAY - 106 + 0*62)
                return 0;
            if (x > _bdoWindow.CAX - 280 && x < _bdoWindow.CAX + 409 &&
                y > _bdoWindow.CAY - 165 + 1*62 && y < _bdoWindow.CAY - 106 + 1*62)
                return 1;
            if (x > _bdoWindow.CAX - 280 && x < _bdoWindow.CAX + 409 &&
                y > _bdoWindow.CAY - 165 + 2*62 && y < _bdoWindow.CAY - 106 + 2*62)
                return 2;
            if (x > _bdoWindow.CAX - 280 && x < _bdoWindow.CAX + 409 &&
                y > _bdoWindow.CAY - 165 + 3*62 && y < _bdoWindow.CAY - 106 + 3*62)
                return 3;
            if (x > _bdoWindow.CAX - 280 && x < _bdoWindow.CAX + 409 &&
                y > _bdoWindow.CAY - 165 + 4*62 && y < _bdoWindow.CAY - 106 + 4*62)
                return 4;
            if (x > _bdoWindow.CAX - 280 && x < _bdoWindow.CAX + 409 &&
                y > _bdoWindow.CAY - 165 + 5*62 && y < _bdoWindow.CAY - 106 + 5*62)
                return 5;
            if (x > _bdoWindow.CAX - 280 && x < _bdoWindow.CAX + 409 &&
                y > _bdoWindow.CAY - 165 + 6*62 && y < _bdoWindow.CAY - 106 + 6*62)
                return 6;
            return -1;
        }

        private static Rectangle AbsC(UIElement e)
        {
            var p = e.PointToScreen(new Point(0, 0));
            return new Rectangle((int) p.X, (int) p.Y, (int) e.RenderSize.Width, (int) e.RenderSize.Height);
        }

        private void MMouseListenerOnMouseMove(object sender, MouseEventArgs e)
        {
            if (_bdoWindow.Ws != BdoWindow.WindowState.Main || _state != State.On) return;
            if (AbsC(Gmax).Contains(e.X, e.Y))
            {
                Imgmax.Source = ImgHelpers.BitmapToImageSource(Properties.Resources._61);
                Tt1.Visibility = Visibility.Visible;
            }
            else
            {
                Imgmax.Source = ImgHelpers.BitmapToImageSource(Properties.Resources._6);
                Tt1.Visibility = Visibility.Hidden;
            }
            if (new Rectangle(AbsC(Gsp).X, AbsC(Gsp).Y, AbsC(Gsp).Width - 50, AbsC(Gsp).Height).Contains(e.X, e.Y))
            {
                Imgsp.Source = ImgHelpers.BitmapToImageSource(Properties.Resources._81);
                Tt2.Visibility = Visibility.Visible;
                Tt3.Visibility = Visibility.Hidden;
            }
            else if (new Rectangle(AbsC(Gsp).X + 50, AbsC(Gsp).Y, AbsC(Gsp).Width - 50, AbsC(Gsp).Height).Contains(e.X,
                e.Y))
            {
                Imgsp.Source = ImgHelpers.BitmapToImageSource(Properties.Resources._82);
                Tt2.Visibility = Visibility.Hidden;
                Tt3.Visibility = Visibility.Visible;
            }
            else
            {
                Imgsp.Source = ImgHelpers.BitmapToImageSource(Properties.Resources._8);
                Tt2.Visibility = Visibility.Hidden;
                Tt3.Visibility = Visibility.Hidden;
            }
            Tt4.Visibility = AbsC(IDResume).Contains(e.X, e.Y) ? Visibility.Visible : Visibility.Hidden;
            Tt5.Visibility = AbsC(IResume).Contains(e.X, e.Y) ? Visibility.Visible : Visibility.Hidden;
            Tt6.Visibility = AbsC(IStop).Contains(e.X, e.Y) ? Visibility.Visible : Visibility.Hidden;
            Tt7.Visibility = AbsC(IDela).Contains(e.X, e.Y) ? Visibility.Visible : Visibility.Hidden;
            Tt8.Visibility = AbsC(IAddUn).Contains(e.X, e.Y) ? Visibility.Visible : Visibility.Hidden;
            if (new Rectangle(_bdoWindow.CAX - 34, _bdoWindow.CAY - 219, 52, 30).Contains(e.X, e.Y))
            {
                Gmax.Visibility = Visibility.Hidden;
                Gsp.Visibility = Visibility.Hidden;
            }
            else
            {
                Gmax.Visibility = Visibility.Visible;
                Gsp.Visibility = Visibility.Visible;
            }
        }

        private void AMouseDown(object sender, MouseEventExtArgs e)
        {
            if (_bdoWindow.Ws != BdoWindow.WindowState.Main || _getImg || _state != State.On) return;
            if (_exec)
                return;
            if (AbsC(Gmax).Contains(e.X, e.Y))
            {
                _bmax = !_bmax;
                Emax.Fill = _bmax
                    ? new SolidColorBrush(Color.FromArgb(255, 0x33, 0xcc, 0x33))
                    : new SolidColorBrush(Color.FromArgb(255, 0xcc, 0x33, 0x33));
            }
            if (new Rectangle(AbsC(Gsp).X, AbsC(Gsp).Y, AbsC(Gsp).Width - 50, AbsC(Gsp).Height).Contains(e.X, e.Y))
            {
                if (_speed > 0)
                    _speed--;
                Dispatcher.Invoke(() => { Lbsp.Content = _speed.ToString(); });
                _dControl.Sett(_speed);
            }
            if (new Rectangle(AbsC(Gsp).X + 50, AbsC(Gsp).Y, AbsC(Gsp).Width - 50, AbsC(Gsp).Height).Contains(e.X, e.Y))
            {
                if (_speed < 11)
                    _speed++;
                Dispatcher.Invoke(() => { Lbsp.Content = _speed.ToString(); });
                _dControl.Sett(_speed);
            }
            if (AbsC(Items).Contains(e.X, e.Y))
            {
                for (var i = 0; i < _bitems.Count; i++)
                    if (AbsC(_bitems[i].ImgX).Contains(e.X, e.Y))
                        DelItem(i);
            }
            if (AbsC(IResume).Contains(e.X, e.Y))
            {
                Dispatcher.Invoke(() =>
                {
                    IResume.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.resumeg);
                    IDResume.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.delres);
                    IStop.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.delall);
                });
                _bstate = BState.Res;
            }
            if (AbsC(IDResume).Contains(e.X, e.Y))
            {
                Dispatcher.Invoke(() =>
                {
                    IResume.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.resume);
                    IDResume.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.delresg);
                    IStop.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.delall);
                });
                _bstate = BState.DelRes;
            }
            if (AbsC(IStop).Contains(e.X, e.Y))
            {
                Dispatcher.Invoke(() =>
                {
                    IResume.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.resume);
                    IDResume.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.delres);
                    IStop.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.delallg);
                });
                _bstate = BState.Stop;
            }
            if (AbsC(IDela).Contains(e.X, e.Y))
            {
                DelAllItem();
            }
            if (AbsC(IStartBack).Contains(e.X, e.Y))
            {
                if (_listByuing)
                {
                    _listByuing = false;
                    Dispatcher.Invoke(() =>
                    {
                        LABSS.Content = "[Выкл]";
                        LABSS.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xee,
                            0x22, 0x22));
                        IStartBack.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.startback);
                    });
                }
                else
                {
                    _listByuing = true;
                    new Thread(() => { BuyingProc(); }).Start();
                    Dispatcher.Invoke(() =>
                    {
                        LABSS.Content = "[Вкл]";
                        LABSS.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0x22,
                            0xcc, 0x22));
                        IStartBack.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.startbackp);
                    });
                }
            }
            if (AbsC(IAddUn).Contains(e.X, e.Y))
            {
                AddItem();
            }
            var pos = GetLotPos(e.X, e.Y);
            if (pos == -1) return;
            if (e.Button == MouseButtons.Left)
            {
                e.Handled = true;
                new Thread(() => { Exec(pos); }).Start();
            }
            if (e.Button != MouseButtons.Right) return;
            e.Handled = true;
            new Thread(() => { AddItem(GetItemImg(pos)); }).Start();
        }
        
        private void AMouseUp(object sender, MouseEventArgs e)
        {
        }

        private void AMouseWheel(object sender, MouseEventArgs e)
        {
            if (_bdoWindow.Ws != BdoWindow.WindowState.Main || _getImg || _state != State.On) return;
            if (!AbsC(Items).Contains(e.X, e.Y)) return;
            if ((e.Delta < 0 && _bitems.Last().G.Margin.Top > Items.ActualHeight - 50) ||
                (e.Delta > 0 && _bitems.First().G.Margin.Top < 5))
            {
                foreach (var q in _bitems)
                    q.G.Margin = new Thickness(q.G.Margin.Left, q.G.Margin.Top + (e.Delta > 0 ? 31 : -31), 0, 0);
            }
        }

        private void AKeyUp(object sender, KeyEventArgs e)
        {
        }
        
        private void AKeyDown(object sender, KeyEventArgs e)
        {
            if (_bdoWindow.Active)
            {
                if (e.KeyCode == Keys.Insert)
                {
                    if (_state == State.On)
                    {
                        _state = State.Off;
                        Dispatcher.Invoke(() =>
                        {
                            LABS.Content = "[Выкл]";
                            LABS.Foreground = new SolidColorBrush(Color.FromArgb(0xcc, 0xcc,
                                0x22, 0x22));
                        });
                        Interface();
                    }
                    else
                    {
                        _state = State.On;
                        Dispatcher.Invoke(() =>
                        {
                            LABS.Content = "[Вкл]";
                            LABS.Foreground = new SolidColorBrush(Color.FromArgb(0xcc, 0x22,
                                0xcc, 0x22));
                        });
                        Interface();
                    }
                }
                if (e.KeyCode == Keys.Home)
                {
                    if (_state != State.Hide)
                    {
                        _state = State.Hide;
                        Interface();
                    }
                    else
                    {
                        _state = State.On;
                        Dispatcher.Invoke(() =>
                        {
                            LABS.Content = "[Вкл]";
                            LABS.Foreground = new SolidColorBrush(Color.FromArgb(0xcc, 0x22,
                                0xcc, 0x22));
                        });
                        Interface();
                    }
                }
                if (e.KeyCode == Keys.Delete)
                {
                    _listByuing = false;
                    //Process.GetCurrentProcess().Kill();
                }
            }
            if (_bdoWindow.Ws == BdoWindow.WindowState.Main)
            {
                if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1)
                {
                    new Thread(() => { Exec(0); }).Start();
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2)
                {
                    new Thread(() => { Exec(1); }).Start();
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3)
                {
                    new Thread(() => { Exec(2); }).Start();
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.D4 || e.KeyCode == Keys.NumPad4)
                {
                    new Thread(() => { Exec(3); }).Start();
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.D5 || e.KeyCode == Keys.NumPad5)
                {
                    new Thread(() => { Exec(4); }).Start();
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.D6 || e.KeyCode == Keys.NumPad6)
                {
                    new Thread(() => { Exec(5); }).Start();
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.D7 || e.KeyCode == Keys.NumPad7)
                {
                    new Thread(() => { Exec(6); }).Start();
                    e.Handled = true;
                }
            }
        }

        private Sbmp GetItemImg(int pos)
        {
            _getImg = true;
            _dControl.MlMPress(_bdoWindow.CAX - 150, _bdoWindow.CAY - 136 + pos*62);
            var b = Wait(BdoWindow.WindowState.Lots)
                ? new Sbmp(_sc).Crop(new Rectangle(_bdoWindow.CAX - 285, _bdoWindow.CAY - 235, 200, 60))
                : null;
            _dControl.MlMPress(_bdoWindow.Back.X, _bdoWindow.Back.Y);
            _getImg = false;
            return b;
        }

        private void AddItem(Sbmp b)
        {
            if (_listByuing)
                return;
            Dispatcher.Invoke(() =>
            {
                if (b != null)
                    if (!_bitems.Any(q => q.ImgS.Compare(b, new Rectangle(13, 12, 37, 37), 25, 100)))
                        _bitems.Add(new AItem(b, (int)Items.ActualWidth));
                BoardRefresh();
            });
        }

        private void AddItem()
        {
            if (_listByuing)
                return;
            Dispatcher.Invoke(() =>
            {
                if (!_bitems.Any(q => q._un))
                    _bitems.Add(new AItem());
                BoardRefresh();
            });
        }

        private void DelItem(int num)
        {
            _bitems.RemoveAt(num);
            BoardRefresh();
        }

        private void DelAllItem()
        {
            _bitems.Clear();
            BoardRefresh();
        }

        private void BoardRefresh()
        {
            Dispatcher.Invoke(() =>
            {
                Items.Children.Clear();
                for (var i = 0; i < _bitems.Count; i++)
                {
                    _bitems[i].G.Margin = new Thickness(0, i*62 + 5, 0, 0);
                    Items.Children.Add(_bitems[i].G);
                }
                if (_bitems.Count == 0)
                {
                    //TaskBoard.Visibility = Visibility.Hidden;
                    LABSS.Content = "[Выкл]";
                    LABSS.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xee,
                        0x22, 0x22));
                    IStartBack.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.startback);
                }
                else
                    TaskBoard.Visibility = Visibility.Visible;
                LItemsCount.Content = _bitems.Count.ToString();
            });
        }

        private void BuyingProc()
        {
            CheckLic();
            while (_bitems.Count != 0 && _listByuing && _state == State.On)
            {
                var b = new Sbmp(_sc);
                for (var i = 0; i < 7 && _listByuing; i++)
                {
                    try
                    {
                        for (var j = 0; j < _bitems.Count && _listByuing; j++)
                        {
                            var q = _bitems[j];
                            if (q._un)
                            {
                                var c = b.GetPixel(_bdoWindow.CAX - 270, _bdoWindow.CAY - 157 + i*62);
                                if (c.R == c.G || c.G == c.B) continue;
                                Exec(i);
                                Dispatcher.Invoke(() =>
                                {
                                    if (_bstate == BState.DelRes)
                                        DelItem(j);
                                    if (_bstate == BState.Stop)
                                        DelAllItem();
                                });
                                throw new InvalidOperationException();
                            }
                            else
                            {
                                if (
                                    !q.ImgS.Compare(b,
                                        new Rectangle(_bdoWindow.CAX - 267, _bdoWindow.CAY - 155 + i*62, 150, 37), 25,
                                        100))
                                    continue;
                                Exec(i);
                                //if (s == BuyMessage.Bought)
                                    Dispatcher.Invoke(() =>
                                    {
                                        if (_bstate == BState.DelRes)
                                            DelItem(j);
                                        if (_bstate == BState.Stop)
                                            DelAllItem();
                                    });
                                throw new InvalidOperationException();
                            }
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        break;
                    }
                }
            }
            _listByuing = false;
            Dispatcher.Invoke(() =>
            {
                LABSS.Content = "[Выкл]";
                LABSS.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xee,
                    0x22, 0x22));
                IStartBack.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.startback);
            });
        }

        private BuyMessage Exec(int pos)
        {
            CheckLic();
            var state = BuyMessage.None;
            if (_exec)
                return state;
            if (_bdoWindow.Ws == BdoWindow.WindowState.None)
                return state;
            if (_state != State.On)
                return state;
            _exec = true;
            if (Wait(BdoWindow.WindowState.Main))
            {
                _dControl.MlMPress(_bdoWindow.CAX - 150, _bdoWindow.CAY - 136 + pos * 62);
                if (Wait(BdoWindow.WindowState.Lots))
                {
                    _dControl.MlMPress(_bdoWindow.CAX + 360, _bdoWindow.CAY - 136);
                    if (Wait(BdoWindow.WindowState.Buying))
                    {
                        _dControl.SInput(GetCaptcha());
                        if (_bdoWindow.count && _bmax)
                        {
                            _dControl.MlMPress(_bdoWindow.CountF.X, _bdoWindow.CountF.Y);
                            _dControl.KPress(Interceptor.Keys.Nine); _dControl.ThSleep(35, 50);
                            _dControl.KPress(Interceptor.Keys.Eight); _dControl.ThSleep(35, 50);
                            _dControl.KPress(Interceptor.Keys.Seven); _dControl.ThSleep(35, 50);
                            _dControl.KPress(Interceptor.Keys.Enter);
                            _dControl.MlMPress(_bdoWindow.CaptchaF.X, _bdoWindow.CaptchaF.Y);
                        }
                        _dControl.KPress(Interceptor.Keys.Enter);
                        //state = WaitMess();
                        _dControl.ThSleep(50, 100);
                        if (!Wait(BdoWindow.WindowState.Lots))
                            _dControl.KPress(Interceptor.Keys.Escape);
                        _dControl.MlMPress(_bdoWindow.Back.X, _bdoWindow.Back.Y);
                        if (!Wait(BdoWindow.WindowState.Main))
                            _dControl.MlMPress(_bdoWindow.Back.X, _bdoWindow.Back.Y);
                    }
                    else
                        _dControl.MlMPress(_bdoWindow.Back.X, _bdoWindow.Back.Y);
                    if (!Wait(BdoWindow.WindowState.Main))
                        _dControl.MlMPress(_bdoWindow.Back.X, _bdoWindow.Back.Y);// _dControl.KPress(Interceptor.Keys.Backspace);
                }
            }
            _dControl.ThSleep(150, 250);
            _dControl.MMove(_bdoWindow.CAX + 450, _bdoWindow.CAY);
            _exec = false;
            return state;
        }

        private string GetCaptcha()
        {
            CheckLic();
            var b = new Sbmp(_sc);
            var bcl = b.Crop(_bdoWindow.LCaptcha);
            var bcr = b.Crop(_bdoWindow.RCaptcha);
            bcl = ImgHelpers.Threshold(bcl);
            bcr = ImgHelpers.Threshold(bcr);
            bcl = bcl.Crop(ImgHelpers.GetRect(bcl));
            bcr = bcr.Crop(ImgHelpers.GetRect(bcr));
            var s = _tr.Recognize(bcl);
            if (s == "-")
                bcl.GetBmp().Save(DateTime.Now.ToShortTimeString() + "l.bmp");
            s += _tr.Recognize(bcr);
            if (s[1] == '-')
                bcr.GetBmp().Save(DateTime.Now.ToShortTimeString() + "r.bmp");
            bcl.Dispose();
            bcr.Dispose();
            b.Dispose();
            return s;
        }

        //private BuyMessage WaitMess()
        //{
        //    var res = BuyMessage.None;
        //    var sw = new Stopwatch();
        //    sw.Start();
        //    while (res == BuyMessage.None && sw.ElapsedTicks < TimeSpan.FromMilliseconds(300).Ticks)
        //    {var b = new Sbmp(_sc);
        //        if(_mbuyed.Compare(b,_bdoWindow.CAX - 96, _bdoWindow.Y + 187))
        //            res = BuyMessage.Bought;
        //        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        //    }
        //    sw.Stop();
        //    return res;
        //}

        enum BuyMessage
        {
            None,
            Bought,
            NoMoney,
            CountError,
            Purchased,
            Random
        }
        private bool Wait(BdoWindow.WindowState ws)
        {
            var sw = new Stopwatch();
            sw.Start();
            while (ws != _bdoWindow.Ws && sw.ElapsedTicks < TimeSpan.FromMilliseconds(200).Ticks)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }
            sw.Stop();
            return ws == _bdoWindow.Ws;
        }

        private void Interface()
        {
            CheckLic();
            if (_bdoWindow.Ws == BdoWindow.WindowState.None || _state == State.Hide)
            {
                Dispatcher.Invoke(() => { Mg.Visibility = Visibility.Hidden; });
                return;
            }
            if (_state != State.On)
            {
                Dispatcher.Invoke(() => { TaskBoard.Visibility = Visibility.Hidden; });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    TaskBoard.Visibility = Visibility.Visible;
                });
            }
            if (_bdoWindow.Ws == BdoWindow.WindowState.Main)
            {
                Dispatcher.Invoke(() =>
                {
                    Width = _bdoWindow.Width;
                    Height = _bdoWindow.Height;
                    Left = _bdoWindow.X;
                    Top = _bdoWindow.Y;
                    label.Margin = new Thickness(0, Height - 50, 0, 0);

                    BDOAL.Margin = new Thickness(_bdoWindow.CX + 30, _bdoWindow.CY - 345, 0, 0);
                    LABS.Margin = new Thickness(BDOAL.Margin.Left + BDOAL.ActualWidth, _bdoWindow.CY - 345, 0, 0);

                    Mg.Visibility = Visibility.Visible;
                    Gmax.Visibility = Visibility.Visible;
                    Gsp.Visibility = Visibility.Visible;
                    Gmax.Margin = new Thickness(_bdoWindow.CX + 27, _bdoWindow.CY - 219, 0, 0);
                    Gsp.Margin = new Thickness(_bdoWindow.CX + 86, _bdoWindow.CY - 219, 0, 0);
                    Tt1.Margin = new Thickness(_bdoWindow.CX + 27 + 53, _bdoWindow.CY - 219, 0, 0);
                    Tt2.Margin = new Thickness(_bdoWindow.CX + 86 + 73, _bdoWindow.CY - 219, 0, 0);
                    Tt3.Margin = new Thickness(_bdoWindow.CX + 86 + 73, _bdoWindow.CY - 219, 0, 0);

                    Tt4.Margin = new Thickness(_bdoWindow.CX + 572 - 40 - Tt4.ActualWidth, _bdoWindow.CY - 286, 0, 0);
                    Tt5.Margin = new Thickness(_bdoWindow.CX + 617 - 40 - Tt5.ActualWidth, _bdoWindow.CY - 286, 0, 0);
                    Tt6.Margin = new Thickness(_bdoWindow.CX + 662 - 40 - Tt6.ActualWidth, _bdoWindow.CY - 286, 0, 0);
                    Tt7.Margin = new Thickness(_bdoWindow.CX + 720 - 40 - Tt7.ActualWidth, _bdoWindow.CY + 284, 0, 0);
                    Tt8.Margin = new Thickness(_bdoWindow.CX + 723 - 40 - Tt8.ActualWidth, _bdoWindow.CY - 286, 0, 0);
                    if (_state == State.On)
                    {
                        TaskBoard.Margin = new Thickness(_bdoWindow.CX + 450, _bdoWindow.CY - 344, 0, 0);
                        BoardRefresh();
                    }
                    if (Screen.PrimaryScreen.Bounds.Width < 1440)
                    {
                        TaskBoard.Width = 185;
                        GTitle.Margin = new Thickness(25, 0, 0, 0);
                        RFiltres.Height = 100;
                        IResume.Margin = new Thickness(72, 103, 0, 0);
                        IDResume.Margin = new Thickness(27, 103, 0, 0);
                        IStop.Margin = new Thickness(117, 103, 0, 0);

                        Tt4.Margin = new Thickness(_bdoWindow.CX + 476 - Tt4.ActualWidth, _bdoWindow.CY - 241, 0, 0);
                        Tt5.Margin = new Thickness(_bdoWindow.CX + 521 - Tt4.ActualWidth, _bdoWindow.CY - 241, 0, 0);
                        Tt6.Margin = new Thickness(_bdoWindow.CX + 566 - Tt4.ActualWidth, _bdoWindow.CY - 241, 0, 0);
                        Tt7.Margin = new Thickness(_bdoWindow.CX + 566 - Tt7.ActualWidth, _bdoWindow.CY + 284, 0, 0);
                        Tt8.Margin = new Thickness(_bdoWindow.CX + 566 - Tt8.ActualWidth, _bdoWindow.CY - 284, 0, 0);
                    }
                });
                return;
            }
            if (_bdoWindow.Ws == BdoWindow.WindowState.Lots)
            {
                Dispatcher.Invoke(() =>
                {
                    Width = _bdoWindow.Width;
                    Height = _bdoWindow.Height;
                    Left = _bdoWindow.X;
                    Top = _bdoWindow.Y;

                    Mg.Visibility = Visibility.Visible;
                    Gmax.Visibility = Visibility.Hidden;
                    Gsp.Visibility = Visibility.Hidden;
                });
                return;
            }
            if (_bdoWindow.Ws == BdoWindow.WindowState.Buying)
            {
                Dispatcher.Invoke(() =>
                {
                    Width = _bdoWindow.Width;
                    Height = _bdoWindow.Height;
                    Left = _bdoWindow.X;
                    Top = _bdoWindow.Y;

                    Mg.Visibility = Visibility.Visible;
                    Gmax.Visibility = Visibility.Hidden;
                    Gsp.Visibility = Visibility.Hidden;
                });
            }
        }

        private void IntScan()
        {
            while (true)
            {
                CheckLic();
                if (_bdoWindow.Active)
                {
                    if (_bdoWindow.Ws != _bdoWindow.WindState(_sc))
                    {
                        _sc._slow = false;
                        Interface();
                        //Dispatcher.Invoke(() => { label.Content = _bdoWindow.Ws.ToString(); });
                    }
                    else if (_bdoWindow.Ws == BdoWindow.WindowState.None)
                    {
                        _sc._slow = true;
                        Thread.Sleep(500);
                    }
                }
                else
                {
                    _bdoWindow.Ws = BdoWindow.WindowState.None;
                    _sc._slow = true;
                    Interface();
                    Thread.Sleep(500);
                    //Dispatcher.Invoke(() => { label.Content = _bdoWindow.Ws.ToString(); });
                }
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }
        }

        private void CheckLic()
        {
            if (_lic.GetLic()) return;
            _sc._slow = true;
            _exec = false;
            _listByuing = false;
            _state = State.Hide;
            DelAllItem();
            Deactivate();
            new MyForm().Msg();
            Process.Start("http://бдобот.рф/?page=buy&bot_id=" + _lic.GetHwid() + "&bot_type=1");
            Process.GetCurrentProcess().Kill();
        }

        private static void ArgCheck()
        {
            if (Process.GetCurrentProcess().ProcessName.IndexOf("host", StringComparison.Ordinal) != -1)
                return;
            var args = GetCommandLineArgs();
            if(args.Length == 1)
                Process.GetCurrentProcess().Kill();
            if (args[1] != GameNet.GetHwid())
                Process.GetCurrentProcess().Kill();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            ArgCheck();
            if (Process.GetProcessesByName("BDOA").Length > 1)
                Process.GetCurrentProcess().Kill();
            _lic = new License("http://angelhci.bget.ru/LBDOA.php");
            _bdoWindow = new BdoWindow();
            _sc = new DxScreenCapture();
            _dControl = new DControl(_speed);
            Imports.SetWindowLong(Process.GetCurrentProcess().MainWindowHandle, -20,
                Imports.GetWindowLong(Process.GetCurrentProcess().MainWindowHandle, -20) | 0x00000020);
            Start();
            CheckLic();
            new Thread(IntScan).Start();
            Imgmax.Source = ImgHelpers.BitmapToImageSource(Properties.Resources._6);
            Imgsp.Source = ImgHelpers.BitmapToImageSource(Properties.Resources._8);
            IResume.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.resume);
            IDResume.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.delres);
            IStop.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.delallg);
            IAddUn.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.addun);

            IStartBack.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.startback);
            IDela.Source = ImgHelpers.BitmapToImageSource(Properties.Resources.dela);

            RenderOptions.SetBitmapScalingMode(Imgmax, BitmapScalingMode.HighQuality);
            RenderOptions.SetBitmapScalingMode(Imgsp, BitmapScalingMode.HighQuality);
            RenderOptions.SetBitmapScalingMode(IResume, BitmapScalingMode.HighQuality);
            RenderOptions.SetBitmapScalingMode(IDResume, BitmapScalingMode.HighQuality);
            RenderOptions.SetBitmapScalingMode(IStop, BitmapScalingMode.HighQuality);
            RenderOptions.SetBitmapScalingMode(IStartBack, BitmapScalingMode.HighQuality);
            RenderOptions.SetBitmapScalingMode(IDela, BitmapScalingMode.HighQuality);
            RenderOptions.SetBitmapScalingMode(IAddUn, BitmapScalingMode.HighQuality);
            _state = State.Off;
            Dispatcher.Invoke(() =>
            {
                LABS.Content = "[Выкл]";
                LABS.Foreground = new SolidColorBrush(Color.FromArgb(0xcc, 0xcc,
                    0x22, 0x22));
            });
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

        private void MainWindow_OnDeactivated(object sender, EventArgs e)
        {
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Deactivate();
            Process.GetCurrentProcess().Kill();
        }

        private void Start()
        {
            _mMouseListener = new MouseHookListener(new GlobalHooker()) {Enabled = true};
            _mMouseListener.MouseDownExt += AMouseDown;
            _mMouseListener.MouseUp += AMouseUp;
            _mMouseListener.MouseWheel += AMouseWheel;
            _mMouseListener.MouseMove += MMouseListenerOnMouseMove;
            _mKeyListener = new KeyboardHookListener(new GlobalHooker()) {Enabled = true};
            _mKeyListener.KeyDown += AKeyDown;
            _mKeyListener.KeyUp += AKeyUp;
        }

        private void Deactivate()
        {
            _mMouseListener.Dispose();
            _mKeyListener.Dispose();
        }

        private enum BState
        {
            Stop,
            DelRes,
            Res
        }

        private enum State
        {
            On,
            Hide,
            Off
        }

        private class MyForm : Form
        {
            public void Msg()
            {
                MessageBox.Show(this, @"Лицензия неактивна. Для дальнейшего использование бота пожалуйста оплатите", "",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                    (MessageBoxOptions) 0x40000);
            }
        }
        
    }
}