using System;
using System.Text;
using XRL;
using XRL.World;
using XRL.Wish;
using XRL.UI;
namespace XRL.World.Parts
{
    [PlayerMutator]
    [HasWishCommand]
    [Serializable]
    public class Kjorteo_ApparentPlayerSpeciesOverride : IPlayerPart, IPlayerMutator
    {
        public const string CMD_CHANGE_SPECIES = "Cmd_Kjorteo_ChangeSpecies";
        public const string CMD_CHANGE_APPARENT_SPECIES = "Cmd_Kjorteo_ChangeApparentSpecies";

        public const string SPECIES = "Species";
        public const string APPARENT_SPECIES = "Kjorteo_ApparentSpecies";

        public string Species
        {
            get => ParentObject?.GetStringProperty(SPECIES);
            set => ParentObject?.SetStringProperty(SPECIES, value);
        }
        public string ApparentSpecies
        {
            get => ParentObject?.GetStringProperty(APPARENT_SPECIES, Species);
            set => ParentObject?.SetStringProperty(APPARENT_SPECIES, value);
        }

        public void mutate(GameObject player)
        {
            player?.RequirePart<Kjorteo_ApparentPlayerSpeciesOverride>();
            ApparentSpecies = Species;
        }

        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            || ID == GetApparentSpeciesEvent.ID
            || ID == OwnerGetInventoryActionsEvent.ID
            || ID == InventoryActionEvent.ID
            ;
        public override bool HandleEvent(GetApparentSpeciesEvent E)
        {
            if (ApparentSpecies != Species
                && E.Priority < ModDisguise.APPARENT_SPECIES_PRIORITY)
            {
                E.ApparentSpecies = ApparentSpecies;
                E.Priority = ModDisguise.APPARENT_SPECIES_PRIORITY - 1;
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(OwnerGetInventoryActionsEvent E)
        {
            if (E.Object.IsPlayer())
            {
                E.AddAction(
                    Name: "Kjorteo_ChangeSpecies",
                    Display: "change species",
                    Command: CMD_CHANGE_SPECIES,
                    PreferToHighlight: "species",
                    Key: 's',
                    FireOnActor: true,
                    Priority: -1,
                    WorksAtDistance: true);
                E.AddAction(
                    Name: "Kjorteo_ChangeApparentSpecies",
                    Display: "change apparent species",
                    Command: CMD_CHANGE_APPARENT_SPECIES,
                    PreferToHighlight: "species",
                    Key: 'S',
                    FireOnActor: true,
                    Priority: -1,
                    WorksAtDistance: true);
            }

            return base.HandleEvent(E);
        }
        public override bool HandleEvent(InventoryActionEvent E)
        {
            if (E.Item.IsPlayer())
            {
                if (E.Command == CMD_CHANGE_SPECIES
                    || E.Command == CMD_CHANGE_APPARENT_SPECIES)
                {
                    bool apparent = E.Command == CMD_CHANGE_APPARENT_SPECIES;
                    string property = apparent ? APPARENT_SPECIES : SPECIES;
                    string speciesWord = E.Command == CMD_CHANGE_SPECIES ? "species" : "apparent species";
                    string oldSpecies = apparent ? ApparentSpecies : Species;
                    if (Popup.AskString("What is the better term for your " + speciesWord + "?",
                        Default: oldSpecies,
                        ReturnNullForEscape: true,
                        EscapeNonMarkupFormatting: true,
                        AllowColorize: true) is string newSpecies)
                    {
                        if (apparent)
                            ApparentSpecies = newSpecies;
                        else
                            Species = newSpecies;

                        Popup.Show("Ever the mutable individual, you've come to realize your " + speciesWord + " is better described as '" + newSpecies + "'; thus it was so.");
                        E.Item.UseEnergy(1000, "Kjorteo_ChangeSpecies");
                    }
                    else
                        Popup.Show("You decide your " + speciesWord + ", '" + oldSpecies + "', is a good fit after all; thus it remains.");
                }
            }
            return base.HandleEvent(E);
        }

        [WishCommand(Command = "species")]
        public static bool SpeciesChange_Wish(string Parameter)
        {
            if (!Parameter.IsNullOrEmpty()
                && The.Player is GameObject player)
            {
                player.SetStringProperty("Species", Parameter);
                return true;
            }
            return false;
        }
        [WishCommand(Command = "apparent species")]
        public static bool ApparentSpecies_WishHandler(string Parameter)
        {
            if (!Parameter.IsNullOrEmpty()
                && The.Player.TryGetPart(out Kjorteo_ApparentPlayerSpeciesOverride apparentSpeciesOverride))
            {
                apparentSpeciesOverride.ApparentSpecies = Parameter;
                return true;
            }
            return false;
        }
    }
}