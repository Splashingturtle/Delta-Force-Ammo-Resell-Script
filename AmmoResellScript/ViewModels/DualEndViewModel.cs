using AmmoResellScript.Model;
using AmmoResellScript.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AmmoResellScript.ViewModels
{
    public partial class DualEndViewModel : ObservableObject
    {
        private bool _isExit = false;
        private const int MaxLines = 100;
        private readonly List<string> _logBuffer = new();
        private CancellationTokenSource _cts;
        private bool _isRunning;

        [ObservableProperty]
        private string targetPrice;
        [ObservableProperty]
        private bool isBroadcastEnabled;
        [ObservableProperty]
        private string logs;

        private int _smallBuyX;
        private int _smallBuyY;
        private System.Windows.WindowState _savedWindowState;

        [RelayCommand]
        private async Task BindSmallWindowBuy()
        {
            AddLog("🖱️ 请点击小窗口中的购买按钮位置...");
            var point = await MouseService.WaitLeftMouseClickAsync();
            _smallBuyX = point.X;
            _smallBuyY = point.Y;
            AddLog($"✅ 小窗购买按钮已绑定：({point.X}, {point.Y})");
        }

        [RelayCommand]
        private async Task Start()
        {
            if (_isRunning) return;

            if (_smallBuyX == 0 && _smallBuyY == 0)
            { AddLog("⚠️ 请先绑定小窗购买按钮"); return; }
            if (!int.TryParse(TargetPrice, out int tp) || tp <= 0)
            { AddLog("⚠️ 请正确填写目标价格"); return; }

            _cts = new CancellationTokenSource();
            _isRunning = true;
            _isExit = false;

            var mainWin = System.Windows.Application.Current?.MainWindow;
            if (mainWin != null)
            {
                _savedWindowState = mainWin.WindowState;
                mainWin.Topmost = false;
                mainWin.WindowState = System.Windows.WindowState.Minimized;
            }

            AddLog("✅ 双端已启动");
            await Task.Run(() => RunDualEndLoop(_cts.Token));
        }

        public void Stop()
        {
            if (!_isRunning) return;
            _cts.Cancel();
            _isRunning = false;
            _isExit = true;
            RestoreMainWindow();
            AddLog("🛑 双端已停止");
        }

        public void OnLeaving()
        {
            if (_isRunning) Stop();
            RestoreMainWindow();
        }

        private void RestoreMainWindow()
        {
            var mainWin = System.Windows.Application.Current?.MainWindow;
            if (mainWin != null)
            {
                mainWin.Topmost = true;
                mainWin.WindowState = _savedWindowState == System.Windows.WindowState.Minimized
                    ? System.Windows.WindowState.Normal : _savedWindowState;
            }
        }

        private void RunDualEndLoop(CancellationToken token)
        {
            try
            {
                var config = ConfigService.LoadDeviceSettingAsync().GetAwaiter().GetResult();
                int clickDelay = config.ClickDelay;

                int accountX = config.AccountX;
                int accountY = config.AccountY;
                int buyBtnX = config.BuyButtonX;
                int buyBtnY = config.BuyButtonY;
                int coinL = config.RemainingHaverCoinLeftX;
                int coinT = config.RemainingHaverCoinLeftY;
                int coinR = config.RemainingHaverCoinRightX;
                int coinB = config.RemainingHaverCoinRightY;

                if (!int.TryParse(TargetPrice, out int target)) target = int.MaxValue;

                MouseService.MoveAndClick(accountX, accountY, clickDelay);
                string firstStr = ScreenOcrHelper.RecognizeNumberFromScreen(coinL, coinT, coinR, coinB);
                if (!double.TryParse(firstStr, out double firstMoney))
                {
                    AddLog("❌ 无法识别大窗口初始余额，请检查配置页的账户和哈弗币坐标");
                    Stop();
                    return;
                }
                AddLog($"💰 初始余额：{firstMoney}");

                while (!token.IsCancellationRequested && !_isExit)
                {
                    if (MouseService.IsRKeyPressed())
                    {
                        AddLog("🔴 R键停止");
                        Stop();
                        break;
                    }

                    MouseService.MoveAndClick(buyBtnX, buyBtnY, clickDelay);
                    MouseService.MoveAndClick(accountX, accountY, clickDelay);

                    string curStr = ScreenOcrHelper.RecognizeNumberFromScreen(coinL, coinT, coinR, coinB);
                    if (!double.TryParse(curStr, out double currentMoney))
                        continue;

                    double price = (firstMoney - currentMoney) / 31.0;
                    int priceInt = (int)Math.Round(price, MidpointRounding.AwayFromZero);

                    if (IsBroadcastEnabled && priceInt > 1)
                    {
                        int p = priceInt;
                        Task.Run(() =>
                        {
                            try { UdpBroadcastService.SendBroadcast(p.ToString()); }
                            catch { }
                        });
                    }

                    firstMoney = currentMoney;
                    AddLog($"价格：{priceInt}");

                    if (priceInt <= target && priceInt >= 10)
                    {
                        AddLog($"🎯 命中！{priceInt} ≤ {target}，切小窗购买");
                        MouseService.AltTab();
                        Thread.Sleep(70);
                        MouseService.MoveMouseTo(_smallBuyX, _smallBuyY);
                        for (int i = 0; i <= config.PurchaseCount; i++)
                        {
                            MouseService.LeftMouseClick();
                            Thread.Sleep(300);
                        }
                        

                       

                        MouseService.AltTab();
                        AddLog("✅ 小窗购买完成");
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                    AddLog($"异常：{ex.Message}");
            }
            finally { _isRunning = false; }
        }

        public void AddLog(string message)
        {
            string timeStampedMsg = $"[{DateTime.Now:HH:mm:ss}] {message}";
            _logBuffer.Add(timeStampedMsg);
            if (_logBuffer.Count > MaxLines)
                _logBuffer.RemoveAt(0);
            Logs = string.Join(Environment.NewLine, _logBuffer);
        }
    }
}
