using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SharpestBeak.UI.WinForms
{
    internal static class FormExtensions
    {
        #region Public Methods

        public static DialogResult ShowMessage(
            this Form form,
            string text,
            string caption,
            MessageBoxButtons buttons,
            MessageBoxIcon icon)
        {
            return MessageBox.Show(
                form,
                text,
                string.IsNullOrEmpty(caption) ? form.Text : caption + " – " + form.Text,
                buttons,
                icon);
        }

        public static void ShowInfoMessage(this Form form, string text, string caption = null)
        {
            ShowMessage(form, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ShowErrorMessage(
            this Form form,
            string text,
            string caption = null,
            MessageBoxIcon icon = MessageBoxIcon.Error)
        {
            ShowMessage(form, text, caption, MessageBoxButtons.OK, icon);
        }

        public static void ShowErrorMessage(this Form form, Exception exception)
        {
            #region Argument Check

            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            #endregion

            ShowErrorMessage(
                form,
                string.Format("An error occurred:{0}{0}{1}", Environment.NewLine, exception),
                "Error");
        }

        public static DialogResult ShowQuestion(
            this Form form,
            string text,
            MessageBoxButtons buttons = MessageBoxButtons.YesNo,
            string caption = null)
        {
            return ShowMessage(form, text, caption, buttons, MessageBoxIcon.Question);
        }

        #endregion
    }
}