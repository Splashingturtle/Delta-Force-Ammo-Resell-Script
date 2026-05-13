using AmmoResellScript.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
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

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ReceiveViewModel vm)
            {
                ChartView.Model = vm.PlotModel;
                ChartView.Controller = vm.PlotController;
                vm.PropertyChanged += OnViewModelPropertyChanged;
                SyncChartVisibility(vm.IsCalcPanelVisible);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ReceiveViewModel vm)
            {
                vm.PropertyChanged -= OnViewModelPropertyChanged;
            }
            ChartView.Model = null;
            ChartView.Controller = null;
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ReceiveViewModel.IsCalcPanelVisible))
            {
                var vm = (ReceiveViewModel)sender;
                SyncChartVisibility(vm.IsCalcPanelVisible);
            }
        }

        private void SyncChartVisibility(bool isPanelVisible)
        {
            if (isPanelVisible)
            {
                RootGrid.RowDefinitions[1].Height = new GridLength(0);
                ChartBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                RootGrid.RowDefinitions[1].Height = new GridLength(2, GridUnitType.Star);
                ChartBorder.Visibility = Visibility.Visible;
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
