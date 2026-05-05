using AmmoResellScript.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace AmmoResellScript
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += OnSourceInitialized;

            var app = Application.Current as App;
            if (app?.ServiceProvider != null)
            {
                DataContext = app.ServiceProvider.GetRequiredService<MainWindowViewModel>();
                if (DataContext is MainWindowViewModel vm)
                {
                    vm.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(MainWindowViewModel.MainContent))
                        {
                            AnimateContentFadeIn();
                        }
                    };
                }
            }
        }

        private void AnimateContentFadeIn()
        {
            ContentArea.BeginAnimation(OpacityProperty, null);
            ContentArea.Opacity = 0;
            var fadeIn = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(200))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            ContentArea.BeginAnimation(OpacityProperty, fadeIn);
        }

        #region Acrylic Blur (Windows 10 1803+)
        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            ApplyAcrylicBlur();
        }

        private void ApplyAcrylicBlur()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd == IntPtr.Zero) return;

            var accent = new AccentPolicy
            {
                AccentState = 4, // ACCENT_ENABLE_ACRYLICBLURBEHIND
                AccentFlags = 2, // 从壁纸取色
                GradientColor = unchecked((int)0x45241C16), // ABGR: 深灰黑 27% 不透明
                AnimationId = 0
            };

            var accentStructSize = Marshal.SizeOf(accent);
            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = 19, // WCA_ACCENT_POLICY
                Data = accentPtr,
                SizeOfData = accentStructSize
            };

            SetWindowCompositionAttribute(hwnd, ref data);
            Marshal.FreeHGlobal(accentPtr);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public int AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
        #endregion

        #region 自定义窗口控件
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximize();
            }
            else
            {
                DragMove();
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ToggleMaximize()
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                MaxBtn.Content = "□";
            }
            else
            {
                WindowState = WindowState.Maximized;
                MaxBtn.Content = "❐";
            }
        }
        #endregion
    }
}
