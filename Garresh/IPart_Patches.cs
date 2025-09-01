using HarmonyLib;
using System;
using XRL.World;
using XRL.World.Parts.Mutation;

namespace Garresh.Harmony
{
    [HarmonyPatch]
    public static class IPart_Patches
    {
        [HarmonyPatch(
            declaringType: typeof(IPart),
            methodName: nameof(IPart.UsePsychometry),
            argumentTypes: new Type[] { typeof(GameObject), typeof(GameObject) },
            argumentVariations: new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal })]
        [HarmonyPostfix]
        public static void UsePsychometry_DescendedFrom_Postfix(ref bool __result, ref GameObject Actor, ref GameObject Subject)
        {
            if (!__result)
            {
                foreach (Psychometry part in Actor.GetPartsDescendedFrom<Psychometry>())
                {
                    if (part.Activate(Subject))
                    {
                        __result = true;
                        break;
                    }
                }
            }
        }
    }
}
