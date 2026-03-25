using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using HarmonyLib;

using XRL.Names;
using XRL.UI;
using XRL.Wish;
using XRL.World;

namespace UD_Modding_Sandbox.Avenkyr_Pakacit
{
    [HasWishCommand]
    public static class Wishes
    {
        [WishCommand(Command: "TQAQ debug names")]
        public static void TQAQ_DebugNames_Wish(string Params)
        {
            List<string> @params = new(Params.Split(' '));
            Dictionary<string, string> paramValues = new();
            int count = 100;
            foreach (string param in @params)
            {
                if (param.Contains(":"))
                {
                    param.Split(':', out var key, out var value);
                    paramValues.Add(key, value);
                }
                else
                if (int.TryParse(param, out int result))
                {
                    count = Math.Min(result, 999);
                }
                else
                {
                    Popup.Show(nameof(param) + " \"" + param + "\" couldn't be parsed.");
                }
            }
            GameObject For = null;
            string Genotype = null;
            string Subtype = null;
            string Species = null;
            string Culture = null;
            string Faction = null;
            string Region = null;
            string Gender = null;
            // string Mutations = null;
            string Tag = null;
            string Special = null;
            // string NamingContext = null;
            bool SpecialFaildown = true;
            bool? HasHonorific = null;
            bool? HasEpithet = null;
            foreach ((string arg, string value) in paramValues)
            {
                switch (arg.ToLower().Capitalize())
                {
                    case nameof(For):
                        For = GameObject.CreateSample(value);
                        break;

                    case nameof(Genotype):
                        Genotype = value;
                        break;

                    case nameof(Subtype):
                        Subtype = value;
                        break;

                    case nameof(Species):
                        Species = value;
                        break;

                    case nameof(Culture):
                        Culture = value;
                        break;

                    case nameof(Faction):
                        Faction = value;
                        break;

                    case nameof(Region):
                        Region = value;
                        break;

                    case nameof(Gender):
                        Gender = value;
                        break;

                    /*
                    case nameof(Mutations):
                        Mutations = value;
                        break;
                    */

                    case nameof(Tag):
                        Tag = value;
                        break;

                    case nameof(Special):
                        Special = value;
                        break;

                    /*
                    case nameof(NamingContext):
                        NamingContext = value;
                        break;
                    */

                    case nameof(SpecialFaildown):
                        SpecialFaildown = value.EqualsNoCase(true.ToString());
                        break;

                    case nameof(HasHonorific):
                        HasHonorific = value.EqualsNoCase(true.ToString());
                        break;

                    case nameof(HasEpithet):
                        HasEpithet = value.EqualsNoCase(true.ToString());
                        break;

                    default:
                        Popup.Show(nameof(arg) + ": \"" + arg + "\" is invalid.");
                        break;
                }
            }
            for (int i = 1; i < count + 1; i++)
            { 
                string name = NameMaker.MakeName(
                    For: For,
                    Genotype: Genotype,
                    Subtype: Subtype,
                    Species: Species,
                    Culture: Culture,
                    Faction: Faction,
                    Region: Region,
                    Gender: Gender,
                    // Mutations: Mutations,
                    Tag: Tag,
                    Special: Special,
                    // NamingContext: NamingContext,
                    SpecialFaildown: SpecialFaildown,
                    HasHonorific: HasHonorific,
                    HasEpithet: HasEpithet);

                UnityEngine.Debug.Log((i < 10 ? "00" + i : i < 100 ? "0" + i : i) + ": " + name);
            }
        }
    }
}
