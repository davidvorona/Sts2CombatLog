using BaseLib.Utils.Patching;
using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using System.Reflection;
using System.Reflection.Emit;
using Sts2CombatLog.CombatLogCode.ModScenes;

namespace Sts2CombatLog.CombatLogCode.Patches
{
    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterPowerAmountChanged), MethodType.Async)]
    class AfterPowerAmountChangedPatch
    {
        [HarmonyTranspiler]
        static List<CodeInstruction> AfterPowerAmountChanged(ILGenerator generator, IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            return AsyncMethodCall.Create(generator, instructions, original,
                AccessTools.Method(typeof(AfterPowerAmountChangedPatch), nameof(BeforeAfterPowerAmountChangedHook)), beforeState: original);
        }

        private static async Task BeforeAfterPowerAmountChangedHook(PowerModel power, decimal amount, CardModel? cardSource)
        {
            string powerChangeStr = power.Id.ToString() + " on " + power.Owner.Name;
            if (amount > 0) powerChangeStr += " increased by ";
            else if (amount < 0) powerChangeStr += " decreased by ";
            powerChangeStr += Math.Abs(amount) + " to " + power.Amount;
            if (cardSource != null)
            {
                powerChangeStr += " [" + cardSource.Id + "]";
            }
            NCombatLogWindow.AddLog(powerChangeStr + ".", NCombatLogWindow.CombatEntryType.Power);
        }
    }
}
