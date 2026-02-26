using HarmonyLib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using XRL;
using XRL.CharacterBuilds;

namespace Kjorteo.SpeciesManager.Patches
{
    [HarmonyDebug]
    [HarmonyPatch]
    internal static class Kjorteo_EmbarkBuilder_Patches
    {
        internal static string LastSpeciesFilesPath => DataManager.SavePath("Kjorteo_lastspecies.txt");
        internal static string LastCharacterCode => File.ReadAllText(DataManager.SavePath("lastcharacter.txt"));

        public static bool TypeFilter(Type t)
            => t.Name.Contains(nameof(EmbarkBuilder.SaveCharacterCode))
            && AccessTools.GetDeclaredMethods(t)
                .Any(m => m.Name == "MoveNext")
            ;
        static MethodBase TargetMethod()
            => AccessTools.Method(
                type: AccessTools.FirstInner(type: typeof(EmbarkBuilder), t => TypeFilter(t)),
                name: "MoveNext")
            ;

        private static void SaveSpeciesCode()
        {
            if (new StreamWriter(LastSpeciesFilesPath) is StreamWriter writer)
            {
                writer.Write(LastCharacterCode + "\n");
                writer.Write(Kjorteo_SpeciesManager_Patches.EmbarkSpeciesData.GenerateCode());
                writer.Flush();
                writer.Dispose();
            }
        }

        static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> Instructions,
            ILGenerator Generator
            )
            // Modifying the below:
            // public async Task SaveCharacterCode()
            // {
            //     string path = DataManager.SavePath("lastcharacter.txt");
            //     if (!(await Blob.WriteAllTextAsync(DataManager.SavePath("lastcharacter.txt"), generateCode())).WasSuccessful())
            //     {
            //         MetricsManager.LogWarning("Could not save \"" + path + "\"");
            //     }
            // }
            //
            // to functionally be:
            // public async Task SaveCharacterCode()
            // {
            //     string path = DataManager.SavePath("lastcharacter.txt");
            //     if ((await Blob.WriteAllTextAsync(DataManager.SavePath("lastcharacter.txt"), generateCode())).WasSuccessful())
            //     {
            //         SaveSpeciesCode();
            //     }
            //     else
            //     {
            //         MetricsManager.LogWarning("Could not save \"" + path + "\"");
            //     }
            // }
            // 
            // if (!awaiter.GetResult().WasSuccessful())
            // IL_0081: ldloca.s 2
            // IL_0083: call instance !0 valuetype[mscorlib]System.Runtime.CompilerServices.TaskAwaiter`1<valuetype[LaundryBear.Platform]LaundryBear.PlatformServices.StorageResult>::GetResult()
            // IL_0088: call bool Platform.IO.ResultUtils::WasSuccessful(valuetype[LaundryBear.Platform]LaundryBear.PlatformServices.StorageResult)
            // IL_008d: brtrue.s IL_00a9
            //
            // MetricsManager.LogWarning("Could not save \"" + <path>5__2 + "\"");
            // IL_008f: ldstr "Could not save \""
            // IL_0094: ldarg.0
            // IL_0095: ldfld string XRL.CharacterBuilds.EmbarkBuilder /'<SaveCharacterCode>d__16'::'<path>5__2'
            // IL_009a: ldstr "\""
            // IL_009f: call string[mscorlib] System.String::Concat(string, string, string)
            // IL_00a4: call void MetricsManager::LogWarning(string)
            => new CodeMatcher(Instructions, Generator)
                
                .MatchStartForward(new CodeMatch(
                    opcode: OpCodes.Call,
                    operand: AccessTools.Method(
                        type: typeof(MetricsManager),
                        name: nameof(MetricsManager.LogWarning),
                        parameters: new Type[] { typeof(string) })
                    ))
                .ThrowIfInvalid($"Could not find call to {nameof(MetricsManager)}.{nameof(MetricsManager.LogWarning)}")

                .MatchEndBackwards(new CodeMatch(OpCodes.Brtrue_S))
                .ThrowIfInvalid($"Could not find {OpCodes.Brtrue_S}")

                .Advance(1)
                .CreateLabel(out Label falseResultLabel)
                .Advance(-1)

                .RemoveInstruction()
                .Insert(
                    new CodeInstruction(OpCodes.Brfalse_S, falseResultLabel),
                    CodeInstruction.Call(
                        type: typeof(Kjorteo_EmbarkBuilder_Patches),
                        name: nameof(SaveSpeciesCode))
                    )
                .Instructions()
                ;
    }
}
