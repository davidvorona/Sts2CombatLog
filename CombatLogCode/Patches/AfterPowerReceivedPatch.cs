using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using Sts2CombatLog.CombatLogCode.ModScenes;
using Sts2CombatLog.CombatLogCode.Utils;
using System.Reflection;
using System.Reflection.Emit;

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
            var powerId = power.Id?.Entry ?? power.GetType().Name;
            var powerTitle = power.Title?.GetFormattedText() ?? powerId;

            if (power.Amount == -1)
            {
                NCombatLogWindow.AddLog(powerTitle + " applied to " + power.Owner.Name + ".", NCombatLogWindow.CombatEntryType.Power);
                return;
            }

            string powerChangeStr = powerTitle + " on " + power.Owner.Name;
            if (amount > 0)
            {
                powerChangeStr += " increased to " + power.Amount + " (+" + amount + ")";
            }
            else if (amount < 0)
            {
                powerChangeStr += " decreased to " + power.Amount + " (" + amount + ")";
            }
            if (cardSource != null)
            {
                var cardName = cardSource.Title ?? cardSource.GetType().Name;
                powerChangeStr += " [" + cardName + "]";
            }
            NCombatLogWindow.AddLog(powerChangeStr + ".", NCombatLogWindow.CombatEntryType.Power);
        }
    }
}
