using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace map_editor_3D.src
{
    internal static class Input
    {
        private static HashSet<Keys> pressedKeys = new HashSet<Keys>();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public static bool IsKeyDown(Keys key)
        {
            return pressedKeys.Contains(key);
        }

        public static void KeyDown(Keys key)
        {
            pressedKeys.Add(key);
            //System.Diagnostics.Debug.WriteLine(key);
        }

        public static void KeyUp(Keys key)
        {
            pressedKeys.Remove(key);
        }

        public static bool IsCtrlDown()
        {
            return (GetAsyncKeyState(0x11) & 0x8000) != 0; // 0x11 is the virtual key code for Ctrl
        }

        public static bool IsShiftDown()
        {
            return (GetAsyncKeyState(0x10) & 0x8000) != 0; // 0x10 is the virtual key code for Shift
        }
    }
}
