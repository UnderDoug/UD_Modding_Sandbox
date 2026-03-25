using HarmonyLib;

using XRL.Annals;
using XRL.Liquids;
using XRL.World.Parts;


using UD_Modding_Toolbox;

using static UD_Modding_Toolbox.Const;
using static UD_Modding_Toolbox.Options;

namespace UD_Modding_Sandbox.Harmony
{
    [HarmonyPatch]
    public static class LiquidVolume_Patches
    {
        private static bool doDebug => getClassDoDebug(nameof(LiquidVolume_Patches));

        [HarmonyPatch(
            declaringType: typeof(LiquidVolume),
            methodName: nameof(LiquidVolume.GetPrimaryLiquid))]
        [HarmonyPrefix]
        public static bool GetPrimaryLiquid_DebugResults_Prefix(ref LiquidVolume __instance, ref BaseLiquid __result)
        {
            int indent = Debug.LastIndent;

            string callingMethod = new System.Diagnostics.StackTrace()?.GetFrame(2)?.GetMethod()?.Name;

            if (callingMethod.Contains(nameof(ImportedFoodorDrink.Generate)))
            {
                Debug.Entry(4,
                    $"[HarmonyPrefix]  " +
                    $"{nameof(LiquidVolume_Patches)}." +
                    $"{nameof(LiquidVolume.GetPrimaryLiquid)}() " +
                    $"{nameof(callingMethod)}: {callingMethod}",
                    Indent: indent + 1, Toggle: doDebug);

                if (!__instance.ComponentLiquids.IsNullOrEmpty())
                {
                    Debug.Entry(4, $"Listing {nameof(__instance.ComponentLiquids)}", Indent: indent + 2, Toggle: doDebug);
                    foreach ((string liquid, int volume) in __instance.ComponentLiquids)
                    {
                        Debug.LoopItem(4, $"{nameof(liquid)}: {liquid}, {nameof(volume)} {volume}",
                            Indent: indent + 3, Toggle: doDebug);
                    }
                }
            }
            Debug.LastIndent = indent;
            return true;
        }

        [HarmonyPatch(
            declaringType: typeof(LiquidVolume),
            methodName: nameof(LiquidVolume.GetPrimaryLiquid))]
        [HarmonyPostfix]
        public static void GetPrimaryLiquid_DebugResults_Postfix(ref LiquidVolume __instance, ref BaseLiquid __result)
        {
            int indent = Debug.LastIndent;

            string callingMethod = new System.Diagnostics.StackTrace()?.GetFrame(2)?.GetMethod()?.Name;

            if (callingMethod.Contains(nameof(ImportedFoodorDrink.Generate)))
            {
                Debug.Entry(4,
                    $"[HarmonyPostfix] " +
                    $"{nameof(LiquidVolume_Patches)}." +
                    $"{nameof(LiquidVolume.GetPrimaryLiquid)}() " +
                    $"{nameof(callingMethod)}: {callingMethod}",
                    Indent: indent + 1, Toggle: doDebug);

                string previousResult = __result?.GetType()?.Name;
                Debug.LoopItem(4, $"{nameof(BaseLiquid)}: {previousResult ?? NULL}",
                    Indent: indent + 2, Toggle: doDebug);

                __result ??= new LiquidCider();

                if (__result?.GetType()?.Name != previousResult)
                {
                    Debug.LoopItem(4, $"{nameof(BaseLiquid)}: {__result?.GetType()?.Name ?? NULL} (Patched)",
                        Indent: indent + 2, Toggle: doDebug);
                }
            }

            Debug.LastIndent = indent;
        }
    }
}
