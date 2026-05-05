using AmmoResellScript.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace AmmoResellScript
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();//创建DI集合
            ConfigureServices(services);//把所有ViewModel放进这个集合
            ServiceProvider = services.BuildServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

        }
        #region DI
        private void ConfigureServices(IServiceCollection service)
        {
            service.AddSingleton<AutoBuyViewModel>();
            service.AddSingleton<ReceiveViewModel>();
            service.AddSingleton<SettingViewModel>();
            service.AddSingleton<MainWindowViewModel>();
        }
        #endregion
    }

}
