using System;
using System.Collections.Generic;
using System.Text;

namespace XRL.World.Effects
{
    public class UD_AmmoStatDrain : IScribedEffect
    {
        public string Stat;

        public string MissileWeaponID;

        public int TurnsPerStack;

        public bool AffectedByWillpower;

        private Stack<int> Penalties;

        public UD_AmmoStatDrain()
            : base()
        {
            DisplayName = "{{K|hampered}}";
            Penalties = new();
        }

        public UD_AmmoStatDrain(string Stat, GameObject MissileWeapon, int TurnsPerStack, bool AffectedByWillpower)
            : this()
        {
            this.Stat = Stat;
            this.MissileWeaponID = MissileWeapon.ID;
            this.TurnsPerStack = TurnsPerStack;
            this.AffectedByWillpower = AffectedByWillpower;
        }
        public override bool Apply(GameObject Object)
        {
            return base.Apply(Object);
        }
    }
}
