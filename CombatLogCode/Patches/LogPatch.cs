using Sts2CombatLog.CombatLogCode.ModScenes;
using Sts2CombatLog.CombatLogCode.Commands;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace Sts2CombatLog.CombatLogCode.Patches
{
    [HarmonyPatch(typeof(NMainMenu), nameof(NMainMenu._Ready))]
    class NMainMenuReadyOpenLogWindowPatch
    {
        private static bool _hasOpenedOnStartup;

        [HarmonyPostfix]
        private static void Postfix()
        {
            if (_hasOpenedOnStartup) return;

            _hasOpenedOnStartup = true;
            if (!NCombatLogWindow.IsOpen)
                OpenLogWindow.OpenWindow(stealFocus: false);
        }
    }
}
