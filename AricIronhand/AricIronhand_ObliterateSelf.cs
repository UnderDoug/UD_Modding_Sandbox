namespace XRL.World.Parts
{
    public class AricIronhand_ObliterateSelf : IScribedPart
    {
        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            || ID == EnvironmentalUpdateEvent.ID;

        public override bool HandleEvent(EnvironmentalUpdateEvent E)
        {
            if ((ParentObject?.Obliterate()).GetValueOrDefault())
            {
                return false;
            }
            ModManager.TryGetCallingMod(out ModInfo thisMod, out _);
            MetricsManager.LogModError(thisMod, (ParentObject?.DebugName ?? "null object") + " failed to obliterate itself.");
            return base.HandleEvent(E);
        }
    }
}
