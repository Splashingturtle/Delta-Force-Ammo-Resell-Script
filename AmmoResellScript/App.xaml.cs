using AmmoResellScript.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace AmmoResellScript
{
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            var vm = ServiceProvider?.GetRequiredService<ReceiveViewModel>();
            vm?.StopUdpListen();
            vm?.SavePriceHistory();
        }

        #region DI
        private void ConfigureServices(IServiceCollection service)
        {
            service.AddSingleton<AutoBuyViewModel>();
            service.AddSingleton<ReceiveViewModel>();
            service.AddSingleton<DualEndViewModel>();
            service.AddSingleton<SettingViewModel>();
            service.AddSingleton<MainWindowViewModel>();
        }
        #endregion
    }
}
