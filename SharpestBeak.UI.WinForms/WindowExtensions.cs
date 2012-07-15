using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace SharpestBeak.UI.WinForms
{
    /// <summary>
    ///     Contains extension methods for <see cref="System.Windows.Window"/> class.
    /// </summary>
    internal static class WindowExtensions
    {
        #region Public Methods

        public static MessageBoxResult ShowMessage(
            this Window window,
            string text,
            string caption,
            MessageBoxButton buttons,
            MessageBoxImage icon)
        {
            return MessageBox.Show(
                window,
                text,
                string.IsNullOrEmpty(caption) ? window.Title : caption + " – " + window.Title,
                buttons,
                icon);
        }

        public static void ShowInfoMessage(this Window window, string text, string caption = null)
        {
            ShowMessage(window, text, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowErrorMessage(
            this Window window,
            string text,
            string caption = null,
            MessageBoxImage icon = MessageBoxImage.Error)
        {
            ShowMessage(window, text, caption, MessageBoxButton.OK, icon);
        }

        public static void ShowErrorMessage(this Window window, Exception exception)
        {
            #region Argument Check

            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            #endregion

            ShowErrorMessage(
                window,
                string.Format("An error occurred:{0}{0}{1}", Environment.NewLine, exception.ToString()),
                "Error");
        }

        public static MessageBoxResult ShowQuestion(
            this Window window,
            string text,
            MessageBoxButton buttons = MessageBoxButton.YesNo,
            string caption = null)
        {
            return ShowMessage(window, text, caption, buttons, MessageBoxImage.Question);
        }

        #endregion
    }
}