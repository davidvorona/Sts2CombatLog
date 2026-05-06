using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using System.Reflection;

namespace Sts2CombatLog.CombatLogCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public static bool IsMainThread;
    public const string ModId = "Sts2CombatLog";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        IsMainThread = true;

        Harmony harmony = new(ModId);

        var assembly = Assembly.GetExecutingAssembly();
        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(assembly);

        harmony.PatchAll(assembly);
    }
}
