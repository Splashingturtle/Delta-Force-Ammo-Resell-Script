using AmmoResellScript.Model;
using AmmoResellScript.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmmoResellScript.ViewModels
{
    public partial class SettingViewModel : ObservableObject
    {
        #region 目标子弹坐标
        [ObservableProperty]
        private int targetAmmoX = 1652;

        [ObservableProperty]
        private int targetAmmoY = 306;
        #endregion

        #region 200发托条坐标
        [ObservableProperty]
        private int twoHundredAmmoX = 1039;

        [ObservableProperty]
        private int twoHundredAmmoY = 594;
        #endregion

        #region 购买按钮坐标
        [ObservableProperty]
        private int buyButtonX = 2022;

        [ObservableProperty]
        private int buyButtonY = 975;
        #endregion

        #region 开始游戏按钮坐标
        [ObservableProperty]
        private int startGameButtonX = 205;

        [ObservableProperty]
        private int startGameButtonY = 58;
        #endregion

        #region 交易行按钮坐标
        [ObservableProperty]
        private int tradeRowButtonX = 700;

        [ObservableProperty]
        private int tradeRowButtonY = 56;
        #endregion

        #region 账户坐标
        [ObservableProperty]
        private int accountX = 2217;

        [ObservableProperty]
        private int accountY = 67;
        #endregion

        #region 全面战场坐标
        [ObservableProperty]
        private int quanMianZhanChangX = 0;

        [ObservableProperty]
        private int quanMianZhanChangY = 0;
        #endregion

        #region 烽火地带坐标
        [ObservableProperty]
        private int fengHuoX = 0;

        [ObservableProperty]
        private int fengHuoY = 0;
        #endregion

        #region 价格区域
        [ObservableProperty]
        private int priceRegionLeftX = 2065;

        [ObservableProperty]
        private int priceRegionLeftY = 352;

        [ObservableProperty]
        private int priceRegionRightX = 2209;

        [ObservableProperty]
        private int priceRegionRightY = 384;
        #endregion

        #region 账户剩余哈弗币区域
        [ObservableProperty]
        private int remainingHaverCoinLeftX = 0;

        [ObservableProperty]
        private int remainingHaverCoinLeftY = 0;

        [ObservableProperty]
        private int remainingHaverCoinRightX = 0;

        [ObservableProperty]
        private int remainingHaverCoinRightY = 0;
        #endregion

        Point point;

        // 购买次数
        [ObservableProperty]
        private int purchaseCount = 2;

        // 点击延迟
        [ObservableProperty]
        private int clickDelay = 2;

        [ObservableProperty]
        private string logs;

        private async void LoadSettings()
        {
            var ds = await ConfigService.LoadDeviceSettingAsync();
            TargetAmmoX = ds.TargetAmmoX;
            TargetAmmoY = ds.TargetAmmoY;
            TwoHundredAmmoX = ds.TwoHundredAmmoX;
            TwoHundredAmmoY = ds.TwoHundredAmmoY;
            BuyButtonX = ds.BuyButtonX;
            BuyButtonY = ds.BuyButtonY;
            StartGameButtonX = ds.StartGameButtonX;
            StartGameButtonY = ds.StartGameButtonY;
            TradeRowButtonX = ds.TradeRowButtonX;
            TradeRowButtonY = ds.TradeRowButtonY;
            AccountX = ds.AccountX;
            AccountY = ds.AccountY;
            PriceRegionLeftX = ds.PriceRegionLeftX;
            PriceRegionLeftY = ds.PriceRegionLeftY;
            PriceRegionRightX = ds.PriceRegionRightX;
            PriceRegionRightY = ds.PriceRegionRightY;
            RemainingHaverCoinLeftX = ds.RemainingHaverCoinLeftX;
            RemainingHaverCoinLeftY = ds.RemainingHaverCoinLeftY;
            RemainingHaverCoinRightX = ds.RemainingHaverCoinRightX;
            RemainingHaverCoinRightY = ds.RemainingHaverCoinRightY;
            FengHuoX = ds.FengHuoX;
            FengHuoY = ds.FengHuoY;
            QuanMianZhanChangX = ds.QuanMianZhanChangX;
            QuanMianZhanChangY = ds.QuanMianZhanChangY;
            PurchaseCount = ds.PurchaseCount;
            ClickDelay = ds.ClickDelay;
        }

        //保存配置
        [RelayCommand]
        public async Task SaveAsync()
        {
            try
            {
                var model = new UserModel
                {
                    TargetAmmoX = TargetAmmoX,
                    TargetAmmoY = TargetAmmoY,
                    TwoHundredAmmoX = TwoHundredAmmoX,
                    TwoHundredAmmoY = TwoHundredAmmoY,
                    BuyButtonX = BuyButtonX,
                    BuyButtonY = BuyButtonY,
                    StartGameButtonX = StartGameButtonX,
                    StartGameButtonY = StartGameButtonY,
                    TradeRowButtonX = TradeRowButtonX,
                    TradeRowButtonY = TradeRowButtonY,
                    AccountX = AccountX,
                    AccountY = AccountY,
                    PriceRegionLeftX = PriceRegionLeftX,
                    PriceRegionLeftY = PriceRegionLeftY,
                    PriceRegionRightX = PriceRegionRightX,
                    PriceRegionRightY = PriceRegionRightY,
                    RemainingHaverCoinLeftX = RemainingHaverCoinLeftX,
                    RemainingHaverCoinLeftY = RemainingHaverCoinLeftY,
                    RemainingHaverCoinRightX = RemainingHaverCoinRightX,
                    RemainingHaverCoinRightY = RemainingHaverCoinRightY,
                    FengHuoX = FengHuoX,
                    FengHuoY = FengHuoY,
                    QuanMianZhanChangX = QuanMianZhanChangX,
                    QuanMianZhanChangY = QuanMianZhanChangY,
                    PurchaseCount = PurchaseCount,
                    ClickDelay = ClickDelay
                };
                await ConfigService.SaveDeviceSettingAsync(model);
            }
            catch (Exception ex)
            {
                //保存配置
            }
        }

        //获取点击坐标
        
        [RelayCommand]
        public async Task CommonButtonBinding(string? destination)
        {
            try
            {
                switch (destination)
                {
                    case "TargetAmmo":
                        Logs = "请点击目标子弹";
                        await Task.Delay(100); // 确保日志刷新到UI
                        point = await MouseService.WaitLeftMouseClickAsync();
                        TargetAmmoX = point.X;
                        TargetAmmoY = point.Y;
                        Logs = $"目标子弹坐标已保存：X={point.X}, Y={point.Y}";
                        break;

                    case "TwoHundredAmmo":
                        Logs = "请点击200发位置";
                        await Task.Delay(100);
                        point = await MouseService.WaitLeftMouseClickAsync();
                        TwoHundredAmmoX = point.X;
                        TwoHundredAmmoY = point.Y;
                        Logs = $"200发位置已保存：X={point.X}, Y={point.Y}";
                        break;

                    case "BuyButton":
                        Logs = "请点击购买按钮";
                        await Task.Delay(100);
                        point = await MouseService.WaitLeftMouseClickAsync();
                        BuyButtonX = point.X;
                        BuyButtonY = point.Y;
                        Logs = $"购买按钮坐标已保存：X={point.X}, Y={point.Y}";
                        break;

                    case "StartGameButton":
                        Logs = "请点击开始游戏按钮";
                        await Task.Delay(100);
                        point = await MouseService.WaitLeftMouseClickAsync();
                        StartGameButtonX = point.X;
                        StartGameButtonY = point.Y;
                        Logs = $"开始游戏按钮已保存：X={point.X}, Y={point.Y}";
                        break;

                    case "TradeRowButton":
                        Logs = "请点击交易行按钮";
                        await Task.Delay(100);
                        point = await MouseService.WaitLeftMouseClickAsync();
                        TradeRowButtonX = point.X;
                        TradeRowButtonY = point.Y;
                        Logs = $"交易行按钮已保存：X={point.X}, Y={point.Y}";
                        break;

                    case "Account":
                        Logs = "请点击账户位置";
                        await Task.Delay(100);
                        point = await MouseService.WaitLeftMouseClickAsync();
                        AccountX = point.X;
                        AccountY = point.Y;
                        Logs = $"账户坐标已保存：X={point.X}, Y={point.Y}";
                        break;

                    case "PriceRegion":
                        // 第一步：点击左上角
                        Logs = "请点击价格区域的左上角";
                        await Task.Delay(100);
                        point = await MouseService.WaitLeftMouseClickAsync();
                        PriceRegionLeftX = point.X;
                        PriceRegionLeftY = point.Y;

                        // 第二步：点击右下角
                        Logs = "请点击价格区域的右下角";
                        await Task.Delay(100);
                        point = await MouseService.WaitLeftMouseClickAsync();
                        PriceRegionRightX = point.X;
                        PriceRegionRightY = point.Y;

                        // 保证 LeftX < RightX（横坐标：左 < 右）
                        if (PriceRegionLeftX > PriceRegionRightX)
                        {
                            (PriceRegionLeftX, PriceRegionRightX) = (PriceRegionRightX, PriceRegionLeftX);
                        }
                        // 保证 LeftY < RightY（纵坐标：上 < 下）
                        if (PriceRegionLeftY > PriceRegionRightY)
                        {
                            (PriceRegionLeftY, PriceRegionRightY) = (PriceRegionRightY, PriceRegionLeftY);
                        }
                        Logs = $"价格区域已保存：左上角({PriceRegionLeftX},{PriceRegionLeftY})，右下角({PriceRegionRightX},{PriceRegionRightY})";
                        break;

                    case "RemainingHaverCoinRegion":
                        // 第一步：点击左上角
                        Logs = "请从右下到左上截图";
                        await Task.Delay(100);
                        point = await MouseService.WaitLeftMouseClickAsync();
                        RemainingHaverCoinLeftX = point.X;
                        RemainingHaverCoinLeftY = point.Y;

                        // 第二步：点击右下角
                        Logs = "请从左上到右下截图";
                        await Task.Delay(100);
                        point = await MouseService.WaitLeftMouseClickAsync();
                        RemainingHaverCoinRightX = point.X;
                        RemainingHaverCoinRightY = point.Y;
                        if (RemainingHaverCoinLeftX > RemainingHaverCoinRightX)
                        {
                            (RemainingHaverCoinLeftX, RemainingHaverCoinRightX) = (RemainingHaverCoinRightX, RemainingHaverCoinLeftX);
                        }
                        // 保证 LeftY < RightY（纵坐标：上 < 下）
                        if (RemainingHaverCoinLeftY > RemainingHaverCoinRightY)
                        {
                            (RemainingHaverCoinLeftY, RemainingHaverCoinRightY) = (RemainingHaverCoinRightY, RemainingHaverCoinLeftY);
                        }
                        Logs = $"剩余哈弗币区域已保存：({RemainingHaverCoinLeftX},{RemainingHaverCoinLeftY})，右下角({RemainingHaverCoinRightX},{RemainingHaverCoinRightY})";
                        break;

                    case "FengHuo":
                        Logs = "请点击烽火地带位置";
                        await Task.Delay(100);
                        point = await MouseService.WaitLeftMouseClickAsync();
                        FengHuoX = point.X;
                        FengHuoY = point.Y;
                        Logs = $"烽火地带坐标已保存：X={point.X}, Y={point.Y}";
                        break;

                    case "QuanMian":
                        Logs = "请点击全面战场位置";
                        await Task.Delay(100);
                        point = await MouseService.WaitLeftMouseClickAsync();
                        QuanMianZhanChangX = point.X;
                        QuanMianZhanChangY = point.Y;
                        Logs = $"全面战场坐标已保存：X={point.X}, Y={point.Y}";
                        break;

                    default:
                        Logs = $"未知的绑定类型：{destination}";
                        break;
                }
            }
            catch (Exception ex)
            {
                Logs = $"绑定失败：{ex.Message}";
                // 调试时输出异常，方便定位
                System.Diagnostics.Debug.WriteLine($"CommonButtonBinding 异常：{ex}");
            }
        }


        public SettingViewModel()
        {
            try
            {
                LoadSettings();
                Logs = "配置加载成功";
            }
            catch (Exception)
            {
                Logs = "加载配置失败，已使用默认设置";
            }
        }
    }
}
