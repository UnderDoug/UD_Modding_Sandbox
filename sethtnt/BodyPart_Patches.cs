using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using Genkit;

using XRL;
using XRL.World;
using XRL.World.Anatomy;
using XRL.UI;

namespace UD_Modding_Sandbox.sethtnt.Harmony
{
    [HarmonyPatch]
    public static class BodyPart_Patches
    {
        [HarmonyPatch(
            declaringType: typeof(BodyPart),
            methodName: nameof(BodyPart.SetAsPreferredDefault),
            argumentTypes: new Type[] { typeof(bool) },
            argumentVariations: new ArgumentType[] { ArgumentType.Normal })]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SetAsPreferredDefault_IgnoreHostile_Transpile(IEnumerable<CodeInstruction> Instructions, ILGenerator Generator)
        {
            bool doPatch = false;
            if (!doPatch)
                return Instructions;

            string patchName = "UD_Modding_Sandbox.sethtnt.Harmony" + nameof(BodyPart_Patches) + "." + nameof(BodyPart.SetAsPreferredDefault);
            int metricsCheckSteps = 0;
            CodeMatcher codeMatcher = new(Instructions, Generator);

            // if (The.Player.AreHostilesNearby())
            // {
            //     Popup.Show("You can't switch primary limbs in combat.");
            //     return;
            // }
            CodeMatch[] match_Player_HostilesNearby_Popup_Return = new CodeMatch[]
            {
                // if (The.Player.AreHostilesNearby())
                new(OpCodes.Call, AccessTools.PropertyGetter(typeof(The), nameof(The.Player))),
                new(OpCodes.Callvirt, AccessTools.Method(typeof(GameObject), nameof(GameObject.AreHostilesNearby))),
                new(OpCodes.Brfalse_S),

                //     Popup.Show("You can't switch primary limbs in combat.");
                new(OpCodes.Ldstr, "You can't switch primary limbs in combat."),
                new(OpCodes.Ldnull),
                new(OpCodes.Ldstr, "Sounds/UI/ui_notification"),
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Ldnull),
                new(OpCodes.Call, AccessTools.Method(typeof(Popup), nameof(Popup.Show),
                    new Type[]
                    {
                        typeof(string), typeof(string), typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(Location2D)
                    })),

                //     return;
                new(OpCodes.Ret)
            };

            // find start of:
            //     if (The.Player.AreHostilesNearby())
            //     {
            //         Popup.Show("You can't switch primary limbs in combat.");
            //         return;
            //     }
            // from the start
            if (codeMatcher.Start().MatchStartForward(match_Player_HostilesNearby_Popup_Return).IsInvalid)
            {
                MetricsManager.LogError(patchName + ": (" + metricsCheckSteps + ") " + 
                    nameof(CodeMatcher.MatchStartForward) + " failed to find instructions " + 
                    nameof(match_Player_HostilesNearby_Popup_Return));
                
                return Instructions;
            }
            metricsCheckSteps++;

            codeMatcher.RemoveInstructions(match_Player_HostilesNearby_Popup_Return.Length);

            MetricsManager.LogInfo("Successfully transpiled " + patchName);
            return codeMatcher.InstructionEnumeration();
        }
    }
}
