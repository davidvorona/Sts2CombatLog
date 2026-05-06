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
    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterDamageReceived), MethodType.Async)]
    class AfterDamageReceivedPatch
    {
        [HarmonyTranspiler]
        static List<CodeInstruction> AfterDamageReceived(ILGenerator generator, IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            return AsyncMethodCall.Create(generator, instructions, original,
                AccessTools.Method(typeof(AfterDamageReceivedPatch), nameof(BeforeAfterDamageReceivedHook)), beforeState: original);
        }

        private static async Task BeforeAfterDamageReceivedHook(Creature target, DamageResult result, Creature? dealer, CardModel? cardSource)
        {
            string damageLogStr = target.Name + " took " + result.UnblockedDamage + " damage (" + result.BlockedDamage + " blocked)";
            if (cardSource != null)
            {
                damageLogStr += " [" + cardSource.Id + "]";
            }
            NCombatLogWindow.AddLog(damageLogStr + ".", NCombatLogWindow.CombatEntryType.Damage);
        }
    }
}
