using HarmonyLib;
using ResoniteModLoader;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using FrooxEngine;
using FrooxEngine.ProtoFlux;

namespace NoHeadMenuDash
{
    public class NoHeadMenuDash : ResoniteMod
    {
        public override string Name => "NoHeadMenuDash";
        public override string Author => "art0007i";
        public override string Version => "2.0.0";
        public override string Link => "https://github.com/art0007i/NoHeadMenuDash/";
        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("me.art0007i.NoHeadMenuDash");
            harmony.PatchAll();

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
}
