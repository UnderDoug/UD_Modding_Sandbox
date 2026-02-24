using System;
using System.Collections.Generic;
using System.Text;

namespace XRL.World.Parts
{
    public class UD_StatAmmoLoader : IActivePart
    {
        private Statistic Statistic => ParentObject?.Equipped?.GetStat(Stat);

        public string Stat;

        public int FlatCostPerShot;

        public double PercentCostPerShot;

        public bool UseStatisticBaseValue;

        public int StatValueFloor;

        public int TurnsPerStack;

        public bool AffectedByWillpower;

        public string PowerVerb = "power";

        public string ProjectileObject;

        public int Available => CalculateAvailable();

        public UD_StatAmmoLoader()
            : base()
        {
            ChargeUse = 0;

            WorksOnEquipper = true;
            WorksOnSelf = true;

            IsBioScannable = true;
            IsPowerLoadSensitive = true;
        }

        #region Overrides

        public override void Initialize()
        {
            base.Initialize();
            NameForStatus = Stat + "MunitionSystem";
        }

        public override bool SameAs(IPart p)
        {
            if (p is UD_StatAmmoLoader statisticAmmoLoader)
            {
                if (statisticAmmoLoader.Stat != Stat)
                    return false;

                if (statisticAmmoLoader.FlatCostPerShot != FlatCostPerShot)
                    return false;

                if (statisticAmmoLoader.PercentCostPerShot != PercentCostPerShot)
                    return false;

                if (statisticAmmoLoader.UseStatisticBaseValue != UseStatisticBaseValue)
                    return false;

                if (statisticAmmoLoader.StatValueFloor != StatValueFloor)
                    return false;

                if (statisticAmmoLoader.Available != Available)
                    return false;

                if (statisticAmmoLoader.ProjectileObject != ProjectileObject)
                    return false;
            }
            return base.SameAs(p);
        }

        #endregion

        public int CalculateAvailable(GameObject Observer = null)
        {
            Observer ??= ParentObject?.Equipped;
            Statistic stat = Observer?.GetStat(Stat);

            if (stat == null)
                return -1;

            if (FlatCostPerShot <= 0
                && PercentCostPerShot <= 0)
            {
                MetricsManager.LogCallingModError(nameof(UD_StatAmmoLoader) + " instantiated with no cost per shot.");
                return -1;
            }

            int statValue = stat.Value;
            int statBaseValue = stat.BaseValue;

            float percentage = (float)(PercentCostPerShot / 100f);

            int count = 0;
            int modifiedStat = statValue;
            int percentReduction = (int)(statBaseValue * percentage);
            while (modifiedStat > StatValueFloor)
            {
                count++;

                if (!UseStatisticBaseValue)
                    percentReduction = (int)(modifiedStat * percentage);

                modifiedStat -= percentReduction;
                modifiedStat -= FlatCostPerShot;
            }
            return count;
        }

        public string GetStatusMessage(ActivePartStatus Status)
        {
            if (Status == ActivePartStatus.Unpowered)
            {
                return ParentObject.T() + " merely " + ParentObject.GetVerb("click", PrependSpace: false) + ".";
            }
            return ParentObject.Does("are") + " " + GetStatusPhrase(Status) + ".";
        }

        public string GetStatusMessage()
            => GetStatusMessage(GetActivePartStatus());

        #region EventHandling

        public override bool WantEvent(int ID, int cascade)
            => base.WantEvent(ID, cascade)
            || ID == GetDisplayNameEvent.ID
            || ID == GetProjectileBlueprintEvent.ID
            || ID == GetMissileWeaponProjectileEvent.ID
            || ID == GetMissileWeaponStatusEvent.ID
            || ID == CheckReadyToFireEvent.ID
            || ID == GetNotReadyToFireMessageEvent.ID
            || ID == GetAmmoCountAvailableEvent.ID
            || ID == CheckLoadAmmoEvent.ID
            || ID == LoadAmmoEvent.ID
            || ID == NeedsReloadEvent.ID
            || ID == AIWantUseWeaponEvent.ID
            || ID == GetDebugInternalsEvent.ID
            ;

        public override bool HandleEvent(GetDisplayNameEvent E)
        {
            if (!ProjectileObject.IsNullOrEmpty()
                && E.Understood())
                E.AddMissileWeaponDamageTag(GetMissileWeaponPerformanceEvent.GetFor(ParentObject.Equipped, ParentObject));

            return base.HandleEvent(E);
        }
        public override bool HandleEvent(GetProjectileBlueprintEvent E)
        {
            if (!ProjectileObject.IsNullOrEmpty())
                E.Blueprint = ProjectileObject;

            return base.HandleEvent(E);
        }
        public override bool HandleEvent(GetMissileWeaponProjectileEvent E)
        {
            if (!ProjectileObject.IsNullOrEmpty())
            {
                E.Blueprint = ProjectileObject;
                return false;
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(GetMissileWeaponStatusEvent E)
        {
            if (E.Override == null)
            {
                E.Status.text = "[Awaiting Host]";
                if (Statistic != null)
                {
                    string statusText = "[" + Stat + ":" + Statistic.Value + "]";
                    E.Status.text = statusText;
                    E.Items.Append(" " + statusText);
                }
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(CheckReadyToFireEvent E)
            => !IsDisabled()
            && Available > 0
            && base.HandleEvent(E)
            ;
        public override bool HandleEvent(GetNotReadyToFireMessageEvent E)
        {
            if (GetActivePartStatus() is ActivePartStatus activePartStatus
                && activePartStatus != 0)
                E.Message ??= GetStatusMessage(activePartStatus);

            return base.HandleEvent(E);
        }
        public override bool HandleEvent(GetAmmoCountAvailableEvent E)
        {
            E.Register(Available);
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(CheckLoadAmmoEvent E)
        {
            if (Available <= 0)
            {
                E.Message ??= ("=object.T's= " + Stat.ToLower() + " is insufficient to " + PowerVerb + " =subject.t=.")
                    .StartReplace()
                    .AddObject(ParentObject)
                    .AddObject(ParentObject.Equipped)
                    .ToString();
                return false;
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(LoadAmmoEvent E)
        {
            if (Available <= 0)
            {
                E.Message ??= ("=object.T's= " + Stat.ToLower() + " is insufficient to " + PowerVerb + " =subject.t=.")
                    .StartReplace()
                    .AddObject(ParentObject)
                    .AddObject(ParentObject.Equipped)
                    .ToString();
                return false;
            }
            if (!ProjectileObject.IsNullOrEmpty())
                E.Projectile = GameObject.Create(Blueprint: ProjectileObject, Context: "Projectile");

            // Do code here to actually shift the stat down.

            return base.HandleEvent(E);
        }
        public override bool HandleEvent(NeedsReloadEvent E)
            => false
            ;
        public override bool HandleEvent(AIWantUseWeaponEvent E)
            => Available > 0
            && base.HandleEvent(E)
            ;
        public override bool HandleEvent(GetDebugInternalsEvent E)
        {
            E.AddEntry(this, nameof(Stat), Stat);
            E.AddEntry(this, nameof(FlatCostPerShot), FlatCostPerShot);
            E.AddEntry(this, nameof(PercentCostPerShot), PercentCostPerShot);
            E.AddEntry(this, nameof(UseStatisticBaseValue), UseStatisticBaseValue);
            E.AddEntry(this, nameof(StatValueFloor), StatValueFloor);
            E.AddEntry(this, nameof(PowerVerb), PowerVerb);
            E.AddEntry(this, nameof(ProjectileObject), ProjectileObject);
            E.AddEntry(this, nameof(Available), Available);
            return base.HandleEvent(E);
        }

        #endregion
    }
}
