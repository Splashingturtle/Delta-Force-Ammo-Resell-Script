using System.Windows;
using System.Windows.Controls;

namespace AmmoResellScript.Helpers;

/// <summary>
/// TextBox 附加属性：启用后自动滚动到底部
/// </summary>
public static class TextBoxHelper
{
    public static readonly DependencyProperty AutoScrollToEndProperty =
        DependencyProperty.RegisterAttached(
            "AutoScrollToEnd",
            typeof(bool),
            typeof(TextBoxHelper),
            new PropertyMetadata(false, OnAutoScrollToEndChanged));

    public static bool GetAutoScrollToEnd(DependencyObject obj) =>
        (bool)obj.GetValue(AutoScrollToEndProperty);

    public static void SetAutoScrollToEnd(DependencyObject obj, bool value) =>
        obj.SetValue(AutoScrollToEndProperty, value);

    private static void OnAutoScrollToEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox || e.NewValue is not true) return;

        textBox.TextChanged += (_, _) =>
        {
            textBox.ScrollToEnd();
        };
    }
}
