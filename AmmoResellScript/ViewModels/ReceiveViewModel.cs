using AmmoResellScript.Model;
using AmmoResellScript.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AmmoResellScript.ViewModels
{
    public partial class ReceiveViewModel : ObservableObject
    {
        private bool _isExit = false;
        private const int MaxLines = 100;

        private readonly List<string> _logBuffer = new List<string>();

        private UdpClient _udpClient;
        private CancellationTokenSource _cts;
        private bool _isRunning;

        [ObservableProperty]
        private string targetPrice;
        [ObservableProperty]
        private string logs;
        [ObservableProperty]
        private string initCapital = string.Empty;
        [ObservableProperty]
        private string endBalance = string.Empty;
        [ObservableProperty]
        private string bulletCount = string.Empty;
        [ObservableProperty]
        private string avgPrice = string.Empty;

        [RelayCommand]
        private void StartReceive()
        {
            if (_isRunning) return;

            _cts = new CancellationTokenSource();
            _isRunning = true;
            _isExit = false; // 重置退出标记
            AddLog("✅ UDP监听已启动，等待价格数据... 按 R 键停止");

            Task.Run(() => RunUdpBackgroundLoop(_cts.Token));
        }

        public void StopUdpListen()
        {
            if (!_isRunning) return;

            _cts.Cancel();
            _udpClient?.Close();
            _udpClient = null;
            _isRunning = false;
            _isExit = true; // 标记退出
            AddLog("🛑 UDP监听已停止");
        }

        private async void RunUdpBackgroundLoop(CancellationToken token)
        {
            var mode = await ConfigService.LoadDeviceSettingAsync();
            try
            {
                _udpClient = new UdpClient(8888);
                _udpClient.EnableBroadcast = true;

                while (!token.IsCancellationRequested && !_isExit)
                {
                    // 核心：实时检测R键，按下则立即退出
                    if (MouseService.IsRKeyPressed())
                    {
                        AddLog("🔴 检测到R键按下，停止UDP监听！");
                        StopUdpListen();
                        break;
                    }

                    if (_udpClient.Available > 0)
                    {
                        var result = await _udpClient.ReceiveAsync(token);
                        string text = Encoding.UTF8.GetString(result.Buffer).Trim();

                        if (!int.TryParse(text, out int nowPrice))
                            continue;

                        AddLog($"当前价格：{nowPrice}，目标价格：{TargetPrice}");

                        // 安全解析
                        if (int.TryParse(TargetPrice, out int tar) && nowPrice <= tar && nowPrice >= 10)
                        {
                            AddLog($"⚠️ 价格低于目标，执行点击！");
                            try
                            {
                                MouseService.MoveAndClick(mode.BuyButtonX, mode.BuyButtonY);

                                // 点击后仍检测R键，按下则退出
                                if (MouseService.IsRKeyPressed())
                                {
                                    AddLog("🔴 点击后检测到R键按下，停止UDP监听！");
                                    StopUdpListen();
                                    break;
                                }
                            }
                            catch (Exception e)
                            {
                                AddLog(e.Message);
                            }
                        }
                    }
                    else
                    {
                        // 等待，不阻塞，能响应停止和R键检测
                        await Task.Delay(100, token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 预期的取消异常，不记录日志
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                    AddLog($"UDP异常：{ex.Message}");
            }
            finally
            {
                _isRunning = false;
                _udpClient?.Close();
                _udpClient = null;
            }
        }

        public void AddLog(string message)
        {
            // 1. 添加时间戳，让日志更易读
            string timeStampedMsg = $"[{DateTime.Now:HH:mm:ss}] {message}";

            // 2. 将新日志加入缓冲区
            _logBuffer.Add(timeStampedMsg);

            // 3. 核心逻辑：检查是否超过最大行数，超过则移除最旧的行
            if (_logBuffer.Count > MaxLines)
            {
                _logBuffer.RemoveAt(0); // 移除列表中的第一项（最旧的）
            }

            // 4. 更新绑定属性（触发UI更新）
            Logs = string.Join(Environment.NewLine, _logBuffer);
        }
    }
}