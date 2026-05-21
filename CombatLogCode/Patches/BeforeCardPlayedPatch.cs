using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using Sts2CombatLog.CombatLogCode.ModScenes;

namespace Sts2CombatLog.CombatLogCode.Patches
{
    [HarmonyPatch(typeof(CardModel), "OnPlayWrapper")]
    class BeforeCardPlayedPatch
    {
        [HarmonyPrefix]
        public static void Prefix(CardModel __instance)
        {
            var cardName = __instance.Title ?? __instance.GetType().Name;
            NCombatLogWindow.AddLog(__instance.Owner.Creature.Name + " played " + cardName + ".", NCombatLogWindow.CombatEntryType.CardPlay);
        }
    }
}
