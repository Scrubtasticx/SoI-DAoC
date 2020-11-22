using DOL.GS.PacketHandler;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
    public class StaticTempestAbility : TimedRealmAbility
    {
        public StaticTempestAbility(DBAbility dba, int level) : base(dba, level) { }

        private int _stunDuration;
        private uint _duration;

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED))
            {
                return;
            }

            if (!(living is GamePlayer caster))
            {
                return;
            }

            if (caster.TargetObject == null)
            {
                caster.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (!(caster.TargetObject is GameLiving) || !GameServer.ServerRules.IsAllowedToAttack(caster, (GameLiving)caster.TargetObject, true))
            {
                caster.Out.SendMessage($"You cannot attack {caster.TargetObject.Name}!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (!caster.TargetInView)
            {
                caster.Out.SendMessage($"You cannot see {caster.TargetObject.Name}!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (!caster.IsWithinRadius(caster.TargetObject, 1500))
            {
                caster.Out.SendMessage("You target is too far away to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                switch (Level)
                {
                    case 1: _duration = 15; break;
                    case 2: _duration = 20; break;
                    case 3: _duration = 25; break;
                    case 4: _duration = 30; break;
                    case 5: _duration = 35; break;
                    default: return;
                }
            }
            else
            {
                switch (Level)
                {
                    case 1: _duration = 10; break;
                    case 2: _duration = 15; break;
                    case 3: _duration = 30; break;
                    default: return;
                }
            }

            _stunDuration = 3;
            foreach (GamePlayer iPlayer in caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
            {
                if (iPlayer == caster)
                {
                    iPlayer.MessageToSelf($"You cast {Name}!", eChatType.CT_Spell);
                }
                else
                {
                    iPlayer.MessageFromArea(caster, $"{caster.Name} casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                }
            }

            Statics.StaticTempestBase st = new Statics.StaticTempestBase(_stunDuration);
            Point3D targetSpot = new Point3D(caster.TargetObject.X, caster.TargetObject.Y, caster.TargetObject.Z);
            st.CreateStatic(caster, targetSpot, _duration, 5, 360);
            DisableSkill(living);
        }

        public override int GetReUseDelay(int level)
        {
            return 600;
        }
    }
}