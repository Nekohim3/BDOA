using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Interceptor;

namespace BDOL
{
    internal class DControl
    {
        private int Pmd;
        private int Pmu;
        private int Pkd;
        private int Pku;
        private int Speed;
        internal Input _is;
        //private InputSimulator _is;
        internal DControl(int speed)
        {
            _is = new Input {KeyboardFilterMode = KeyboardFilterMode.All};
            _is.Load();
            Sett(speed);
        }

        internal void Sett(int speed)
        {
            Speed = 10 - speed;
            Pmd = 65 + Speed * 10;
            Pmu = 65 + Speed * 10;
            Pkd = 65 + Speed * 10;
            Pku = 65 + Speed * 10;
        }
        internal void ThSleep(int min, int max) => Thread.Sleep(new Random().Next(min, max));

        internal void SInput(string s)
        {
            foreach (var q in s)
                if (Speed < 5)
                    _is.SendKeys((Interceptor.Keys)Enum.Parse(typeof(Interceptor.Keys), "Numpad" + q));//_is.Keyboard.KeyPress((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), "VK_" + q));
                else
                    KPress((Interceptor.Keys)Enum.Parse(typeof(Interceptor.Keys), "Numpad" + q));
        }
        internal void MlPress()
        {
            _is.SendMouseEvent(MouseState.LeftDown);//_is.Mouse.LeftButtonDown();
            ThSleep(Pmd - 5, Pmd + 5);
            _is.SendMouseEvent(MouseState.LeftUp);//_is.Mouse.LeftButtonUp();
            ThSleep(Pmu - 5, Pmu + 5);
        }
        internal void KPress(Interceptor.Keys vk, bool qwe = false)
        {
            if (Speed > 5 && !qwe)
            {
                _is.SendKey(vk, KeyState.Down); //_is.Keyboard.KeyDown(vk);
                ThSleep(Pkd - 5, Pkd + 5);
                _is.SendKey(vk, KeyState.Up); //_is.Keyboard.KeyUp(vk);
                ThSleep(Pku - 5, Pku + 5);
            }
            else
                _is.SendKey(vk);
        }

        internal void MlMPress(int tx, int ty)
        {
            MMove(tx, ty);
            MlPress();
        }
        internal void MMove(int tx, int ty)
        {
            System.Drawing.Point pt;
            Imports.GetCursorPos(out pt);
            var fx = pt.X;
            var fy = pt.Y;
            var r = new Random();
            while (Math.Abs(fx - tx) > 11 || Math.Abs(fy - ty) > 11)
            {
                if (Math.Abs(fx - tx) > 11)
                {
                    if (tx > fx)
                        fx += r.Next(10, 20);
                    else
                        fx -= r.Next(10, 20);
                }
                if (Math.Abs(fy - ty) > 11)
                {
                    if (ty > fy)
                        fy += r.Next(10, 20);
                    else
                        fy -= r.Next(10, 20);
                }
                _is.MoveMouseTo(fx, fy);//Imports.SetCursorPos(fx, fy);
                if (Speed < 5)
                    ThSleep(1, 1);
                else
                    ThSleep(1, Speed - 4);
            }
        }
    }
}
