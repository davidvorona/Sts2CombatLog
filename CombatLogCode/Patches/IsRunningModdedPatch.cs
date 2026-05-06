using HarmonyLib;
using MegaCrit.Sts2.Core.Saves;

namespace Sts2CombatLog.CombatLogCode.Patches
{
    [HarmonyPatch(typeof(UserDataPathProvider), "get_IsRunningModded")]
    class GetIsRunningModdedPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref bool __result)
        {
            __result = false;
        }
    }

    [HarmonyPatch(typeof(UserDataPathProvider), "set_IsRunningModded")]
    class SetIsRunningModdedPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool value)
        {
            value = false;
            return true; // Run original setter with value = false
        }
    }
}
