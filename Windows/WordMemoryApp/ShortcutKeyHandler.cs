using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WordMemoryApp
{
    public class ShortcutKeyHandler
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;

        public static void RegisterShortcutKey(IntPtr windowHandle)
        {
            const uint MOD_CONTROL = 0x0002; // CTRL key
            const uint VK_O = 0x4F;          // 'O' key

            if (!RegisterHotKey(windowHandle, HOTKEY_ID, MOD_CONTROL, VK_O))
            {
                throw new InvalidOperationException("Failed to register hotkey.");
            }
        }

        public static void UnregisterShortcutKey(IntPtr windowHandle)
        {
            UnregisterHotKey(windowHandle, HOTKEY_ID);
        }
    }
}
