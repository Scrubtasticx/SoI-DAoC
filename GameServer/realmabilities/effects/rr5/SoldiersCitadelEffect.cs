/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */

using System;
using System.Collections.Generic;
using DOL.Events;

namespace DOL.GS.Effects
{

    public class SoldiersCitadelEffect : TimedEffect
    {
        private const int Duration = 15 * 1000;

        private GamePlayer _effectOwner;

        public SoldiersCitadelEffect()
            : base(Duration)
        { }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            if (target is GamePlayer player)
            {
                _effectOwner = player;
                foreach (GamePlayer p in _effectOwner.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    p.Out.SendSpellEffectAnimation(_effectOwner, _effectOwner, 7093, 0, false, 1);
                }

                _effectOwner.BaseBuffBonusCategory[(int)eProperty.ParryChance] += 50;
                _effectOwner.BaseBuffBonusCategory[(int)eProperty.BlockChance] += 50;

                GameEventMgr.AddHandler(_effectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.AddHandler(_effectOwner, GameLivingEvent.Dying, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.AddHandler(_effectOwner, GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.AddHandler(_effectOwner, GamePlayerEvent.RegionChanged, new DOLEventHandler(PlayerLeftWorld));
            }
        }

        public override void Stop()
        {
            if (_effectOwner != null)
            {
                _effectOwner.BaseBuffBonusCategory[(int)eProperty.ParryChance] -= 50;
                _effectOwner.BaseBuffBonusCategory[(int)eProperty.BlockChance] -= 50;
                if (_effectOwner.IsAlive)
                {
                    new SoldiersCitadelSecondaryEffect().Start(_effectOwner);
                }

                GameEventMgr.RemoveHandler(_effectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.RemoveHandler(_effectOwner, GameLivingEvent.Dying, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.RemoveHandler(_effectOwner, GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.RemoveHandler(_effectOwner, GamePlayerEvent.RegionChanged, new DOLEventHandler(PlayerLeftWorld));
            }

            base.Stop();
        }

        /// <summary>
        /// Called when a player leaves the game
        /// </summary>
        /// <param name="e">The event which was raised</param>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">EventArgs associated with the event</param>
        protected void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
        {
            if (sender is GamePlayer player)
            {
                SoldiersCitadelEffect soldiersCitadel = player.EffectList.GetOfType<SoldiersCitadelEffect>();
                soldiersCitadel?.Cancel(false);
            }
        }

        public override string Name => "Soldier's Citadel";

        public override ushort Icon => 3091;

        // Delve Info
        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Grants +50% block/parry for 30s."
                };

                return list;
            }
        }
    }

    public class SoldiersCitadelSecondaryEffect : TimedEffect
    {
        private const int Duration = 30 * 1000;

        private GamePlayer _effectOwner;

        public SoldiersCitadelSecondaryEffect()
            : base(Duration)
        { }

        public override void Start(GameLiving target)
        {
            base.Start(target);
            if (target is GamePlayer player)
            {
                _effectOwner = player;
                _effectOwner.BaseBuffBonusCategory[(int)eProperty.ParryChance] -= 10;
                _effectOwner.BaseBuffBonusCategory[(int)eProperty.BlockChance] -= 10;

                GameEventMgr.AddHandler(_effectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.AddHandler(_effectOwner, GameLivingEvent.Dying, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.AddHandler(_effectOwner, GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.AddHandler(_effectOwner, GamePlayerEvent.RegionChanged, new DOLEventHandler(PlayerLeftWorld));
            }
        }

        public override void Stop()
        {
            if (_effectOwner != null)
            {
                _effectOwner.BaseBuffBonusCategory[(int)eProperty.ParryChance] += 10;
                _effectOwner.BaseBuffBonusCategory[(int)eProperty.BlockChance] += 10;

                GameEventMgr.RemoveHandler(_effectOwner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.RemoveHandler(_effectOwner, GameLivingEvent.Dying, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.RemoveHandler(_effectOwner, GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerLeftWorld));
                GameEventMgr.RemoveHandler(_effectOwner, GamePlayerEvent.RegionChanged, new DOLEventHandler(PlayerLeftWorld));
            }

            base.Stop();
        }

        /// <summary>
        /// Called when a player leaves the game
        /// </summary>
        /// <param name="e">The event which was raised</param>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">EventArgs associated with the event</param>
        private static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
        {
            if (sender is GamePlayer player)
            {
                SoldiersCitadelSecondaryEffect soldiersCitadel = player.EffectList.GetOfType<SoldiersCitadelSecondaryEffect>();
                soldiersCitadel?.Cancel(false);
            }
        }

        public override string Name => "Soldier's Citadel";

        public override ushort Icon => 3091;

        // Delve Info
        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    "Penality -10% block/parry for 15s"
                };

                return list;
            }
        }
    }
}
