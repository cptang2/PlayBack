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
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

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
            {"doubleclickLeft", 0x0002 | 0x0004},
            {"doubleclickRight", 0x0008 | 0x0010},
            {"doubleclickMiddle", 0x0020 | 0x0040}
        };


        MouseInput() { }

        public static void mouse(string key, uint x, uint y)
        {
            mouse_event(inputMap[key], (uint)((x / (float)bounds.Width) * 65535), (uint)((y / (float)bounds.Height) * 65535), 0, 0);
        }

        public static void move(uint x, uint y)
        {
            int curXPos = Cursor.Position.X;
            int curYPos = Cursor.Position.Y;
            float dx = (x - curXPos) / (float)300;
            float dy = (y - curYPos) / (float)300;

            for (int i = 0; i < 300; i++)
            {
                mouse("move", (uint)(curXPos + (dx * i)), (uint)(curYPos + (dy * i)));
                Thread.Sleep(5);
            }

            mouse("move", x, y);
        }

        public static void doubleClick(string key, uint x, uint y)
        {
            mouse_event(inputMap[key], (uint)((x / (float)bounds.Width) * 65535), (uint)((y / (float)bounds.Height) * 65535), 0, 0);
            mouse_event(inputMap[key], (uint)((x / (float)bounds.Width) * 65535), (uint)((y / (float)bounds.Height) * 65535), 0, 0);
        }

    }
}
