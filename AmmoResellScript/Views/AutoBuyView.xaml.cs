using AmmoResellScript.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AmmoResellScript.Views
{
    /// <summary>
    /// AutoBuyView.xaml 的交互逻辑
    /// </summary>
    public partial class AutoBuyView : UserControl
    {
        public AutoBuyView()
        {
            InitializeComponent();

            var app = Application.Current as App;
            if (app.ServiceProvider != null)
            {
                DataContext = app.ServiceProvider.GetRequiredService<AutoBuyViewModel>();
            }

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is AutoBuyViewModel vm)
            {
                ChartView.Model = vm.PlotModel;
                ChartView.Controller = vm.PlotController;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ChartView.Model = null;
            ChartView.Controller = null;
        }
    }
}
