using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.NET.Common;
using BepInExResoniteShim;
using FrooxEngine;
using HarmonyLib;

namespace NoHeadMenuDash;

[ResonitePlugin(PluginMetadata.GUID, PluginMetadata.NAME, PluginMetadata.VERSION, PluginMetadata.AUTHORS, PluginMetadata.REPOSITORY_URL)]
[BepInDependency(BepInExResoniteShim.PluginMetadata.GUID, BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BasePlugin
{
#nullable disable
    internal static new ManualLogSource Log;
#nullable enable

    public override void Load()
    {
        Log = base.Log;
        HarmonyInstance.PatchAll();
    }
    
    [HarmonyPatch(typeof(InteractionHandler))]
    [HarmonyPatch("EndMenu")]
    class InteractionHandler_EndMenu_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var start = 0;
            var end = 0;

            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].operand is MethodInfo)
                {
                    MethodInfo mi = codes[i].operand as MethodInfo;
                    if (mi.Name == "get_IsNearHead")
                        start = i;
                    else if (mi.Name == "TryVibrateMedium")
                        end = i + 1;
                }
            }
            codes.RemoveRange(start - 5, end - start + 6);
            /*
             * This prints the il code, useful for debugging
            for(var i = 0; i < codes.Count; i++)
            {
                Debug(codes[i].ToString());
            }
            */
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(InteractionHandler))]
    [HarmonyPatch("StartMenu")]
    class InteractionHandler_StartMenu_Patch
    {
        public static void Postfix(InteractionHandler __instance)
        {
            if (__instance.IsNearHead && !__instance.SharesUserspaceToggleAndMenus)
            {
                AccessTools.Method(typeof(InteractionHandler), "TryOpenContextMenu").Invoke(__instance, null);
            }
        }
    }
}
