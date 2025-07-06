using System.Windows.Input;

namespace SharpestBeak.UI.Commands;

internal static class AppCommands
{
    public static readonly RoutedUICommand Exit = new(
        "E_xit",
        "Exit",
        typeof(AppCommands),
        new InputGestureCollection { new KeyGesture(Key.Q, ModifierKeys.Control) });

    public static readonly RoutedUICommand Play = new(
        "_Play",
        nameof(Play),
        typeof(AppCommands),
        new InputGestureCollection { new KeyGesture(Key.F5) });

    public static readonly RoutedUICommand DefaultPreset = new(
        "_Default",
        nameof(DefaultPreset),
        typeof(AppCommands),
        new InputGestureCollection { new KeyGesture(Key.D, ModifierKeys.Control) });

    public static readonly RoutedUICommand SmallPreset = new(
        "_Small",
        nameof(SmallPreset),
        typeof(AppCommands),
        new InputGestureCollection { new KeyGesture(Key.D1, ModifierKeys.Control) });

    public static readonly RoutedUICommand MediumPreset = new(
        "_Medium",
        nameof(MediumPreset),
        typeof(AppCommands),
        new InputGestureCollection { new KeyGesture(Key.D2, ModifierKeys.Control) });

    public static readonly RoutedUICommand LargePreset = new(
        "_Large",
        nameof(LargePreset),
        typeof(AppCommands),
        new InputGestureCollection { new KeyGesture(Key.D3, ModifierKeys.Control) });

    public static readonly RoutedUICommand ExtraLargePreset = new(
        "_Extra Large",
        nameof(ExtraLargePreset),
        typeof(AppCommands),
        new InputGestureCollection { new KeyGesture(Key.D4, ModifierKeys.Control) });

    public static readonly RoutedUICommand MaximumPreset = new(
        "Ma_ximum",
        nameof(MaximumPreset),
        typeof(AppCommands),
        new InputGestureCollection { new KeyGesture(Key.D0, ModifierKeys.Control) });
}