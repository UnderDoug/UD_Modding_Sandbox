using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Qud.API;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.Wish;

using UD_Modding_Toolbox;
using Debug = UD_Modding_Toolbox.Logging.Debug;
using UD_Modding_Toolbox.Collections;

namespace XRL.World.Parts
{
    [HasWishCommand]
    [Serializable]
    public class FakeSecrets : IPoweredPart
    {
        private static bool doDebug = true;

        private static bool ForceChance100 => UD_Modding_Sandbox.Options.ForceFakeSecretChanceTo100;

        public static Dictionary<Type, int> JournalEntryTypeWeights => new()
        {
            { typeof(JournalGeneralNote), 15 },
            { typeof(JournalMapNote), 5 },
            { typeof(JournalObservation), 25 },
            { typeof(JournalRecipeNote), 15 },
            { typeof(JournalSultanNote), 10 },
            { typeof(JournalVillageNote), 5 },
            { typeof(JournalAccomplishment), 25 },
        };

        public int Chance;
        public int MinTurns;
        public int MaxTurns;
        public int RolledTurns;
        public int TurnsCounter;
        public bool CanLearn;

        public FakeSecrets()
        {
            Chance = 5;
            MinTurns = 60;
            MaxTurns = 120;

            RolledTurns = 0;

            MustBeUnderstood = false;
            WorksOnEquipper = true;
            NameForStatus = "RealVision";
            ChargeUse = 5;
            IsEMPSensitive = true;
            IsPowerLoadSensitive = true;
            IsPowerSwitchSensitive = false;
            IsBootSensitive = false;
        }

        public bool CanBestowFakeSecret()
        {
            return CanLearn
                && IsOccludingPsionics(true)
                && GetActivePartFirstSubject() is GameObject subject
                && subject.IsPlayer()
                && !subject.OnWorldMap()
                && !JournalAPI.GetKnownNotes().IsNullOrEmpty()
                && JournalAPI.GetKnownNotes().Count() > 9;
        }

        public bool IsOccludingPsionics(bool UseCharge = false)
        {
            return !ParentObject.IsEMPed()
                && ParentObject.IsEquippedProperly()
                && IsReady(UseCharge: UseCharge);
        }

        public bool TryBestowSecret()
        {
            int indent = Debug.LastIndent;
            bool doDebug = true;
            if (GetActivePartFirstSubject() is GameObject subject
                && CanBestowFakeSecret())
            {
                CanLearn = false;
                int chanceMultiplier = 1 + MyPowerLoadBonus();
                int chance = Chance * chanceMultiplier;
                Debug.Entry(4, nameof(FakeSecrets) + " " + nameof(chance), chance.ToString(), Indent: indent + 1, Toggle: doDebug);
                if (ForceChance100)
                {
                    chance = 100;
                }
                bool powerLoaded = chanceMultiplier > 1;
                if (chance.in100())
                {
                    Raffle<IBaseJournalEntry> journalEntryRaffle = new(nameof(FakeSecrets)+ ParentObject?.ID + The.CurrentTurn);
                    foreach (IBaseJournalEntry journalEntry in JournalAPI.GetKnownNotes())
                    {
                        Type journalEntryType = journalEntry.GetType();
                        int journalEntryWeight = 10;
                        if (JournalEntryTypeWeights.ContainsKey(journalEntryType))
                        {
                            journalEntryWeight = JournalEntryTypeWeights[journalEntryType];
                        }
                        journalEntryRaffle.Add(journalEntry, journalEntryWeight);
                    }

                    IBaseJournalEntry fakeSecretEntry = journalEntryRaffle.Draw();

                    journalEntryRaffle.Vomit(4, 
                        nameof(FakeSecrets), nameof(TryBestowSecret), 
                        ShowChance: true, Drawn: fakeSecretEntry, 
                        Indent: indent + 1, Toggle: doDebug);

                    var fakeSecretMapNote = fakeSecretEntry as JournalMapNote;
                    string secretText = fakeSecretMapNote != null
                        ? "The location of " + Grammar.InitLowerIfArticle(fakeSecretMapNote?.Text)
                        : fakeSecretEntry?.Text ?? LoreGenerator.RuinOfHouseIsnerLore(ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y);

                    Debug.Entry(4,
                        nameof(FakeSecrets) + "." + nameof(TryBestowSecret), secretText,
                        Indent: indent + 1, Toggle: doDebug);

                    double cutoffPercent = 0.25 + Stat.RandomCosmetic(-7, 7) * 0.01;
                    int cutoff = (int)(secretText.Length * cutoffPercent);
                    int trunc = Math.Min(secretText.Length - 2, secretText.Length - 1 - cutoff);

                    secretText = secretText[..trunc].Color("y") + "...";

                    AutoAct.Interrupt("you need to think for a sec..", subject.CurrentCell);

                    if (!powerLoaded)
                    {
                        Popup.Show(Message:
                            "Protected from the negative noospheric influences of else-whence minds, " +
                            "a new truth surfaces from your settling mental silt:\n\n" + secretText);
                    }
                    else
                    {
                        var truthColors = "RGBCWYM".ToRaffle();
                        string reality = "{{" + truthColors.DrawCosmetic() + "|REALITY}}";
                        string pertinent = "{{" + truthColors.DrawCosmetic() + "|PERTITNENT}}";
                        string weaponised = "{{" + truthColors.DrawCosmetic() + "|WEAPONISED}}";
                        string idea = "{{" + truthColors.DrawCosmetic() + "|IDEA}}";

                        Popup.Show(Message:
                            "A geyser of unadulterated " + reality + " errupts in your mind, spraying you with " + pertinent + " facts; " +
                            "unimpeded by " + weaponised + " distraction, a single crystalline " + idea + " emerges:\n\n" + secretText);
                    }
                    Popup.ShowFail(Message:
                        "...\n\nYou go to make note of this obviously important piece of knowledge, " +
                        "but as quickly as it struck you... It slips away.");

                    Debug.LastIndent = indent;
                    return true;
                }
            }
            Debug.LastIndent = indent;
            return false;
        }

        public override bool AllowStaticRegistration()
        {
            return true;
        }
        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade)
                || ID == EndTurnEvent.ID
                || ID == IsSensableAsPsychicEvent.ID
                || ID == GetDebugInternalsEvent.ID;
        }
        public override bool HandleEvent(EndTurnEvent E)
        {
            TryBestowSecret();
            if (!CanLearn
                && ParentObject.IsEquippedProperly()
                && GetActivePartFirstSubject() is GameObject subject
                && TurnsCounter++ > RolledTurns)
            {
                TurnsCounter = 0;
                RolledTurns = subject.GetSeededRange(nameof(FakeSecrets), MinTurns, MaxTurns);
                CanLearn = true;
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(IsSensableAsPsychicEvent E)
        {
            if (IsOccludingPsionics() && GetActivePartFirstSubject() == E.Object)
            {
                E.Sensable = false;
                return true;
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(GetDebugInternalsEvent E)
        {
            E.AddEntry(this, nameof(Chance), Chance);
            E.AddEntry(this, nameof(MinTurns), MinTurns);
            E.AddEntry(this, nameof(MaxTurns), MaxTurns);
            E.AddEntry(this, nameof(RolledTurns), RolledTurns);
            E.AddEntry(this, nameof(TurnsCounter), TurnsCounter);
            E.AddEntry(this, nameof(CanLearn), CanLearn);
            E.AddEntry(this, nameof(CanBestowFakeSecret), CanBestowFakeSecret());
            E.AddEntry(this, nameof(IsOccludingPsionics), IsOccludingPsionics());
            E.AddEntry(this, nameof(ParentObject.IsEquippedProperly), ParentObject.IsEquippedProperly());
            E.AddEntry(this, nameof(JournalAPI.GetKnownNotes) + "." + nameof(string.IsNullOrEmpty), JournalAPI.GetKnownNotes().IsNullOrEmpty());
            E.AddEntry(this, nameof(JournalAPI.GetKnownNotes) + "." + nameof(List<IBaseJournalEntry>.Count), JournalAPI.GetKnownNotes().Count());
            return base.HandleEvent(E);
        }

        [WishCommand(Command: "psionic occluder test kit")]
        public static bool GivePsionicOccluder_WishHandler()
        {
            return The.Player.ReceiveObject("Psionic Occluder")
                && The.Player.ReceiveObject("Psionic Occluder", 
                BeforeObjectCreated: GO => 
                { 
                    GO.ApplyModification(nameof(ModOverloaded));
                    GO.ApplyModification(nameof(ModSturdy)); 
                });
        }
    }
}
