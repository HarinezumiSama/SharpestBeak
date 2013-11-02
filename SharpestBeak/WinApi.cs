using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

namespace SharpestBeak
{
    internal static class WinApi
    {
        #region Constants and Fields

        //// ReSharper disable InconsistentNaming

        /// <summary>
        ///     Hides the window and activates another window.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore",
            Justification = "Reviewed. Suppression is OK here.")]
        public const int SW_HIDE = 0;

        /// <summary>
        ///     Activates and displays a window. If the window is minimized
        ///     or maximized, the system restores it to its original size and
        ///     position. An application should specify this flag when displaying
        ///     the window for the first time.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore",
            Justification = "Reviewed. Suppression is OK here.")]
        public const int SW_SHOWNORMAL = 1;

        /// <summary>
        ///     Activates the window and displays it as a minimized window.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore",
            Justification = "Reviewed. Suppression is OK here.")]
        public const int SW_SHOWMINIMIZED = 2;

        /// <summary>
        ///     Activates the window and displays it as a maximized window.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore",
            Justification = "Reviewed. Suppression is OK here.")]
        public const int SW_SHOWMAXIMIZED = 3;

        /// <summary>
        ///     Maximizes the specified window.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore",
            Justification = "Reviewed. Suppression is OK here.")]
        public const int SW_MAXIMIZE = 3;

        /// <summary>
        ///     Displays a window in its most recent size and position.
        ///     This value is similar to "ShowNormal", except the window is not active.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore",
            Justification = "Reviewed. Suppression is OK here.")]
        public const int SW_SHOWNOACTIVATE = 4;

        /// <summary>
        ///     Activates the window and displays it in its current size and position.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore",
            Justification = "Reviewed. Suppression is OK here.")]
        public const int SW_SHOW = 5;

        /// <summary>
        ///     Minimizes the specified window and activates the next top-level window in the Z order.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore",
            Justification = "Reviewed. Suppression is OK here.")]
        public const int SW_MINIMIZE = 6;

        /// <summary>
        ///     Displays the window as a minimized window. This value is
        ///     similar to SW_SHOWMINIMIZED, except the window is not activated.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore",
            Justification = "Reviewed. Suppression is OK here.")]
        public const int SW_SHOWMINNOACTIVE = 7;

        /// <summary>
        ///     Displays the window in its current size and position. This
        ///     value is similar to SW_SHOW, except the window is not activated.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore",
            Justification = "Reviewed. Suppression is OK here.")]
        public const int SW_SHOWNA = 8;

        /// <summary>
        ///     Activates and displays the window. If the window is
        ///     minimized or maximized, the system restores it to its original size
        ///     and position. An application should specify this flag when restoring
        ///     a minimized window.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore",
            Justification = "Reviewed. Suppression is OK here.")]
        public const int SW_RESTORE = 9;

        /// <summary>
        ///     Sets the show state based on the SW_ value specified in the
        ///     STARTUPINFO structure passed to the CreateProcess function by the
        ///     program that started the application.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore",
            Justification = "Reviewed. Suppression is OK here.")]
        public const int SW_SHOWDEFAULT = 10;

        /// <summary>
        ///     Windows 2000/XP: Minimizes a window, even if the thread
        ///     that owns the window is hung. This flag should only be used when
        ///     minimizing windows from a different thread.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore",
            Justification = "Reviewed. Suppression is OK here.")]
        public const int SW_FORCEMINIMIZE = 11;

        //// ReSharper restore InconsistentNaming

        #endregion

        #region Public Methods

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        #endregion
    }
}