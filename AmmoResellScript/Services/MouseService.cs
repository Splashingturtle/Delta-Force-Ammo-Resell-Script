using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public static class MouseService
{
    // 原有API导入
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out Point lpPoint);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    // 原有常量
    private const int VK_LBUTTON = 0x01;
    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;
    // 新增：R键虚拟键码
    private const int VK_R = 0x52;
    // 新增：默认延时（可根据游戏调整，建议50-200ms）
    private const int DefaultDelayMs = 100;

    // 原有方法...
    public static async Task<Point> WaitLeftMouseClickAsync()
    {
        while ((GetAsyncKeyState(VK_LBUTTON) & 0x8000) == 0)
            await Task.Delay(10);

        while ((GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0)
            await Task.Delay(10);

        GetCursorPos(out Point point);
        return point;
    }

    // 重构：拆分移动和点击，增加延时（解决游戏不识别问题）
    public static void MoveAndClick(int x, int y, int delayMs = DefaultDelayMs)
    {
        // 第一步：仅移动鼠标到目标位置
        MoveMouseTo(x, y);
        // 关键：移动后延时，让游戏检测到鼠标位置变化
        System.Threading.Thread.Sleep(delayMs);
        // 第二步：执行左键点击（按下+抬起）
        LeftMouseClick();
    }

    // 新增：独立的左键点击方法（按下+抬起，保证点击事件完整）
    public static void LeftMouseClick()
    {
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        // 点击按下后短暂延时，模拟真实点击
        System.Threading.Thread.Sleep(20);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
    }

    public static void MoveMouseTo(int x, int y) => SetCursorPos(x, y);

    // 原有：检测R键是否按下
    public static bool IsRKeyPressed()
    {
        return (GetAsyncKeyState(VK_R) & 0x8000) != 0;
    }
}