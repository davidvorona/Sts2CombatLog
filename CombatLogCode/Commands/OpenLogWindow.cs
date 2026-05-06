using Godot;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes;
using Sts2CombatLog.CombatLogCode.ModScenes;

namespace Sts2CombatLog.CombatLogCode.Commands
{
    public class OpenLogWindow : AbstractConsoleCmd
    {
        public override string CmdName => "showcombatlog";
        public override string Args => "";
        public override string Description => "Open combat log window";
        public override bool IsNetworked => false;

        public override CmdResult Process(Player? issuingPlayer, string[] args)
        {
            OpenWindow(stealFocus: true);
            return new CmdResult(true, "Opened combat log.");
        }

        public static void OpenWindow(bool stealFocus)
        {
            if (!MainFile.IsMainThread)
            {
                Log.Info("OpenWindow called when not on main thread");
                return;
            }

            var instance = NGame.Instance;
            if (instance == null) return;

            try
            {
                Window window = instance.GetWindow();
                window.GuiEmbedSubwindows = false;
                var scene = ResourceLoader.Load<PackedScene>("res://CombatLog/scenes/CombatLogWindow.tscn").Instantiate<NCombatLogWindow>();

                // Prevent flicker on open (open in the final position)
                scene.Visible = false;
                window.AddChildSafely(scene);
                LogWindowPlacement.SetupPosition(scene, window);
                scene.Visible = true;

                if (!stealFocus)
                    window.GrabFocus();
            }
            catch (Exception e)
            {
                Log.Info($"Failed to open combat log window: {e}");
            }
        }
    }
}
