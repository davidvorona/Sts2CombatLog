using HarmonyLib;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using Sts2CombatLog.CombatLogCode.ModScenes;

namespace Sts2CombatLog.CombatLogCode.Patches
{
    [HarmonyPatch(typeof(CombatHistory), nameof(CombatHistory.BlockGained))]
    class AfterBlockGainedPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Creature __1, int __2, CardPlay? __4)
        {
            var creature = __1;
            var amount = __2;
            var cardSource = __4;
            string blockLogStr = creature.Name + " gained " + amount + " block";
            if (cardSource != null)
            {
                var cardName = cardSource.Card.Title ?? cardSource.Card.GetType().Name;
                blockLogStr += " [" + cardName + "]";
            }
            NCombatLogWindow.AddLog(blockLogStr + ".", NCombatLogWindow.CombatEntryType.Block);
        }
    }
}
