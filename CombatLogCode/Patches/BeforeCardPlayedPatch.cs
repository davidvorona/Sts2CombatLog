using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;
using System.Reflection;
using System.Reflection.Emit;
using Sts2CombatLog.CombatLogCode.ModScenes;
using Sts2CombatLog.CombatLogCode.Utils;

namespace Sts2CombatLog.CombatLogCode.Patches
{
    [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCardPlayed), MethodType.Async)]
    class BeforeCardPlayedPatch
    {
        [HarmonyTranspiler]
        static List<CodeInstruction> BeforePlay(ILGenerator generator, IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            return AsyncMethodCall.Create(generator, instructions, original,
                AccessTools.Method(typeof(BeforeCardPlayedPatch), nameof(BeforePlayHooks)), beforeState: original);
        }

        private static async Task BeforePlayHooks(CardPlay cardPlay)
        {
            NCombatLogWindow.AddLog(cardPlay.Card.Owner.Creature.Name + " played " + cardPlay.Card.Id + ".", NCombatLogWindow.CombatEntryType.CardPlay);
        }
    }
}
