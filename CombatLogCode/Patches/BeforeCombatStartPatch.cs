using BaseLib.Utils.Patching;
using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using System.Reflection;
using System.Reflection.Emit;
using Sts2CombatLog.CombatLogCode.ModScenes;

namespace Sts2CombatLog.CombatLogCode.Patches
{
    [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart), MethodType.Async)]
    class BeforeCombatStartPatch
    {
        [HarmonyTranspiler]
        static List<CodeInstruction> BeforeCombatStart(ILGenerator generator, IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            return AsyncMethodCall.Create(generator, instructions, original,
                AccessTools.Method(typeof(BeforeCombatStartPatch), nameof(BeforeBeforeCombatStartHook)), beforeState: original);
        }

        private static async Task BeforeBeforeCombatStartHook()
        {
            NCombatLogWindow.AddLog("------ COMBAT " + (++MainFile.CombatCounter) + " ------", NCombatLogWindow.CombatEntryType.Start);
        }
    }
}
