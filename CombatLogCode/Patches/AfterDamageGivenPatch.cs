using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using System.Reflection;
using System.Reflection.Emit;
using Sts2CombatLog.CombatLogCode.ModScenes;
using Sts2CombatLog.CombatLogCode.Utils;

namespace Sts2CombatLog.CombatLogCode.Patches
{
    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterDamageGiven), MethodType.Async)]
    class AfterDamageGivenPatch
    {
        [HarmonyTranspiler]
        static List<CodeInstruction> AfterDamageGiven(ILGenerator generator, IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            return AsyncMethodCall.Create(generator, instructions, original,
                AccessTools.Method(typeof(AfterDamageGivenPatch), nameof(BeforeAfterDamageGivenHook)), beforeState: original);
        }

        private static async Task BeforeAfterDamageGivenHook(DamageResult result, Creature target, CardModel? cardSource)
        {
            string damageLogStr = target.Name + " took " + result.UnblockedDamage + " damage (" + result.BlockedDamage + " blocked)";
            if (result.WasTargetKilled)
            {
                damageLogStr += " and was killed";
            }
            if (cardSource != null)
            {
                damageLogStr += " [" + cardSource.Id + "]";
            }
            NCombatLogWindow.AddLog(damageLogStr + ".", NCombatLogWindow.CombatEntryType.Damage);
        }
    }
}
