using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Threading; 

namespace PlayBack
{
    class MouseInput
    {
        #region Unmanaged code import
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, int dwData, uint dwExtraInfo);
        #endregion

        private static readonly Rectangle bounds = Screen.GetBounds(Point.Empty);

        // constants for the mouse_input() API function
        private static readonly Dictionary<string, uint> inputMap = new Dictionary<string, uint>()
        {
            {"move", 0x0001 | 0x8000},
            {"Left", 0x0002},
            {"upLeft", 0x0004},
            {"Right", 0x0008},
            {"upRight", 0x0010},
            {"Middle", 0x0020},
            {"upMiddle", 0x0040},
            {"detent", 0x0800},
            {"doubleclickLeft", 0x0002 | 0x0004},
            {"doubleclickRight", 0x0008 | 0x0010},
            {"doubleclickMiddle", 0x0020 | 0x0040}
        };


        //Change to microsoft provided coordinate system
        static Func<int, int, int> map = (i, j) =>
            {
                return (int)((((i) * 65535) / (double)j) + .5);
            };


        //Get 2D dot product
        static Func<double, double, double> dot = (x, y) =>
            {
                return (x * x + y * y);
            };


        //Normalize 2D vector
        static Func<double[], double[]> norm = (v) =>
            {
                double[] v2 = new double[2];

                double mag = Math.Sqrt(dot(v[0], v[1]));
                v2[0] = v[0] / (mag);
                v2[1] = v[1] / (mag);

                return v2;
            };


        MouseInput() { }

        public static void mouse(string key, int x, int y)
        {
            mouse_event(inputMap[key], map(x, bounds.Width), map(y, bounds.Height), 0, 0);

            if (key.Contains("doubleclick"))
                mouse_event(inputMap[key], map(x, bounds.Width), map(y, bounds.Height), 0, 0);

            if (key == "upRight")
                Thread.Sleep(1000);
        }


        //Simulate mouse movement (Constant velocity of sqrt(800) pixels per 5 milliseconds):
        public static void move(int x, int y)
        {
            double[] v = new double[2];

            do
            {
                v[0] = (x - Cursor.Position.X);
                v[1] = (y - Cursor.Position.Y);

                //Scale to unit vector:
                v = norm(v);

                mouse("move", (int)(Cursor.Position.X + v[0] * 20), (int)(Cursor.Position.Y + v[1] * 20));

                Thread.Sleep(5);
            } while (dot(x - Cursor.Position.X, y - Cursor.Position.Y) > 100);

            mouse("move", x, y);
        }


        public static void mouseWheel(int x, int y, int detents)
        {
            mouse("move", x, y);
            mouse_event(inputMap["detent"], x, y, detents, 0);
            System.Threading.Thread.Sleep(300);
        }
    }
}
