using HarmonyLib;

using XRL.Liquids;
using XRL.World.Parts;

using UD_Modding_Toolbox;

using static UD_Modding_Toolbox.Options;
using static UD_Modding_Toolbox.Const;

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
            Debug.Entry(4,
                $"[HarmonyPrefix] " +
                $"{nameof(LiquidVolume_Patches)}." +
                $"{nameof(LiquidVolume.GetPrimaryLiquid)}()",
                Indent: indent + 1, Toggle: doDebug);
            if (!__instance.ComponentLiquids.IsNullOrEmpty())
            {
                foreach ((string liquid, int volume) in __instance.ComponentLiquids)
                {
                    Debug.LoopItem(4, $"{nameof(liquid)} {liquid}, {nameof(volume)} {volume}",
                        Indent: indent + 2, Toggle: doDebug);
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
            Debug.Entry(4,
                $"[HarmonyPostfix] " +
                $"{nameof(LiquidVolume_Patches)}." +
                $"{nameof(LiquidVolume.GetPrimaryLiquid)}()",
                Indent: indent + 1, Toggle: doDebug);

            Debug.LoopItem(4, $"{nameof(BaseLiquid)} {__result?.GetType()?.Name ?? NULL}",
                Indent: indent + 2, Toggle: doDebug);

            Debug.LastIndent = indent;
        }
    }
}
