using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Tesseract;

public static class ScreenOcrHelper
{
    #region 常量配置
    private const string DefaultLanguage = "eng";
    private static readonly string DefaultTessDataDir = GetSafeTessDataPath();
    private const int MaxRetryTimes = 3; // 最大重试次数
    #endregion

    #region 核心功能：传入坐标 → 截图 → 识别数字（支持重试）
    /// <summary>
    /// 截取屏幕指定区域并识别其中的数字（最多重试3次）
    /// </summary>
    /// <param name="leftTopX">截图区域左上角X坐标</param>
    /// <param name="leftTopY">截图区域左上角Y坐标</param>
    /// <param name="rightBottomX">截图区域右下角X坐标</param>
    /// <param name="rightBottomY">截图区域右下角Y坐标</param>
    /// <returns>识别的数字文本</returns>
    public static string RecognizeNumberFromScreen(int leftTopX, int leftTopY, int rightBottomX, int rightBottomY)
    {
        return RecognizeNumberFromScreen(leftTopX, leftTopY, rightBottomX, rightBottomY,
                GetSafeTessDataPath(), DefaultLanguage);
    }

    /// <summary>
    /// 自定义TessData路径和语言：截取屏幕指定区域并识别其中的数字（最多重试3次）
    /// </summary>
    /// <param name="leftTopX">截图区域左上角X坐标</param>
    /// <param name="leftTopY">截图区域左上角Y坐标</param>
    /// <param name="rightBottomX">截图区域右下角X坐标</param>
    /// <param name="rightBottomY">截图区域右下角Y坐标</param>
    /// <param name="tessDataPath">TessData文件夹路径</param>
    /// <param name="language">识别语言（默认eng）</param>
    /// <returns>识别的数字文本</returns>
    public static string RecognizeNumberFromScreen(int leftTopX, int leftTopY, int rightBottomX, int rightBottomY,
        string tessDataPath, string language)
    {
        // 1. 坐标合法性校验（前置校验，不参与重试）
        if (leftTopX < 0 || leftTopY < 0 || rightBottomX < 0 || rightBottomY < 0)
            return "错误：坐标不能为负数";
        if (rightBottomX <= leftTopX || rightBottomY <= leftTopY)
            return "错误：右下角坐标必须大于左上角坐标";

        // 2. 校验TessData路径（前置校验，不参与重试）
        if (string.IsNullOrWhiteSpace(tessDataPath))
            return "错误：tessdata路径为空";
        if (!Directory.Exists(tessDataPath))
            return $"错误：tessdata文件夹不存在 [{tessDataPath}]";
        if (string.IsNullOrWhiteSpace(language))
            language = DefaultLanguage;

        // 3. 重试逻辑：最多3次识别
        int retryCount = 0;
        while (retryCount < MaxRetryTimes)
        {
            try
            {
                // 计算截图区域的宽高
                int captureWidth = rightBottomX - leftTopX;
                int captureHeight = rightBottomY - leftTopY;

                // 截取屏幕指定区域（临时内存流存储，无需写入文件）
                using (var bitmap = new Bitmap(captureWidth, captureHeight))
                using (var g = Graphics.FromImage(bitmap))
                using (var memoryStream = new MemoryStream())
                {
                    // 从屏幕拷贝指定区域到Bitmap
                    g.CopyFromScreen(leftTopX, leftTopY, 0, 0, new Size(captureWidth, captureHeight),
                        CopyPixelOperation.SourceCopy);

                    // 将Bitmap转为Tesseract可识别的Pix格式
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    memoryStream.Position = 0;
                    using (var pix = Pix.LoadFromMemory(memoryStream.ToArray()))
                    using (var engine = new TesseractEngine(tessDataPath, language, EngineMode.Default))
                    {
                        // 强制只识别数字
                        engine.SetVariable("tessedit_char_whitelist", "0123456789");

                        // 识别截图区域的数字
                        using (var page = engine.Process(pix))
                        {
                            string result = page.GetText().Trim();
                            if (!string.IsNullOrWhiteSpace(result))
                            {
                                // 识别到数字，直接返回
                                return result;
                            }

                            // 未识别到数字，记录重试次数并继续
                            retryCount++;
                            if (retryCount >= MaxRetryTimes)
                            {
                                return "未识别到数字（已重试3次）";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                retryCount++;
                // 最后一次重试仍异常，返回错误信息
                string errorMsg = ex.ToString();
                if (retryCount >= MaxRetryTimes)
                {
                    return $"识别失败（已重试3次）：{errorMsg}";
                }
            }
        }
        // 理论上不会走到这里，兜底返回
        return "未识别到数字（已重试3次）";
    }
    #endregion

    #region 兼容原有功能：识别本地图片（保留，如需重试可参考上述逻辑修改）
    public static string RecognizeText(string imagePath)
    {
        return RecognizeText(imagePath, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultTessDataDir), DefaultLanguage);
    }

    public static string RecognizeText(string imagePath, string tessDataPath, string language)
    {
        if (string.IsNullOrWhiteSpace(imagePath)) return "图片路径为空";
        if (!File.Exists(imagePath)) return $"图片文件不存在 [{imagePath}]";
        if (string.IsNullOrWhiteSpace(tessDataPath)) return "tessdata路径为空";
        if (!Directory.Exists(tessDataPath)) return $"tessdata文件夹不存在 [{tessDataPath}]";
        if (string.IsNullOrWhiteSpace(language)) language = DefaultLanguage;

        try
        {
            using (var engine = new TesseractEngine(tessDataPath, language, EngineMode.Default))
            {
                engine.SetVariable("tessedit_char_whitelist", "0123456789");

                using (var img = Pix.LoadFromFile(imagePath))
                using (var page = engine.Process(img))
                {
                    string raw = page.GetText().Trim();
                    return string.IsNullOrWhiteSpace(raw) ? "未识别到文本" : raw;
                }
            }
        }
        catch (Exception ex)
        {
            return $"识别失败：{ex.Message}";
        }
    }
    #endregion

    private static string GetSafeTessDataPath()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "tessdata"
        );
    }
}