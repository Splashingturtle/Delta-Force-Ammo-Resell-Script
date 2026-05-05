using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using OpenCvSharp;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.Local;

public static class ScreenOcrHelper
{
    #region 引擎缓存（仅识别模型，跳过检测/分类，大幅提速）
    private static readonly object _engineLock = new();
    private static PaddleOcrRecognizer? _cachedRecognizer;
    private static bool _engineInitFailed;

    private static PaddleOcrRecognizer? GetRecognizer()
    {
        lock (_engineLock)
        {
            if (_cachedRecognizer != null)
                return _cachedRecognizer;

            if (_engineInitFailed)
                return null;

            try
            {
                // ChineseV4 识别模型：支持中文、英文、数字
                var recModel = LocalRecognizationModel.ChineseV4;

                // 优先 Onnx，失败回退 Mkldnn
                try
                {
                    _cachedRecognizer = new PaddleOcrRecognizer(recModel, PaddleDevice.Onnx());
                }
                catch
                {
                    _cachedRecognizer = new PaddleOcrRecognizer(recModel, PaddleDevice.Mkldnn());
                }

                return _cachedRecognizer;
            }
            catch
            {
                _engineInitFailed = true;
                return null;
            }
        }
    }
    #endregion

    #region 核心功能：传入坐标 → 截图 → 预处理 → 多策略识别数字
    public static string RecognizeNumberFromScreen(int leftTopX, int leftTopY, int rightBottomX, int rightBottomY)
    {
        if (leftTopX < 0 || leftTopY < 0 || rightBottomX < 0 || rightBottomY < 0)
            return "-1";
        if (rightBottomX <= leftTopX || rightBottomY <= leftTopY)
            return "-1";

        int captureWidth = rightBottomX - leftTopX;
        int captureHeight = rightBottomY - leftTopY;

        using (var bitmap = new Bitmap(captureWidth, captureHeight))
        {
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(leftTopX, leftTopY, 0, 0,
                    new System.Drawing.Size(captureWidth, captureHeight), CopyPixelOperation.SourceCopy);
            }

            // 策略1: 灰度 + 自动反转检测
            string result = TryRecognize(bitmap, PreprocessMode.Normal);
            if (IsValidNumber(result)) return CleanNumber(result);

            // 策略2: 灰度 + 强制反转
            result = TryRecognize(bitmap, PreprocessMode.Inverted);
            if (IsValidNumber(result)) return CleanNumber(result);
        }

        return "-1";
    }

    private enum PreprocessMode { Normal, Inverted }

    private static string TryRecognize(Bitmap original, PreprocessMode mode)
    {
        var recognizer = GetRecognizer();
        if (recognizer == null) return string.Empty;

        using (var processed = PreprocessImage(original, mode))
        using (var mat = BitmapToMat(processed))
        {
            if (mat.Empty()) return string.Empty;

            // 仅运行识别模型（无检测/分类），直接识别全图文字
            PaddleOcrRecognizerResult result = recognizer.Run(mat);
            return result.Text.Trim();
        }
    }

    private static Mat BitmapToMat(Bitmap bitmap)
    {
        using (var ms = new MemoryStream())
        {
            bitmap.Save(ms, ImageFormat.Png);
            ms.Position = 0;
            return Cv2.ImDecode(ms.ToArray(), ImreadModes.Grayscale);
        }
    }

    private static Bitmap PreprocessImage(Bitmap original, PreprocessMode mode)
    {
        int w = original.Width;
        int h = original.Height;

        // 转灰度
        var result = new Bitmap(w, h);
        using (var g = Graphics.FromImage(result))
        {
            var grayMatrix = new ColorMatrix(new float[][] {
                new float[] { 0.299f, 0.299f, 0.299f, 0, 0 },
                new float[] { 0.587f, 0.587f, 0.587f, 0, 0 },
                new float[] { 0.114f, 0.114f, 0.114f, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { 0, 0, 0, 0, 1 }
            });
            var attrs = new ImageAttributes();
            attrs.SetColorMatrix(grayMatrix);
            g.DrawImage(original, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel, attrs);
        }

        // 判断是否需要反转（游戏UI常见亮色文字/深色背景）
        bool shouldInvert = (mode == PreprocessMode.Inverted) ||
                           (mode == PreprocessMode.Normal && NeedsInvert(result));

        if (shouldInvert)
        {
            using (var g = Graphics.FromImage(result))
            {
                var invMatrix = new ColorMatrix(new float[][] {
                    new float[] { -1, 0, 0, 0, 0 },
                    new float[] { 0, -1, 0, 0, 0 },
                    new float[] { 0, 0, -1, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { 1, 1, 1, 0, 1 }
                });
                var attrs = new ImageAttributes();
                attrs.SetColorMatrix(invMatrix);
                g.DrawImage(result, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel, attrs);
            }
        }

        return result;
    }

    private static bool NeedsInvert(Bitmap bmp)
    {
        int w = bmp.Width, h = bmp.Height;

        int bgLum = 0;
        bgLum += bmp.GetPixel(0, 0).R;
        bgLum += bmp.GetPixel(w - 1, 0).R;
        bgLum += bmp.GetPixel(0, h - 1).R;
        bgLum += bmp.GetPixel(w - 1, h - 1).R;
        bgLum /= 4;

        int cx = w / 2, cy = h / 2;
        int centerLum = 0, count = 0;
        for (int y = cy - 2; y <= cy + 2; y++)
        {
            for (int x = cx - 4; x <= cx + 4; x++)
            {
                int sx = Math.Clamp(x, 0, w - 1);
                int sy = Math.Clamp(y, 0, h - 1);
                centerLum += bmp.GetPixel(sx, sy).R;
                count++;
            }
        }
        centerLum /= count;

        return bgLum < centerLum - 15;
    }

    private static bool IsValidNumber(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        return text.Replace(",", "").Trim().All(c => c >= '0' && c <= '9');
    }

    private static string CleanNumber(string text)
    {
        return text.Replace(",", "").Trim();
    }
    #endregion

    #region 兼容原有功能：识别本地图片
    public static string RecognizeText(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath)) return "-1";
        if (!File.Exists(imagePath)) return "-1";

        try
        {
            using (var mat = Cv2.ImRead(imagePath, ImreadModes.Grayscale))
            {
                if (mat.Empty()) return "-1";

                var recognizer = GetRecognizer();
                if (recognizer == null) return "-1";

                PaddleOcrRecognizerResult result = recognizer.Run(mat);
                string raw = result.Text.Trim();
                return IsValidNumber(raw) ? CleanNumber(raw) : "-1";
            }
        }
        catch
        {
            return "-1";
        }
    }
    #endregion
}
