using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UD_Modding_Toolbox;
using XRL;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Tinkering;

using static UD_Modding_Sandbox.Options;

namespace Garresh.Harmony
{
    [HarmonyPatch]
    public static class LightManipulation_Patches
    {
        private static ModInfo ThisMod => ModManager.GetMod();

        [HarmonyPatch(
            declaringType: typeof(LightManipulation),
            methodName: nameof(LightManipulation.GetRadiusRegrowthTurns),
            argumentTypes: new Type[] { typeof(int) },
            argumentVariations: new ArgumentType[] { ArgumentType.Normal })]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GetRadiusRegrowthTurns_EnableWillpower_Transpile(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator)
        {
            bool doVomit = getClassDoDebug(nameof(LightManipulation_Patches));
            string patchMethodName = $"Garresh.Harmony.{nameof(LightManipulation_Patches)}.{nameof(LightManipulation.GetRadiusRegrowthTurns)}({typeof(int).Name})";
            if (!EnableGarreshLightManipTranspile)
            {
                MetricsManager.LogModInfo(ThisMod, $"{patchMethodName}: Option set to disabled.");
                return Instructions;
            }
            int metricsCheckSteps = 0;

            CodeMatcher codeMatcher = new(Instructions, Generator);

            // if (!GlobalConfig.GetBoolSetting("LightManipulationWillpowerRecharge"))
            // {
            //     return num;
            // }
            CodeMatch[] match_If_GlobalSetting_WillpowerRecharge = new CodeMatch[]
            {
                // if (!GlobalConfig.GetBoolSetting("LightManipulationWillpowerRecharge"))
                new(OpCodes.Ldstr, "LightManipulationWillpowerRecharge"),
                new(OpCodes.Ldc_I4_0),
                new(ins => ins.Calls(AccessTools.Method(typeof(GlobalConfig), nameof(GlobalConfig.GetBoolSetting), new Type[] { typeof(string), typeof(bool) }))),
                new(OpCodes.Brtrue_S),
                // {
                //     return num
                new(OpCodes.Ldloc_0),
                new(OpCodes.Ret),
                // }
            };

            // find start of:
            // if (!GlobalConfig.GetBoolSetting("LightManipulationWillpowerRecharge"))
            // {
            //     return num;
            // }
            // from the start
            if (codeMatcher.Start().MatchStartForward(match_If_GlobalSetting_WillpowerRecharge).IsInvalid)
            {
                MetricsManager.LogModError(ThisMod, $"{patchMethodName}: ({metricsCheckSteps}) {nameof(CodeMatcher.MatchStartForward)} failed to find instructions {nameof(match_If_GlobalSetting_WillpowerRecharge)}");
                foreach (CodeMatch match in match_If_GlobalSetting_WillpowerRecharge)
                {
                    MetricsManager.LogModError(ThisMod, $"    {match.name} {match.opcode}");
                }
                codeMatcher.Vomit(doVomit);
                return Instructions;
            }
            metricsCheckSteps++;

            // remove
            // if (!GlobalConfig.GetBoolSetting("LightManipulationWillpowerRecharge"))
            // {
            //     return num;
            // }
            codeMatcher.RemoveInstructions(match_If_GlobalSetting_WillpowerRecharge.Length);

            MetricsManager.LogModInfo(ThisMod, $"Successfully transpiled {patchMethodName}");
            return codeMatcher.Vomit(doVomit).InstructionEnumeration();
        }
    }
}
