using System;

using XRL.World.Effects;

namespace XRL.World.Parts
{
    [Serializable]
    public class BNSG1_NoMoreLost : IPlayerPart
    {
        public override bool WantEvent(int ID, int Cascade)
        {
            return base.WantEvent(ID, Cascade)
                || ID == ApplyEffectEvent.ID;
        }

        public override bool HandleEvent(ApplyEffectEvent E)
        {
            if (E.Effect.GetType() == typeof(Lost))
            {
                return false;
            }
            return base.HandleEvent(E);
        }
    }
}
