using System;
using System.Diagnostics;
using System.Drawing;

namespace BDOA
{
    internal class BdoWindow
    { 
        internal Rectangle Filtr;
        internal Rectangle Refr;
        internal Rectangle Cap;
        internal Rectangle LCaptcha;
        internal Rectangle RCaptcha;
        internal Rectangle CountF;
        internal Rectangle Min;
        internal Rectangle CaptchaF;
        internal Rectangle Back;
        internal Rectangle PCap;
        internal bool count;
        private readonly Mask _mcap = new Mask(Properties.Resources.cap);
        private readonly Mask _mfiltr = new Mask(Properties.Resources.filtr);
        private readonly Mask _mmin = new Mask(Properties.Resources.min);
        private readonly Mask _mrefr = new Mask(Properties.Resources.refr);

        internal int X;
        internal int Y;
        internal int CX;
        internal int CY;
        internal int CAX;
        internal int CAY;
        internal int Width;
        internal int Height;
        private IntPtr _mwh;
        internal WindowState Ws;

        internal enum WindowState
        {
            None,
            Main,
            Lots,
            Buying
        }
        
        internal BdoWindow()
        {
            if (!ProcessCheck())
                Process.GetCurrentProcess().Kill();
            RectRefresh();
        }

        private void RectRefresh()
        {
            var r = Imports.NativeMethods.GetAbsoluteClientRect(_mwh);
            X = r.X;
            Y = r.Y;
            CX = r.Width / 2;
            CY = r.Height / 2;
            CAX = X + r.Width / 2;
            CAY = Y + r.Height / 2;
            Width = r.Width;
            Height = r.Height;
            Filtr = new Rectangle(CAX - 409, CAY - 216, 38, 10);
            //Filtr = new Rectangle(0, 0, 1680, 1050);
            Refr = new Rectangle(CAX + 95, CAY + 231, 56, 10);
            PCap = new Rectangle(CAX - 140, CAY + 15, 90, 115);
            Back = new Rectangle(CAX - 200, CAY + 235, 90, 115);
        }

        internal WindowState WindState(DxScreenCapture sc)
        {
            var b = new Sbmp(sc);
            if (_mfiltr.Compare(b, Filtr))
            {
                if (_mrefr.Compare(b, Refr))
                {
                    Point pt;
                    if (_mcap.Compare(b, PCap, out pt))
                    {
                        Cap = new Rectangle(pt.X, pt.Y, 41, 6);
                        LCaptcha = new Rectangle(Cap.X, Cap.Y - 49, 85, 33);
                        RCaptcha = new Rectangle(Cap.X + 85, Cap.Y - 49, 85, 33);
                        Min = new Rectangle(Cap.X + 245, Cap.Y - 119, 0, 0);
                        CountF = new Rectangle(Cap.X + 166, Cap.Y - 105, 0, 0);
                        CaptchaF = new Rectangle(Cap.X + 205, Cap.Y - 33, 0, 0);
                        count = _mmin.Compare(b, Min);
                        Ws = WindowState.Buying;
                    }
                    else
                        Ws = WindowState.Lots;
                }
                else
                    Ws = WindowState.Main;
            }
            else
                Ws = WindowState.None;
            return Ws;
        }
        private bool ProcessCheck()
        {
            var p = Process.GetProcessesByName("BlackDesert64");
            
            switch (p.Length)
            {
                case 1:
                    _mwh = p[0].MainWindowHandle;
                    return true;
                case 0:
                    p = Process.GetProcessesByName("BlackDesert32");
                    if (p.Length == 1)
                    {
                        _mwh = p[0].MainWindowHandle;
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        public bool Active
        {
            get
            {
                if (!ProcessCheck())
                    Process.GetCurrentProcess().Kill();//throw new Exception("BDO is not running!");
                RectRefresh();
                return Imports.GetForegroundWindow() == _mwh;
            }
            set
            {
                Imports.SetForegroundWindow(_mwh);
            }
        }
    }
}
