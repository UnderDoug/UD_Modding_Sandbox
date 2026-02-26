using HarmonyLib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using XRL;
using XRL.CharacterBuilds;
using XRL.CharacterBuilds.Qud;
using XRL.CharacterBuilds.Qud.UI;
using XRL.Collections;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Parts;

namespace Kjorteo.SpeciesManager.Patches
{
    [HasGameBasedStaticCache]
    [HarmonyPatch]
    internal static class Kjorteo_SpeciesManager_Patches
    {
        internal static string LastSpeciesFilesPath => Kjorteo_EmbarkBuilder_Patches.LastSpeciesFilesPath;
        internal static string LastCharacterCode => Kjorteo_EmbarkBuilder_Patches.LastCharacterCode;

        private const string SPECIES_MANAGER_TECHNICAL_ID = "Kjorteo_SpeciesManager_Technical";
        private const string SPECIES_MANAGER_DISPLAY_ID = "Kjorteo_SpeciesManager_Display";

        public static PlayerSpeciesData EmbarkSpeciesData = new();
        public static bool LoadededFromLast = false;

        private static string GetPlayerBlueprint()
        {
            var builder = GameManager.Instance.gameObject.GetComponent<EmbarkBuilder>();
            if (builder is null)
                return null;

            var body = builder.GetModule<QudGenotypeModule>()?.data?.Entry?.BodyObject
                .Coalesce(builder.GetModule<QudSubtypeModule>()?.data?.Entry?.BodyObject)
                .Coalesce("Humanoid");

            return builder.info?.fireBootEvent(QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECTBLUEPRINT, The.Game, body);
        }

        private static GameObjectBlueprint GetPlayerModel()
            => GameObjectFactory.Factory.GetBlueprintIfExists(GetPlayerBlueprint());

        [HarmonyPatch(
            declaringType: typeof(QudCustomizeCharacterModule),
            methodName: nameof(QudCustomizeCharacterModule.Init))]
        [HarmonyPostfix]
        private static void Init_InstantiateData_Postfix()
        {
            if (!LoadededFromLast)
                EmbarkSpeciesData = new();
        }

        [HarmonyPatch(
            declaringType: typeof(QudCustomizeCharacterModuleWindow),
            methodName: nameof(QudCustomizeCharacterModuleWindow.GetSelections))]
        [HarmonyPostfix]
        private static IEnumerable<PrefixMenuOption> GetSelections_AddSpeciesOption_Postfix(IEnumerable<PrefixMenuOption> __result)
        {
            foreach (var option in __result)
            {
                yield return option;
                if (option.Id == "Name")
                {
                    EmbarkSpeciesData.TechnicalSpecies ??= GetPlayerModel()?.GetTag("Species") ?? "default";

                    yield return new PrefixMenuOption
                    {
                        Id = SPECIES_MANAGER_TECHNICAL_ID,
                        Prefix = "Species: ",
                        Description = EmbarkSpeciesData.TechnicalSpecies,
                    };

                    string displaySpecies = EmbarkSpeciesData.DisplaySpecies ?? EmbarkSpeciesData.TechnicalSpecies;

                    yield return new PrefixMenuOption
                    {
                        Id = SPECIES_MANAGER_DISPLAY_ID,
                        Prefix = "Display Species: ",
                        Description = displaySpecies,
                    };
                }
            }
        }

		/// <summary>
		/// Adds a listener to option selection.
		/// </summary>
		[HarmonyPostfix]
        [HarmonyPatch(
            declaringType: typeof(QudCustomizeCharacterModuleWindow),
            methodName: nameof(QudCustomizeCharacterModuleWindow.UpdateUI))]
        private static void UpdateUIPostfix(QudCustomizeCharacterModuleWindow __instance)
        {
            _jankstance = __instance;

            __instance.prefabComponent.onSelected.AddListener(HandleSelection);
        }

        private static QudCustomizeCharacterModuleWindow _jankstance;

        /// <summary>
        /// Handles option selection.
        /// </summary>
        private static async void HandleSelection(FrameworkDataElement DataElement)
        {
            if (DataElement.Id == SPECIES_MANAGER_TECHNICAL_ID
                || DataElement.Id == SPECIES_MANAGER_DISPLAY_ID)
            {
                using var distinctSpecies = ScopeDisposedList<string>.GetFromPool();
                distinctSpecies.AddRange(
                    Items: GameObjectFactory.Factory
                        ?.BlueprintList                         // Goes to the list of all objets in GameObjectBlueprints
                        ?.Where(bp => bp.HasTag("Species"))     // Only selects the ones where the "Species" field isn't blank
                        ?.Select(bp => bp.GetTag("Species"))    // Converts the entire object data structure into a string for just the "Species" value
                        ?.Distinct());                          // Filters out duplicates
                distinctSpecies.Sort();                         // Alphabetize the list real quick
                distinctSpecies.Insert(0, Kjorteo_SpeciesManager.CUSTOM_ENTRY);       // Add custom value
                bool isTechnical = DataElement.Id == SPECIES_MANAGER_TECHNICAL_ID;
                string currentValue = isTechnical ? EmbarkSpeciesData.TechnicalSpecies : (EmbarkSpeciesData.DisplaySpecies ?? EmbarkSpeciesData.TechnicalSpecies ?? "");
                string newValue = null;

                string remarks = "{{y|" + (isTechnical ? "(An empty string will fall back to blueprint species)" : "(An empty string will delete this entry)") + "}}";

                string intro = "{{W|" + $"Here are the existing technical species held by all objects in this world excluding the player. " +
                    $"Select one to change your {(isTechnical ? "technical" : "display")} species to match.\n\n" +
                    $"Pick \"{Kjorteo_SpeciesManager.CUSTOM_ENTRY}\" to enter your own!\n\n" + "}}";

                if (await Popup.PickOptionAsync(Title: "Species Manager",
                    Intro: intro,
                    Options: distinctSpecies.ToArray(),
                    DefaultSelected: -1,                        // Falls back to -1 if escaped,
                    AllowEscape: true) is int selectionTSpecies
                    && selectionTSpecies >= 0)                  // only does something if an option is selected.
                {
                    string selectedValue = distinctSpecies[selectionTSpecies];
                    if (selectionTSpecies != 0
                        || selectedValue != Kjorteo_SpeciesManager.CUSTOM_ENTRY)
                        newValue = selectedValue;
                    else
                    if (await Popup.AskStringAsync(
                        Message: "{{W|" + $"What is your new {(isTechnical ? "technical" : "display")} species?\n" + remarks + "}}",
                        Default: currentValue ?? "",
                        ReturnNullForEscape: true) is string newCustomSpecies)
                        newValue = newCustomSpecies;

                    if (newValue != null) // escape returns null, do nothing if escaped.
                    {
                        if (newValue == "")     // if empty string (but not escaped)
                            newValue = null;    // set to null (to "delete" the value")

                        if (isTechnical)
                            EmbarkSpeciesData.TechnicalSpecies = newValue;
                        else
                            EmbarkSpeciesData.DisplaySpecies = newValue;
                    }

                    _jankstance.UpdateUI();
                }
            }
        }

        [HarmonyPatch(
            declaringType: typeof(QudChartypeModule),
            methodName: nameof(QudChartypeModule.selectType),
            argumentTypes: new Type[] { typeof(string) },
            argumentVariations: new ArgumentType[] { ArgumentType.Normal })]
        [HarmonyPrefix]
        private static bool selectType_LoadFromLast_Prefix(string type)
        {
            if (type == "Last"
                && File.Exists(LastSpeciesFilesPath)
                && File.ReadAllText(LastSpeciesFilesPath) is string lastSpeciesRaw
                && lastSpeciesRaw.Split("\n") is string[] fileParts
                && fileParts.Length > 1
                && fileParts[0] == LastCharacterCode
                && fileParts[1] is string speciesCode)
            {
                LoadededFromLast = true;
                EmbarkSpeciesData = PlayerSpeciesData.LoadCode(speciesCode);
            }
            else
                LoadededFromLast = false;

            return true;
        }
    }
}
