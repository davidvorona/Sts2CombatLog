using Sts2CombatLog.CombatLogCode.ModScenes;
using Godot;

namespace Sts2CombatLog.CombatLogCode.Commands;

/// <summary>
/// Sizes and positions the log window relative to the game window so ultrawide / multi-monitor
/// setups do not inherit a full-screen-width two-thirds rectangle.
/// </summary>
internal static class LogWindowPlacement
{
    internal static void SetupPosition(NCombatLogWindow logWindow, Window host)
    {
        var targetScreen = host.CurrentScreen;
        var screenCount = DisplayServer.GetScreenCount();
        if (screenCount > 1)
        {
            for (var i = 0; i < screenCount; ++i)
            {
                if (i == targetScreen) continue;

                targetScreen = i;
                break;
            }
        }
        logWindow.CurrentScreen = targetScreen;

        if (host.ContentScaleFactor > 0f)
            logWindow.ContentScaleFactor = host.ContentScaleFactor;

        var screenRect = DisplayServer.ScreenGetUsableRect(targetScreen);

        logWindow.Size = ComputeDefaultSize(targetScreen == host.CurrentScreen ? host.Size : screenRect.Size);

        logWindow.Position = screenRect.Position + screenRect.Size / 2 - logWindow.Size / 2;
    }

    internal static Vector2I ComputeDefaultSize(Vector2I hostSize)
    {
        if (hostSize.X <= 0 || hostSize.Y <= 0)
            return new Vector2I(800, 600);

        int tw = hostSize.X * 2 / 3;
        int th = hostSize.Y * 2 / 3;

        // Avoid an extremely wide panel on ultrawide / super-ultrawide fullscreen.
        int maxReadableW = Mathf.Clamp((int)(th * 2.35f), 960, 2048);
        tw = Mathf.Min(tw, maxReadableW);

        tw = Mathf.Min(tw, Mathf.Max(320, hostSize.X - 32));
        th = Mathf.Min(th, Mathf.Max(200, hostSize.Y - 32));

        return new Vector2I(tw, th);
    }
}
