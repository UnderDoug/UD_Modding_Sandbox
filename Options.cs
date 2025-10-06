using Garresh.Harmony;
using System.Collections.Generic;
using XRL;

namespace UD_Modding_Sandbox
{
    [HasModSensitiveStaticCache]
    [HasOptionFlagUpdate(Prefix = "Option_UD_ModdingSandbox_")]
    public static class Options
    {
        public static bool doDebug = true;
        public static Dictionary<string, bool> classDoDebug = new()
        {
            // General
            { nameof(Extensions), true },

            { "LiquidVolume_Patches", true },
            { nameof(LightManipulation_Patches), false },
        };

        public static bool getClassDoDebug(string Class) => classDoDebug.ContainsKey(Class) ? classDoDebug[Class] : doDebug;

        // Debug Settings
        // [OptionFlag] public static int DebugVerbosity;
        // [OptionFlag] public static bool DebugIncludeInMessage;
        // [OptionFlag] public static bool DebugGeneralDebugDescriptions;
        [OptionFlag] public static bool ForceFakeSecretChanceTo100;

        // Checkbox settings
        [OptionFlag] public static bool EnableGarreshLightManipTranspile;

    } //!-- public static class Options
}
