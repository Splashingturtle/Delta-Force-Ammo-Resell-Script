using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System;
using System.Threading;

public static class MouseService
{
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out Point lpPoint);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(int x, int y);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int VK_LBUTTON = 0x01;
    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;
    private const int VK_R = 0x52;
    private const int VK_MENU = 0x12;
    private const int VK_TAB = 0x09;
    private const int KEYEVENTF_KEYUP = 0x0002;
    private const int SW_MINIMIZE = 6;
    private const int SW_RESTORE = 9;
    private const int DefaultDelayMs = 100;

    public static async Task<Point> WaitLeftMouseClickAsync()
    {
        while ((GetAsyncKeyState(VK_LBUTTON) & 0x8000) == 0)
            await Task.Delay(10);

        while ((GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0)
            await Task.Delay(10);

        GetCursorPos(out Point point);
        return point;
    }

    public static void MoveAndClick(int x, int y, int delayMs = DefaultDelayMs)
    {
        MoveMouseTo(x, y);
        Thread.Sleep(delayMs);
        LeftMouseClick();
    }

    public static void LeftMouseClick()
    {
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        Thread.Sleep(20);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
    }

    public static void MoveMouseTo(int x, int y) => SetCursorPos(x, y);

    public static bool IsRKeyPressed()
    {
        return (GetAsyncKeyState(VK_R) & 0x8000) != 0;
    }

    public static IntPtr GetWindowHandleAtPoint(Point point)
    {
        return WindowFromPoint(point.X, point.Y);
    }

    /// <summary>
    /// 模拟 Alt+Tab 切换窗口。脚本最小化后，Alt+Tab 在两个游戏窗口间来回切。
    /// </summary>
    public static void AltTab()
    {
        keybd_event((byte)VK_MENU, 0, 0, UIntPtr.Zero);
        keybd_event((byte)VK_TAB, 0, 0, UIntPtr.Zero);
        Thread.Sleep(10);
        keybd_event((byte)VK_TAB, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        keybd_event((byte)VK_MENU, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }

    /// <summary>最小化窗口</summary>
    public static void MinimizeWindow(IntPtr hWnd)
    {
        if (hWnd != IntPtr.Zero)
            ShowWindow(hWnd, SW_MINIMIZE);
    }

    /// <summary>恢复窗口</summary>
    public static void RestoreWindow(IntPtr hWnd)
    {
        if (hWnd != IntPtr.Zero)
            ShowWindow(hWnd, SW_RESTORE);
    }
}
