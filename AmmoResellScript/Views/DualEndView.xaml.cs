using AmmoResellScript.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AmmoResellScript.Views
{
    public partial class DualEndView : UserControl
    {
        public DualEndView()
        {
            InitializeComponent();

            KeyDown += (s, e) =>
            {
                if (e.Key == Key.R && DataContext is DualEndViewModel vm)
                {
                    vm.Stop();
                }
            };

            var app = Application.Current as App;
            if (app?.ServiceProvider != null)
            {
                DataContext = app.ServiceProvider.GetRequiredService<DualEndViewModel>();
            }

            Unloaded += (s, e) =>
            {
                if (DataContext is DualEndViewModel vm)
                {
                    vm.OnLeaving();
                }
            };
        }
    }
}
