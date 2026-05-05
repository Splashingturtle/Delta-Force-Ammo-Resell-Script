# AmmoResellScript

基于 WPF (.NET 8.0) 的桌面自动化交易工具，使用 PaddleOCR 实时识别屏幕价格，支持自动购买与局域网价格广播。

## 功能

- **自动购买 (AutoBuy)** — 通过 OCR 实时识别游戏内商品价格，低于目标价自动点击购买
- **UDP 广播接收 (Receive)** — 监听局域网内其他实例广播的价格，联动触发购买
- **可视化配置 (Settings)** — 图形化配置屏幕坐标区域，支持点击取样绑定
- **Acrylic 模糊窗口** — 适配 Windows 10/11 的亚克力毛玻璃窗口效果

## 软件截图

| 自动购买 | UDP 接收 | 配置设置 |
|:---:|:---:|:---:|
| ![AutoBuy](images/Screenshot%202026-05-05%20132642.png) | ![Receive](images/Screenshot%202026-05-05%20132750.png) | ![Settings](images/Screenshot%202026-05-05%20132802.png) |

## 技术栈

| 组件 | 说明 |
|------|------|
| .NET 8.0 + WPF | 桌面框架 |
| CommunityToolkit.Mvvm | MVVM 架构 |
| Sdcb.PaddleOCR | 本地 OCR 文字识别 |
| OpenCvSharp4 | 图像预处理 |
| Microsoft.Extensions.DependencyInjection | 依赖注入 |

## 环境要求

- Windows 10 1803+ 或 Windows 11
- .NET 8.0 Desktop Runtime
- x64 架构

## 快速开始

```bash
# 克隆仓库
git clone https://github.com/your-username/AmmoResellScript.git

# 还原依赖
dotnet restore

# 编译运行
dotnet run --project AmmoResellScript
```

## 项目结构

```
AmmoResellScript/
├── App.xaml.cs                # 应用入口，DI 容器初始化
├── MainWindow.xaml.cs         # 主窗口，包含 Acrylic 模糊和自定义标题栏
├── MainWindowViewModel.cs     # 主窗口 ViewModel
├── AssemblyInfo.cs            # 程序集信息
├── Model/
│   └── UserModel.cs           # 用户配置数据模型
├── ViewModels/
│   ├── AutoBuyViewModel.cs    # 自动购买逻辑
│   ├── ReceiveViewModel.cs    # UDP 接收逻辑
│   └── SettingViewModel.cs    # 配置界面逻辑
├── Views/
│   ├── AutoBuyView.xaml.cs    # 自动购买视图
│   ├── ReceiveView.xaml.cs    # 接收视图
│   └── SettingView.xaml.cs    # 设置视图
├── Services/
│   ├── ConfigService.cs       # 配置文件读写服务
│   ├── FastKeyboard.cs        # 键盘模拟服务
│   ├── MouseService.cs        # 鼠标模拟服务
│   ├── ScreenOcrHelper.cs     # 屏幕 OCR 识别（PaddleOCR）
│   └── UdpBroadcastService.cs # UDP 广播服务
├── UserControls/
│   └── Navigation.xaml.cs     # 导航栏控件
├── Helpers/
│   └── TextBoxHelper.cs       # 文本框辅助工具
├── Resources/                 # 资源文件
├── Properties/                # 项目属性
└── images/                    # 图片资源
```

## 注意事项

- 首次运行时会自动下载 PaddleOCR 识别模型
- 使用前需在设置页面配置各项屏幕坐标
- 按 `R` 键可紧急停止自动购买/UDP 监听
- 仅用于学习交流，请勿用于违规操作
