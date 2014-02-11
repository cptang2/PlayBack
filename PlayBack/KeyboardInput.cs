using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace PlayBack
{
    class KeyboardInput
    {
        #region Unmanaged code import
        [DllImport("user32.dll")]
        static extern uint keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        #endregion

        public static void KeyDown(int key)
        {
            keybd_event((byte)key, 0, 0, 0);
        }

        public static void KeyUp(int key)
        {
            keybd_event((byte)key, 0, 0x02, 0);
        }
    }
}
