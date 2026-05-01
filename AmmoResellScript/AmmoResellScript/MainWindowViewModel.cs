using AmmoResellScript.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmmoResellScript
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private object mainContent;
        [ObservableProperty]
        private string logs;

        private readonly IServiceProvider _serviceProvider;

        public MainWindowViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        [RelayCommand]

        #region 导航函数
        private void Navigate(string? destination)
        {
            switch (destination)
            {
                case "AutoBuy":
                    MainContent = _serviceProvider.GetService<AutoBuyViewModel>();
                    break;
                case "Receive":
                    MainContent = _serviceProvider.GetService<ReceiveViewModel>();
                    break;
                case "Setting":
                    MainContent = _serviceProvider.GetService<SettingViewModel>();
                    break;
                default:
                    break;
            }
        }
        #endregion


    }
}
