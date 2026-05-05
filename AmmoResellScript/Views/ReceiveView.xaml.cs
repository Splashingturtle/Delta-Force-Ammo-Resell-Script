using AmmoResellScript.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AmmoResellScript.Views
{
    public partial class ReceiveView : UserControl
    {
        public ReceiveView()
        {
            InitializeComponent();
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.R && DataContext is ReceiveViewModel vm)
                {
                    vm.StopUdpListen();
                }
            };

            var app = Application.Current as App;
            if (app?.ServiceProvider != null)
            {
                DataContext = app.ServiceProvider.GetRequiredService<ReceiveViewModel>();
            }
        }

        private void CalcAvgPrice_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ReceiveViewModel vm) return;

            if (!double.TryParse(vm.InitCapital, out double initCap))
            {
                LogError(vm, "请正确填写初始资金");
                return;
            }
            if (!double.TryParse(vm.EndBalance, out double endBal))
            {
                LogError(vm, "请正确填写结束余额");
                return;
            }
            if (!double.TryParse(vm.BulletCount, out double bullets) || bullets <= 0)
            {
                LogError(vm, "请正确填写子弹总数（必须大于0）");
                return;
            }

            double avg = (initCap - endBal) / bullets;
            vm.AvgPrice = avg.ToString("F2");
            vm.AddLog($"[计算] 平均价格 = ({initCap} - {endBal}) / {bullets} = {avg:F2}");
        }

        private static void LogError(ReceiveViewModel vm, string msg)
        {
            vm.AddLog($"[错误] {msg}");
        }
    }
}
