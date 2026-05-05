using AmmoResellScript.Model;
using AmmoResellScript.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmmoResellScript.ViewModels
{
    public partial class AutoBuyViewModel : ObservableObject
    {
        private bool _isExit = false;
        private bool _isRunning;
        private CancellationTokenSource _cts;
        int count = 0;

        [ObservableProperty]
        private string logs;
        [ObservableProperty]
        private int targetPrice;
        [ObservableProperty]
        private bool isBroadcastEnabled;
        [ObservableProperty]
        private bool isThreeChecked;

        private const int MaxLines = 100;

        private readonly List<string> _logBuffer = new List<string>();

        UserModel ds = new UserModel();

        [RelayCommand]
        private async Task StartAutoBuy()
        {
            AddLog("已启动");
            _cts = new CancellationTokenSource();
            _isRunning = true;
            _isExit = false;
            //加载文件
            ds = await ConfigService.LoadDeviceSettingAsync();
            if (IsThreeChecked)
            {
                Task.Run(() => RunStart31(_cts.Token, ds, IsBroadcastEnabled));
            }
            else
            {
                Task.Run(() => RunStart(_cts.Token, ds, IsBroadcastEnabled));
            }
            

        }

        private async void RunStart(CancellationToken token,UserModel ds,bool isBroadcastEnabled)
        {
            try
            {
                await Task.Delay(1000);
                // 死循环，直到 R键 或 外部停止
                while (!token.IsCancellationRequested && !_isExit)
                {
                    
                    //防止检测
                    if (count >= 300)
                    {
                        FastKeyboard.PressEsc();
                        Thread.Sleep(1000);
                        MouseService.MoveAndClick(ds.StartGameButtonX, ds.StartGameButtonY);
                        Thread.Sleep(1000);
                        FastKeyboard.PressEsc();
                        Thread.Sleep(1000);
                        MouseService.MoveAndClick(ds.QuanMianZhanChangX, ds.QuanMianZhanChangY);
                        Thread.Sleep(2000);
                        for (int i = 0; i < 5; i++)
                        {
                            FastKeyboard.PressSpace(2);
                            Thread.Sleep(500);
                        }
                        FastKeyboard.PressEsc();
                        Thread.Sleep(1000);
                        MouseService.MoveAndClick(ds.FengHuoX, ds.FengHuoY);
                        Thread.Sleep(1000);
                        MouseService.MoveAndClick(ds.TradeRowButtonX, ds.TradeRowButtonY);
                        Thread.Sleep(1000);
                        MouseService.MoveAndClick(ds.TargetAmmoX, ds.TargetAmmoY);
                        count = 0;
                    }
                    // ==============================================
                    // 【按 R 键立即退出循环】—— 核心判断
                    if (MouseService.IsRKeyPressed())
                    {
                        AddLog("🔴 检测到 R 键按下，退出循环");
                        break;
                    }
                    // ==============================================
                    string rawPrice = ScreenOcrHelper.RecognizeNumberFromScreen(ds.PriceRegionLeftX, ds.PriceRegionLeftY, ds.PriceRegionRightX, ds.PriceRegionRightY);
                    AddLog($"{DateTime.Now:HH:mm:ss} OCR 识别价格：{rawPrice}");
                    if (int.TryParse(rawPrice.Replace(",", ""), out int current))
                    {
                        //AddLog($"当前价格：{current}，目标价格：{TargetPrice}");

                        if (current <= TargetPrice && current > 0)
                        {
                            AddLog($"{DateTime.Now:HH:mm:ss} ⚠️ 价格低于目标，执行购买");
                            MouseService.MoveAndClick(ds.TwoHundredAmmoX, ds.TwoHundredAmmoY);  
                           
                            for (int i = 0; i < ds.PurchaseCount; i++)
                            {
                                MouseService.MoveAndClick(ds.BuyButtonX, ds.BuyButtonY);
                            }

                            Thread.Sleep(200);

                        }                       
                    }
                    if (isBroadcastEnabled && int.TryParse(rawPrice.Replace(",", ""), out int scendcurrent))
                    {
                        try
                        {
                            UdpBroadcastService.SendBroadcast(scendcurrent.ToString());
                            AddLog("已经广播");
                        }
                        catch (Exception)
                        {

                            AddLog("广播失败");
                        }
                    }
                    
                    FastKeyboard.PressEsc();
                    
                    MouseService.MoveAndClick(ds.TargetAmmoX, ds.TargetAmmoY);
                    Thread.Sleep(ds.ClickDelay);
                    count ++;
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消，不处理
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                    AddLog($"循环异常：{ex.Message}");
            }
            finally
            {
                // 收尾
                _isRunning = false;
                _isExit = true;
                AddLog("✅ 后台循环已安全停止");
            }
        }

        private async void RunStart31(CancellationToken token, UserModel ds, bool isBroadcastEnabled)
        {
            try
            {
                await Task.Delay(1000);
                //识别初始金额
                MouseService.MoveAndClick(ds.AccountX, ds.AccountY, ds.ClickDelay);
                string rawFirstMoney = ScreenOcrHelper.RecognizeNumberFromScreen(ds.RemainingHaverCoinLeftX, ds.RemainingHaverCoinLeftY, ds.RemainingHaverCoinRightX, ds.RemainingHaverCoinRightY);
                bool success = int.TryParse(rawFirstMoney.Replace(",", ""), out int FirstMoney);
                if (success)
                {
                    AddLog($"初始金额：{FirstMoney}");
                }
                else { AddLog($"初始金额识别失败，原始结果：{rawFirstMoney}"); }
                // 死循环，直到 R键 或 外部停止
                while (!token.IsCancellationRequested && !_isExit)
                {
                    // 【按 R 键立即退出循环】—— 核心判断
                    if (MouseService.IsRKeyPressed())
                    {
                        AddLog("🔴 检测到 R 键按下，退出循环");
                        break;
                    }
                    // ==============================================

                    //点击购买31发
                    MouseService.MoveAndClick(ds.BuyButtonX, ds.BuyButtonY);
                    //点击账户
                    MouseService.MoveAndClick(ds.AccountX, ds.AccountY, ds.ClickDelay);
                    //识别余额
                    string rawCurrentMoney = ScreenOcrHelper.RecognizeNumberFromScreen(ds.RemainingHaverCoinLeftX, ds.RemainingHaverCoinLeftY, ds.RemainingHaverCoinRightX, ds.RemainingHaverCoinRightY);
                    success = int.TryParse(rawCurrentMoney.Replace(",", ""), out int CurrentMoney);
                    //计算价格
                    int price = (FirstMoney - CurrentMoney) / 31;
                    FirstMoney = CurrentMoney;
                    AddLog($"{DateTime.Now:HH:mm:ss}当前价格：{price}");

                    if (isBroadcastEnabled&&price > 1)
                    {
                        try
                        {
                            UdpBroadcastService.SendBroadcast(price.ToString());
                            AddLog("已广播");
                        }
                        catch (Exception)
                        {
                            AddLog("广播失败");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消，不处理
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                    AddLog($"循环异常：{ex.Message}");
            }
            finally
            {
                // 收尾
                _isRunning = false;
                _isExit = true;
                AddLog("✅ 后台循环已安全停止");
            }
        }

        public void AddLog(string message)
        {
            // 1. 添加时间戳，让日志更易读
            string timeStampedMsg = $"[{DateTime.Now:HH:mm:ss}] {message}";

            // 2. 将新日志加入缓冲区
            _logBuffer.Add(timeStampedMsg);

            // 3. 核心逻辑：检查是否超过100行，如果超过则移除最旧的行
            if (_logBuffer.Count > MaxLines)
            {
                _logBuffer.RemoveAt(0); // 移除列表中的第一项（最旧的）
            }

            // 4. 更新绑定属性
            // 使用 string.Join 将列表转换回字符串，并赋予 Logs 属性触发 UI 更新
            Logs = string.Join(Environment.NewLine, _logBuffer);
        }

        public void ClearLogs()
        {
            _logBuffer.Clear();
            Logs = string.Empty; // 清空日志显示
        }
    }
}
