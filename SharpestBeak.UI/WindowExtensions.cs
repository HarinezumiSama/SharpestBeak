using System;
using System.Windows;

namespace SharpestBeak.UI;

/// <summary>
///     Contains extension methods for <see cref="System.Windows.Window"/> class.
/// </summary>
internal static class WindowExtensions
{
    public static MessageBoxResult ShowMessage(
        this Window window,
        string text,
        string caption,
        MessageBoxButton buttons,
        MessageBoxImage icon)
        => MessageBox.Show(
            window,
            text,
            string.IsNullOrEmpty(caption) ? window.Title : caption + " – " + window.Title,
            buttons,
            icon);

    public static void ShowInfoMessage(this Window window, string text, string caption = null)
        => ShowMessage(window, text, caption, MessageBoxButton.OK, MessageBoxImage.Information);

    public static void ShowErrorMessage(
        this Window window,
        string text,
        string caption = null,
        MessageBoxImage icon = MessageBoxImage.Error)
        => ShowMessage(window, text, caption, MessageBoxButton.OK, icon);

    public static void ShowErrorMessage(this Window window, Exception exception)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        ShowErrorMessage(
            window,
            $"An error occurred:{Environment.NewLine}{Environment.NewLine}{exception}",
            "Error");
    }

    public static MessageBoxResult ShowQuestion(
        this Window window,
        string text,
        MessageBoxButton buttons = MessageBoxButton.YesNo,
        string caption = null)
        => ShowMessage(window, text, caption, buttons, MessageBoxImage.Question);
}