using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace SharpestBeak.UI.Commands
{
    internal static class AppCommands
    {
        #region Constants and Fields

        public static readonly RoutedUICommand Play = new RoutedUICommand(
            "_Play",
            "Play",
            typeof(AppCommands),
            new InputGestureCollection { new KeyGesture(Key.F5) });

        public static readonly RoutedUICommand DefaultPreset = new RoutedUICommand(
            "_Default",
            "DefaultPreset",
            typeof(AppCommands),
            new InputGestureCollection { new KeyGesture(Key.D, ModifierKeys.Control) });

        public static readonly RoutedUICommand SmallPreset = new RoutedUICommand(
            "_Small",
            "SmallPreset",
            typeof(AppCommands),
            new InputGestureCollection { new KeyGesture(Key.D1, ModifierKeys.Control) });

        public static readonly RoutedUICommand MediumPreset = new RoutedUICommand(
            "_Medium",
            "MediumPreset",
            typeof(AppCommands),
            new InputGestureCollection { new KeyGesture(Key.D2, ModifierKeys.Control) });

        public static readonly RoutedUICommand LargePreset = new RoutedUICommand(
            "_Large",
            "LargePreset",
            typeof(AppCommands),
            new InputGestureCollection { new KeyGesture(Key.D3, ModifierKeys.Control) });

        public static readonly RoutedUICommand ExtraLargePreset = new RoutedUICommand(
            "E_xtra Large",
            "ExtraLargePreset",
            typeof(AppCommands),
            new InputGestureCollection { new KeyGesture(Key.D4, ModifierKeys.Control) });

        #endregion
    }
}