using HarmonyLib;
using Sts2CombatLog.CombatLogCode.ModScenes;
using MegaCrit.Sts2.Core.Rooms;

namespace Sts2CombatLog.CombatLogCode.Patches;

[HarmonyPatch(typeof(CombatRoom), nameof(CombatRoom.StartCombat))]
class CombatRoomPatch
{
    [HarmonyPrefix]
    public static void Prefix()
    {
        NCombatLogWindow.AddLog("Combat " + (++MainFile.CombatCounter) + " has started.", NCombatLogWindow.CombatEntryType.Start);
    }
}

[HarmonyPatch(typeof(CombatRoom), nameof(CombatRoom.OnCombatEnded))]
class AfterCombatEndPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        NCombatLogWindow.AddLog("Combat has ended.", NCombatLogWindow.CombatEntryType.Misc);
    }
}
