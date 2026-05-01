using System.Runtime.InteropServices;

public static class FastKeyboard
{
    private const byte VK_ESC = 0x1B;
    private const byte VK_SPACE = 0x20;
    private const int KEY_DOWN = 0;
    private const int KEY_UP = 2;

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    /// <summary>
    /// 按下一次 ESC（极速）
    /// </summary>
    public static void PressEsc()
    {
        keybd_event(VK_ESC, 0, KEY_DOWN, 0);
        keybd_event(VK_ESC, 0, KEY_UP, 0);
    }

    /// <summary>
    /// 高速按空格
    /// </summary>
    /// <param name="count">按压次数</param>
    public static void PressSpace(int count)
    {
        for (int i = 0; i < count; i++)
        {
            keybd_event(VK_SPACE, 0, KEY_DOWN, 0);
            keybd_event(VK_SPACE, 0, KEY_UP, 0);
        }
    }
}