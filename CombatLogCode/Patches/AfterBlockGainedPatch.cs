using BaseLib.Utils.Patching;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using System.Reflection;
using System.Reflection.Emit;
using Sts2CombatLog.CombatLogCode.ModScenes;

namespace Sts2CombatLog.CombatLogCode.Patches
{
    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterBlockGained), MethodType.Async)]
    class AfterBlockGainedPatch
    {
        [HarmonyTranspiler]
        static List<CodeInstruction> AfterBlockGained(ILGenerator generator, IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            return AsyncMethodCall.Create(generator, instructions, original,
                AccessTools.Method(typeof(AfterBlockGainedPatch), nameof(BeforeAfterBlockGainedHook)), beforeState: original);
        }

        private static async Task BeforeAfterBlockGainedHook(Creature creature, decimal amount, CardModel? cardSource)
        {
            string blockLogStr = creature.Name + " gained " + amount + " block";
            if (cardSource != null)
            {
                blockLogStr += " [" + cardSource.Id + "]";
            }
            NCombatLogWindow.AddLog(blockLogStr + ".", NCombatLogWindow.CombatEntryType.Block);
        }
    }
}
