using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL;
using XRL.Core;
using XRL.Wish;
using XRL.World;
using XRL.World.Effects;
using XRL.UI;
using XRL.Collections;
using System.Threading.Tasks;
using XRL.UI.Framework;
using Kjorteo.SpeciesManager.Patches;
using Kjorteo.SpeciesManager;

namespace XRL.World.Parts
{
    [HasCallAfterGameLoaded]
    [PlayerMutator]
    [HasWishCommand]
    [Serializable]
    public class Kjorteo_SpeciesManager : IPlayerPart, IPlayerMutator
    {
        public static PlayerSpeciesData EmbarkSpeciesData => Kjorteo_SpeciesManager_Patches.EmbarkSpeciesData;

        public const string CHANGE_SPECIES_COMMAND = "Cmd_Kjorteo_ChangeSpecies";
        public const string CHANGE_DISPLAY_SPECIES_COMMAND = "Cmd_Kjorteo_ChangeDisplaySpecies";
        public const string CUSTOM_ENTRY = "{{W|custom entry}}";

        public const string DISGUISE_WARNING_1 = " {{R|(Disguised)}}\n\n{{r|NOTE: You are currently disguised. Your Display Species will only show as the species you are currently disguised as. To see your true Display Species, please remove your disguise.}}";

        public const string DISGUISE_WARNING_2 = "\n\n{{r|NOTE: You are currently disguised. Changing your Display Species now will change your {{R|undisguised}} Display Species only. Your Display Species will continue to show the species you are disguised as so long as you remain disguised, but the change you enter here will take effect as soon as you remove your disguise.}}";

        static readonly char[] HotKeysMain = new char[] { 'a', 'b', 'c', 'd', 'e', 'f' };
        static readonly string[] OptionsMain = new string[]
        {
            "Change technical species by list",
            "Change technical species by free entry",
            "Change display species",
            "Explanation",
            "(Optional) Support the authors",
            "Exit menu"
        };

        public string Species
        {
            get => ParentObject?.GetTagOrStringProperty("Species");
            set => ParentObject?.SetStringProperty("Species", value);
        }

        public string DisplaySpecies
        {
            get => ParentObject?.GetTagOrStringProperty("Kjorteo_DisplaySpecies", Species);
            set => ParentObject?.SetStringProperty("Kjorteo_DisplaySpecies", value);
        }

        public void mutate(GameObject player)
        {
            if (player?.RequirePart<Kjorteo_SpeciesManager>() is var speciesManager)
            {
                if (EmbarkSpeciesData is null)
                    speciesManager.DisplaySpecies = speciesManager.Species;
                else
                {
                    speciesManager.Species = EmbarkSpeciesData.TechnicalSpecies;
                    speciesManager.DisplaySpecies = EmbarkSpeciesData.DisplaySpecies ?? EmbarkSpeciesData.TechnicalSpecies;
                }
            }
        }

        // This node is called when a save game is loaded; this is what makes the overall mod compatible with already-in-progress campaigns.
        [CallAfterGameLoaded]
        public static void Kjorteo_Species_LoadGameCallback()
        {
            // Called whenever loading a saved game
            if (The.Player?.RequirePart<Kjorteo_SpeciesManager>() is var speciesManager)
                speciesManager.DisplaySpecies ??= speciesManager.Species; // Only set this the FIRST time a saved game is loaded (ie if it's null), not EVERY time
        }

        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            || ID == GetApparentSpeciesEvent.ID
            || ID == GetInventoryActionsEvent.ID
            || ID == InventoryActionEvent.ID
            || ID == GetDebugInternalsEvent.ID
            ;

        public override bool HandleEvent(GetApparentSpeciesEvent E)
        {
            if (E.Priority == 0
                && !ParentObject.HasEffect<Disguised>())
                E.ApparentSpecies = DisplaySpecies;

            UnityEngine.Debug.Log($"{nameof(E.ApparentSpecies)}: {E.ApparentSpecies} @ {nameof(E.Priority)} {E.Priority}");

            return base.HandleEvent(E);
        }
        public override bool HandleEvent(GetInventoryActionsEvent E)
        {
            E.AddAction(
                Name: "Kjorteo_ChangeSpecies",
                Display: "change species",
                Command: CHANGE_SPECIES_COMMAND,
                Key: 's',
                FireOnActor: true);
            E.AddAction(
                Name: "Kjorteo_ChangeDisplaySpecies",
                Display: "change display species",
                Command: CHANGE_DISPLAY_SPECIES_COMMAND,
                PreferToHighlight: "species",
                Key: 'S',
                FireOnActor: true);
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(InventoryActionEvent E)
        {
            if (E.Item == ParentObject)
            {
                if (E.Command == CHANGE_SPECIES_COMMAND
                    || E.Command == CHANGE_DISPLAY_SPECIES_COMMAND)
                {
                    using var distinctSpecies = ScopeDisposedList<string>.GetFromPool();
                    distinctSpecies.AddRange(
                        Items: GameObjectFactory.Factory
                            ?.BlueprintList                         // Goes to the list of all objets in GameObjectBlueprints
                            ?.Where(bp => bp.HasTag("Species"))     // Only selects the ones where the "Species" field isn't blank
                            ?.Select(bp => bp.GetTag("Species"))    // Converts the entire object data structure into a string for just the "Species" value
                            ?.Distinct());                          // Filters out duplicates
                    distinctSpecies.Sort();                         // Alphabetize the list real quick
                    distinctSpecies.Insert(0, CUSTOM_ENTRY);        // Add custom value
                    bool isTechnical = E.Command == CHANGE_SPECIES_COMMAND;
                    string currentValue = isTechnical ? Species : (DisplaySpecies ?? Species ?? "");
                    string newValue = null;

                    string disguiseWarning1 = null;
                    string disguiseWarning2 = null;
                    if (!isTechnical && ParentObject.HasEffect<Disguised>())
                    {
                        disguiseWarning1 = DISGUISE_WARNING_1;
                        disguiseWarning2 = DISGUISE_WARNING_2;
                    }

                    string remarks = "{{y|" + (isTechnical ? "(An empty string will fall back to blueprint species)" : "(An empty string will delete this entry)") + "}}";

                    string intro = "{{W|" + $"Here are the existing technical species held by all objects in this world excluding the player. " +
                        $"Select one to change your {(isTechnical? "technical" : "display")} species to match.\n\n" +
                        $"Pick \"{CUSTOM_ENTRY}\" to enter your own!\n\n" + "}}"+
                        disguiseWarning1; 

                    if (Popup.PickOption(Title: "Species Manager",
                        Intro: intro,
                        Options: distinctSpecies.ToArray(),
                        DefaultSelected: -1,                        // Falls back to -1 if escaped,
                        AllowEscape: true) is int selectionTSpecies
                        && selectionTSpecies >= 0)                  // only does something if an option is selected.
                    {
                        string selectedValue = distinctSpecies[selectionTSpecies];
                        if (selectionTSpecies != 0
                            || selectedValue != CUSTOM_ENTRY)
                            newValue = selectedValue;
                        else
                        if (Popup.AskString(
                            Message: "{{W|" + $"What is your new {(isTechnical ? "technical" : "display")} species?\n" + remarks + "}}" + disguiseWarning2,
                            Default: currentValue ?? "",
                            ReturnNullForEscape: true) is string newCustomSpecies)
                            newValue = newCustomSpecies;

                        if (isTechnical)
                            Species = newValue;
                        else
                            DisplaySpecies = newValue;

                        if (!newValue.IsNullOrEmpty())
                            Popup.Show($"Your new {(isTechnical ? "technical" : "display")} species is {newValue}!");

                        return true;
                    }

                    return false;
                }
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(GetDebugInternalsEvent E)
        {
            E.AddEntry(this, nameof(Species), Species);
            E.AddEntry(this, nameof(DisplaySpecies), DisplaySpecies);
            E.AddEntry(this, $"{nameof(EmbarkSpeciesData)}.{nameof(EmbarkSpeciesData.TechnicalSpecies)}", EmbarkSpeciesData.TechnicalSpecies);
            E.AddEntry(this, $"{nameof(EmbarkSpeciesData)}.{nameof(EmbarkSpeciesData.DisplaySpecies)}", EmbarkSpeciesData.DisplaySpecies);
            return base.HandleEvent(E);
        }

        // Call main menu when player wishes for "species"
        [WishCommand(Command = "species")]
        public static int Kjorteo_SpeciesManager_Handler()
        {
            if (The.Player?.RequirePart<Kjorteo_SpeciesManager>() is var speciesManager)
            {
                PlayerSpeciesData speciesData = new()
                {
                    TechnicalSpecies = speciesManager.Species,
                    DisplaySpecies = speciesManager.DisplaySpecies,
                };
                int selectionMain = -1; // Variable for the chosen main menu option

                string disguiseWarning1 = null; // Attaches to the end of the listed Display Species line if the player is currently disguised
                string disguiseWarning2 = null; // Attaches to the window asking the player for their new Display Species if they're currently disguised
                if (The.Player.HasEffect<Disguised>())
                {
                    disguiseWarning1 = DISGUISE_WARNING_1;
                    disguiseWarning2 = DISGUISE_WARNING_2;
                }

                while (true) // This loop will keep going unless/until the player manually breaks it.
                {
                    selectionMain = Popup.PickOption(
                        Title: "Species Manager",
                        Intro: "&WYour current technical species is:&y\n"
                            + speciesData.TechnicalSpecies + "\n"
                            + "\n"
                            + "&WYour current display species is:&y\n"
                            + speciesData.DisplaySpecies + disguiseWarning1 + "\n"
                            + "\n",
                        Options: OptionsMain,
                        Hotkeys: HotKeysMain,
                        AllowEscape: true);

                    if (selectionMain == -1
                        || selectionMain == 5)
                        break;

                    switch (selectionMain)
                    {
                        case 0: // Happens when the player has chosen to change technical species by list
                            using (var distinctSpecies = ScopeDisposedList<string>.GetFromPoolFilledWith(
                                items: GameObjectFactory.Factory
                                    ?.BlueprintList                         // Goes to the list of all objets in GameObjectBlueprints
                                    ?.Where(bp => bp.HasTag("Species"))     // Only selects the ones where the "Species" field isn't blank
                                    ?.Select(bp => bp.GetTag("Species"))    // Converts the entire object data structure into a string for just the "Species" value
                                    ?.Distinct()))                          // Filters out duplicates
                            {
                                if (!distinctSpecies.IsNullOrEmpty())
                                {
                                    distinctSpecies.Sort(); // Alphabetize the list real quick
                                    if (Popup.PickOption(
                                        Title: "Species Manager",
                                        Intro: "&WHere are the existing technical species held by all objects in this world excluding the player. Select one to change your technical species to match.&y\n\n",
                                        Options: distinctSpecies.ToArray(),
                                        DefaultSelected: -1,                        // Falls back to -1 if escaped,
                                        AllowEscape: true) is int selectionTSpecies
                                        && selectionTSpecies >= 0)                  // only does something if an option is selected.
                                        speciesData.TechnicalSpecies = distinctSpecies[selectionTSpecies];
                                }
                                else
                                    Popup.Show("There are no species in the game??");
                            }
                            break;

                        case 1: // Happens when the player has chosen to change technical species by free entry
                            if (Popup.AskString(
                                Message: "&WWhat is your new technical species? (An empty string will fall back to blueprint species)",
                                Default: speciesData.TechnicalSpecies) is string newTechnicalSpecies)
                                speciesData.TechnicalSpecies = newTechnicalSpecies;
                            break;

                        case 2: // Happens when the player has chosen to change display species
                            if (Popup.AskString(
                                Message: "&WWhat is your new display species? (An empty string will delete this entry)" + disguiseWarning2,
                                Default: speciesData.DisplaySpecies) is string newDisplaySpecies)
                                speciesData.DisplaySpecies = newDisplaySpecies;
                            break;

                        case 3: // Happens when the player has chosen to read the explanation.
                            Popup.Show("Your display species is what you will see in-game in cases such as NPC dialogue. By itself, it does not serve any mechanical or gameplay-affecting purpose; this value is for display purposes only.\n\nYour technical species is the opposite: It should never be displayed in a manner visible to the player, but it silently affects gameplay mechanics. The main example of this in the standard unmodded Caves of Qud experience is weapons with the 'morphogenetic' multiplier, but other mods may use the player's species to determine other mechanics as well.\n\nSpecies Manager separates your technical and display species so that you may be choose to have NPCs refer to your species with whatever fanciful name you desire while not disturbing gameplay mechanics that rely on your technical species.\n\nFor example, one might wish to play a lizardfolk character of their own invention while matching a preexisting in-game species for mechanical purposes. To manage this, the display species could be 'lizardfolk' while the technical species could be the closest reptilian species on the list--perhaps something like 'croc' or 'salamander'.\n\nIf you prefer not to belong to any preexisting species even for mechanical purposes, the option to chose your own technical species by free entry exists as well. This would, for example, allow you to use morphogenetic weapons with no risk of affecting yourself.");
                            break;

                        case 4: // Happens when the player has chosen to support the authors.  (Thank you. uwu)
                            Popup.Show("Hello! We are Celine Kalante Love and Friends, AKA the Woodling System. (\x22Kjorteo\x22 is a general all-purpose account name to refer to the whole system while being unique enough to make it easier to sign up for things.) We're a disabled neurodiverse furry plural system who gets hyperfixated on things and enjoys making content for whatever we're currently into. Supporting ourselves is tricky since ADHD tends to make it hard to finish projects and thus have anything we could sell. The fact that so many of our works are mods for whatever we're currently playing (like this one!) doesn't help, either; obviously we're not going to start paywalling things like this. Therefore, the best we can offer is a Ko-Fi subscription that offers sneak-peek access to WIP updates and blog entries talking about what we're currently working on.\n\n{{Y|https://ko-fi.com/kjorteo/}}\n\nWe also take hourly-rate commissions for coding, writing, 3D model texturing, pixel art, or whatever else you'd like us to do that's within our skill set. You may email inquiries to {{Y|husky@kjorteo.net}} or reach out to us in the DMs or comments of various online platforms.\n\nConsider all of this a tip jar: Nothing is ever required of you. (In fact, it wasn't until version 1.1 or so that this mod even started including this support info at all.) Any support you choose to provide is greatly appreciated, though, and helps us continue to make things like this for you.\n\nThank you so much!");
                            break;
                    }
                }
                speciesManager.Species = speciesData.TechnicalSpecies;
                speciesManager.DisplaySpecies = speciesData.DisplaySpecies;
                return selectionMain;
            }
            return -1;
        }
    }
}