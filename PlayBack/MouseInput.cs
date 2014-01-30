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
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, int dwData, uint dwExtraInfo);

        private static Rectangle bounds = Screen.GetBounds(Point.Empty);

        // constants for the mouse_input() API function
        private static Dictionary<string, uint> inputMap = new Dictionary<string, uint>()
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


        MouseInput() { }

        public static void mouse(string key, int x, int y)
        {
            mouse_event(inputMap[key], (int)((((x+1) * 65535) / (double)bounds.Width) + .5), (int)((((y+1) * 65535) / (double)bounds.Height) + .5), 0, 0);

            if (key == "upRight")
                Thread.Sleep(1000);
        }

        public static void move(int x, int y)
        {
            double dx = (x - Cursor.Position.X);
            double dy = (y - Cursor.Position.Y);

            //Scale to unit vector:
            Double magnitude = Math.Sqrt(dx * dx + dy * dy);
            dx = dx / (magnitude);
            dy = dy / (magnitude);

            while ((x - Cursor.Position.X) * (x - Cursor.Position.X) + (y - Cursor.Position.Y) * (y - Cursor.Position.Y) > 100)
            {
                dx = (x - Cursor.Position.X);
                dy = (y - Cursor.Position.Y);
                magnitude = Math.Sqrt(dx * dx + dy * dy);
                dx = dx / (magnitude);
                dy = dy / (magnitude);

                mouse("move", (int)(Cursor.Position.X + dx * 20), (int)(Cursor.Position.Y + dy * 20));

                Thread.Sleep(5);
            }

            mouse("move", x, y);
        }


        public static void mouseWheel(int x, int y, int detents)
        {
            mouse("move", x, y);
            mouse_event(inputMap["detent"], x, y, detents, 0);
            System.Threading.Thread.Sleep(300);
        }


        public static void doubleClick(string key, int x, int y)
        {
            mouse_event(inputMap[key], (int)((((x+1) * 65535) / (double)bounds.Width) + .5), (int)((((y+1) * 65535) / (double)bounds.Height) + .5), 0, 0);
            mouse_event(inputMap[key], (int)((((x+1) * 65535) / (double)bounds.Width) + .5), (int)((((y+1) * 65535) / (double)bounds.Height) + .5), 0, 0);
        }

    }
}
