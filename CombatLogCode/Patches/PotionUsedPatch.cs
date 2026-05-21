using HarmonyLib;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using Sts2CombatLog.CombatLogCode.ModScenes;

namespace Sts2CombatLog.CombatLogCode.Patches
{
    [HarmonyPatch(typeof(CombatHistory), nameof(CombatHistory.PotionUsed))]
    class PotionUsedPatch
    {
        [HarmonyPostfix]
        public static void Postfix(PotionModel __1, Creature? __2)
        {
            var potion = __1;
            var receiver = __2;
            if (potion is null) return;

            var potionName = potion.Title?.GetFormattedText() ?? potion.Id?.Entry ?? potion.GetType().Name;
            var ownerName = potion.Owner.Creature.Name;
            string potionStr = ownerName + " used a " + potionName;
            if (receiver?.Player is not null && receiver.Player.NetId != potion.Owner.NetId)
            {
                potionStr += " on " + receiver.Name;
            }
            NCombatLogWindow.AddLog(potionStr + ".", NCombatLogWindow.CombatEntryType.Potion);
        }
    }
}
