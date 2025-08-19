using HarmonyLib;
using UD_Modding_Toolbox;
using XRL.Annals;
using XRL.Liquids;
using XRL.World.Parts;
using static UD_Modding_Toolbox.Const;
using static UD_Modding_Toolbox.Options;

namespace UD_Modding_Sandbox.Harmony
{
    [HarmonyPatch]
    public static class ImportedFoodorDrink_Patches
    {
        private static bool doDebug => getClassDoDebug(nameof(ImportedFoodorDrink_Patches));

        [HarmonyPatch(
            declaringType: typeof(ImportedFoodorDrink),
            methodName: nameof(ImportedFoodorDrink.Generate))]
        [HarmonyPrefix]
        public static bool Generate_DebugReportCall_Prefix()
        {
            int indent = Debug.LastIndent;
            Debug.Entry(4,
                $"[HarmonyPrefix]  " +
                $"{nameof(ImportedFoodorDrink_Patches)}." +
                $"{nameof(ImportedFoodorDrink.Generate)}()",
                Indent: indent + 1, Toggle: doDebug);
            Debug.LastIndent = indent;
            return true;
        }

        [HarmonyPatch(
            declaringType: typeof(ImportedFoodorDrink),
            methodName: nameof(ImportedFoodorDrink.Generate))]
        [HarmonyPostfix]
        public static void Generate_DebugReportCall_Postfix()
        {
            int indent = Debug.LastIndent;
            Debug.Entry(4,
                $"[HarmonyPostfix] " +
                $"{nameof(ImportedFoodorDrink_Patches)}." +
                $"{nameof(ImportedFoodorDrink.Generate)}()",
                Indent: indent + 1, Toggle: doDebug);
            Debug.LastIndent = indent;
        }
    }
}
