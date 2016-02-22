using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using SlimDX.Direct3D9;
using Application = System.Windows.Application;

namespace BDOA
{
    internal class DxScreenCapture
    {
        private readonly Device _d;
        private readonly Surface _s;
        private byte[] _buf;
        private Thread _th;
        internal bool _slow = false;
        private int _stime = 500;
        bool ready = false;
        internal DxScreenCapture(int stime = 500)
        {
            _stime = stime;
            var presentParams = new PresentParameters
            {
                Windowed = true,
                SwapEffect = SwapEffect.Discard
            };
            _d = new Device(new Direct3D(), 0, DeviceType.Hardware, IntPtr.Zero,
                CreateFlags.SoftwareVertexProcessing, presentParams);
            _s = Surface.CreateOffscreenPlain(_d, Screen.PrimaryScreen.Bounds.Width,
                Screen.PrimaryScreen.Bounds.Height, Format.A8R8G8B8, Pool.Scratch);
            _th = new Thread(Refresh);
            _th.Start();
        }

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private object _o = new object();

        private void Refresh()
        {
            while (true)
            {
                _d.GetFrontBufferData(0, _s);
                var dr = _s.LockRectangle(LockFlags.None);
                ready = false;
                _buf = new byte[dr.Data.Length];
                dr.Data.Read(_buf, 0, (int) dr.Data.Length);
                ready = true;
                _s.UnlockRectangle();
                dr.Data.Dispose();
                if (_slow)
                    Thread.Sleep(_stime);
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }
        }

        internal byte[] GetImg()
        {
            while (!ready) ;
            return _buf.ToArray();
        }

        public void Dispose()
        {
            _th.Abort();
            _buf = null;
            _s.Dispose();
            _d.Dispose();
        }
    }
}
