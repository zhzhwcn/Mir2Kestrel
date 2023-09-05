using ServerPackets;
using System.Drawing;

namespace ServerKestrel.Mir2Amz.Objects
{
    internal abstract class HumanObject : MapObject
    {
        protected HumanObject(uint objectId) : base(objectId)
        {
        }

        public virtual bool CanMove
        {
            get
            {
                return !Dead && Envir.Time >= ActionTime && !CurrentPoison.HasFlag(PoisonType.Paralysis) && !CurrentPoison.HasFlag(PoisonType.LRParalysis) && !CurrentPoison.HasFlag(PoisonType.Frozen);
            }
        }
        public virtual bool CanWalk
        {
            get
            {
                return !Dead && Envir.Time >= ActionTime && !InTrapRock && !CurrentPoison.HasFlag(PoisonType.Paralysis) && !CurrentPoison.HasFlag(PoisonType.LRParalysis) && !CurrentPoison.HasFlag(PoisonType.Frozen);
            }
        }
        public virtual bool CanRun
        {
            get
            {
                return !Dead && Envir.Time >= ActionTime && (_stepCounter > 0 || FastRun) && (!Sneaking || ActiveSwiftFeet) && CurrentBagWeight <= Stats[Stat.BagWeight] && !CurrentPoison.HasFlag(PoisonType.Paralysis) && !CurrentPoison.HasFlag(PoisonType.LRParalysis) && !CurrentPoison.HasFlag(PoisonType.Frozen);
            }
        }
        public virtual bool CanAttack
        {
            get
            {
                return !Dead && Envir.Time >= ActionTime && Envir.Time >= AttackTime && !CurrentPoison.HasFlag(PoisonType.Paralysis) && !CurrentPoison.HasFlag(PoisonType.LRParalysis) && !CurrentPoison.HasFlag(PoisonType.Frozen) && !CurrentPoison.HasFlag(PoisonType.Dazed) && Mount.CanAttack;
            }
        }
        public bool CanRegen
        {
            get
            {
                return Envir.Time >= RegenTime && _runCounter == 0;
            }
        }
        protected virtual bool CanCast
        {
            get
            {
                return !Dead && Envir.Time >= ActionTime && Envir.Time >= SpellTime && !CurrentPoison.HasFlag(PoisonType.Stun) && !CurrentPoison.HasFlag(PoisonType.Dazed) &&
                    !CurrentPoison.HasFlag(PoisonType.Paralysis) && !CurrentPoison.HasFlag(PoisonType.Frozen) && Mount.CanAttack;
            }
        }
    }
}
