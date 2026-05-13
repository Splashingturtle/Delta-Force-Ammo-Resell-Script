using AmmoResellScript.Model;
using AmmoResellScript.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
        private string minPrice = "10";
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

        // 图表相关
        [ObservableProperty]
        private PlotModel plotModel;
        [ObservableProperty]
        private IPlotController plotController;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsCalcPanelVisible))]
        private bool isScanning;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsCalcPanelVisible))]
        private bool isCalcPanelExpanded = true;

        public bool IsCalcPanelVisible => !IsScanning && IsCalcPanelExpanded;

        private LineSeries _lineSeries;
        private readonly List<PriceDataPoint> _priceHistory = new();
        private int _dataIndex;
        private const string PriceHistoryFileName = "price-history.json";

        public ReceiveViewModel()
        {
            InitializeChart();
        }

        [RelayCommand]
        private void StartReceive()
        {
            if (_isRunning) return;

            _cts = new CancellationTokenSource();
            _isRunning = true;
            _isExit = false;
            IsScanning = true;
            AddLog("✅ UDP监听已启动，等待价格数据... 按 R 键停止");

            Task.Run(() => RunUdpBackgroundLoop(_cts.Token));
        }

        [RelayCommand]
        private void ToggleCalcPanel()
        {
            IsCalcPanelExpanded = !IsCalcPanelExpanded;
        }

        [RelayCommand]
        private void ClearAll()
        {
            _lineSeries.Points.Clear();
            _priceHistory.Clear();
            _dataIndex = 0;
            PlotModel.InvalidatePlot(true);
            _logBuffer.Clear();
            Logs = string.Empty;
            AddLog("🗑️ 图表和日志已清空");
        }

        public void StopUdpListen()
        {
            if (!_isRunning) return;

            _cts.Cancel();
            _udpClient?.Close();
            _udpClient = null;
            _isRunning = false;
            _isExit = true;
            IsScanning = false;
            AddLog("🛑 UDP监听已停止");
            Task.Run(() => SavePriceHistory());
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

                        AddLog($"当前价格：{nowPrice}，区间：[{MinPrice} ~ {TargetPrice}]");

                        // 图表更新：异步投递，不阻塞购买判断
                        ScheduleChartUpdate(nowPrice);

                        // 购买判断：价格在 [minPrice, maxPrice] 区间内才购买
                        int.TryParse(MinPrice, out int min);
                        if (int.TryParse(TargetPrice, out int max) && nowPrice >= min && nowPrice <= max)
                        {
                            AddLog($"⚠️ 价格在区间内，执行点击！");
                            try
                            {
                                MouseService.MoveAndClick(mode.BuyButtonX, mode.BuyButtonY);

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
                        await Task.Delay(100, token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
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
            string timeStampedMsg = $"[{DateTime.Now:HH:mm:ss}] {message}";
            _logBuffer.Add(timeStampedMsg);

            if (_logBuffer.Count > MaxLines)
            {
                _logBuffer.RemoveAt(0);
            }

            Logs = string.Join(Environment.NewLine, _logBuffer);
        }

        #region 图表

        private void InitializeChart()
        {
            var controller = new PlotController();
            controller.UnbindAll();
            PlotController = controller;

            _lineSeries = new LineSeries
            {
                Title = "实时价格",
                Color = OxyColor.FromRgb(0, 212, 255),
                MarkerType = MarkerType.Circle,
                MarkerSize = 2,
                MarkerFill = OxyColor.FromRgb(0, 212, 255),
                StrokeThickness = 1.5,
                MarkerStroke = OxyColors.Transparent,
            };

            var model = new PlotModel
            {
                Title = "实时价格走势",
                TitleColor = OxyColor.FromRgb(200, 210, 220),
                TextColor = OxyColor.FromRgb(160, 170, 185),
                PlotAreaBorderColor = OxyColor.FromRgb(55, 65, 80),
                PlotMargins = new OxyThickness(60, 10, 20, 40),
            };
            model.Legends.Add(new OxyPlot.Legends.Legend
            {
                LegendTextColor = OxyColor.FromRgb(160, 170, 185),
                LegendBorder = OxyColors.Transparent,
                LegendBackground = OxyColors.Transparent,
                LegendPosition = OxyPlot.Legends.LegendPosition.TopRight,
                LegendPlacement = OxyPlot.Legends.LegendPlacement.Inside,
            });

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "序号",
                TitleColor = OxyColor.FromRgb(160, 170, 185),
                TextColor = OxyColor.FromRgb(140, 150, 165),
                AxislineColor = OxyColor.FromRgb(55, 65, 80),
                TicklineColor = OxyColor.FromRgb(55, 65, 80),
                MajorGridlineColor = OxyColor.FromArgb(40, 80, 90, 105),
                MinorGridlineColor = OxyColor.FromArgb(20, 80, 90, 105),
                Minimum = 0,
            });

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "价格",
                TitleColor = OxyColor.FromRgb(160, 170, 185),
                TextColor = OxyColor.FromRgb(140, 150, 165),
                AxislineColor = OxyColor.FromRgb(55, 65, 80),
                TicklineColor = OxyColor.FromRgb(55, 65, 80),
                MajorGridlineColor = OxyColor.FromArgb(40, 80, 90, 105),
                MinorGridlineColor = OxyColor.FromArgb(20, 80, 90, 105),
                Minimum = 0,
            });

            model.Series.Add(_lineSeries);
            PlotModel = model;
        }

        /// <summary>
        /// 异步投递图表更新，绝不阻塞 UDP 接收循环
        /// </summary>
        private void ScheduleChartUpdate(int price)
        {
            var index = _dataIndex;
            _dataIndex++;
            var point = new PriceDataPoint
            {
                Index = index,
                Price = price,
                Time = DateTime.Now
            };
            _priceHistory.Add(point);

            // 脏数据过滤：<25 或 >最高价+1000 的不上图表
            if (!IsPriceClean(price))
                return;

            // 投递到 UI 线程即返回，不等待渲染完成
            Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                _lineSeries.Points.Add(new DataPoint(point.Index, point.Price));
                PlotModel.InvalidatePlot(true);
            });
        }

        private bool IsPriceClean(int price)
        {
            if (price < 25) return false;
            if (int.TryParse(TargetPrice, out int max) && max > 0 && price > max + 1000)
                return false;
            return true;
        }

        public void SavePriceHistory()
        {
            try
            {
                var json = JsonSerializer.Serialize(_priceHistory);
                File.WriteAllText(PriceHistoryPath, json);
            }
            catch { }
        }

        private static string PriceHistoryPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PriceHistoryFileName);

        #endregion
    }
}
