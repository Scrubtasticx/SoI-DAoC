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
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.Language;
using DOL.Events;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Damages the target and lowers their resistance to the spell's type.
    /// </summary>
    [SpellHandler("DirectDamageWithDebuff")]
    public class DirectDamageDebuffSpellHandler : AbstractResistDebuff
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override eProperty Property1 => Caster.GetResistTypeForDamage(Spell.DamageType);

        public override string DebuffTypeName => GlobalConstants.DamageTypeToName(Spell.DamageType);

        private const string LOSEFFECTIVENESS = "LOS Effectivness";

        /// <summary>
        /// execute direct effect
        /// </summary>
        /// <param name="target">target that gets the damage</param>
        /// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target == null)
            {
                return;
            }

            bool spellOK = !(Spell.Target == "Frontal" || Spell.Target == "Enemy" && Spell.Radius > 0 && Spell.Range == 0);

            if (!spellOK || CheckLOS(Caster))
            {
                GamePlayer player = null;
                if (target is GamePlayer)
                {
                    player = target as GamePlayer;
                }
                else
                {
                    if (Caster is GamePlayer gamePlayer)
                    {
                        player = gamePlayer;
                    }
                    else if ((Caster as GameNPC)?.Brain is IControlledBrain)
                    {
                        IControlledBrain brain = ((GameNPC) Caster).Brain as IControlledBrain;

                        // Ryan: edit for BD
                        if (brain.Owner is GamePlayer player1)
                        {
                            player = player1;
                        }
                        else
                        {
                            player = (GamePlayer)((IControlledBrain)((GameNPC)brain.Owner).Brain).Owner;
                        }
                    }
                }

                if (player != null)
                {
                    player.TempProperties.setProperty(LOSEFFECTIVENESS, effectiveness);
                    player.Out.SendCheckLOS(Caster, target, new CheckLOSResponse(DealDamageCheckLOS));
                }
                else
                {
                    DealDamage(target, effectiveness);
                }
            }
            else
            {
                DealDamage(target, effectiveness);
            }
        }

        private bool CheckLOS(GameLiving living)
        {
            foreach (AbstractArea area in living.CurrentAreas)
            {
                if (area.CheckLOS)
                {
                    return true;
                }
            }

            return false;
        }

        private void DealDamageCheckLOS(GamePlayer player, ushort response, ushort targetOID)
        {
            if (player == null) // Hmm
            {
                return;
            }

            if ((response & 0x100) == 0x100)
            {
                try
                {
                    if (Caster.CurrentRegion.GetObject(targetOID) is GameLiving target)
                    {
                        double effectiveness = (double)player.TempProperties.getProperty<object>(LOSEFFECTIVENESS, null);
                        DealDamage(target, effectiveness);

                        // Due to LOS check delay the actual cast happens after FinishSpellCast does a notify, so we notify again
                        GameEventMgr.Notify(GameLivingEvent.CastFinished, Caster, new CastingEventArgs(this, target, m_lastAttackData));
                    }
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                    {
                        log.Error($"targetOID:{targetOID} caster:{Caster} exception:{e}");
                    }
                }
            }
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            // do not apply debuff to keep components or doors
            if ((target is Keeps.GameKeepComponent) == false && (target is Keeps.GameKeepDoor) == false)
            {
                base.ApplyEffectOnTarget(target, effectiveness);
            }

            if (Spell.Duration > 0 && Spell.Target != "Area" || Spell.Concentration > 0)
            {
                OnDirectEffect(target, effectiveness);
            }
        }

        private void DealDamage(GameLiving target, double effectiveness)
        {
            if (!target.IsAlive || target.ObjectState != GameObject.eObjectState.Active)
            {
                return;
            }

            if (target is Keeps.GameKeepDoor || target is Keeps.GameKeepComponent)
            {
                MessageToCaster("Your spell has no effect on the keep component!", eChatType.CT_SpellResisted);
                return;
            }

            // calc damage
            AttackData ad = CalculateDamageToTarget(target, effectiveness);
            SendDamageMessages(ad);
            DamageTarget(ad, true);
            target.StartInterruptTimer(target.SpellInterruptDuration, ad.AttackType, Caster);
            /*
            if (target.IsAlive)
                base.ApplyEffectOnTarget(target, effectiveness);*/
        }

        /*
         * We need to send resist spell los check packets because spell resist is calculated first, and
         * so you could be inside keep and resist the spell and be interupted when not in view
         */
        protected override void OnSpellResisted(GameLiving target)
        {
            if (target is GamePlayer && Caster.TempProperties.getProperty("player_in_keep_property", false))
            {
                GamePlayer player = target as GamePlayer;
                player.Out.SendCheckLOS(Caster, player, new CheckLOSResponse(ResistSpellCheckLOS));
            }
            else
            {
                SpellResisted(target);
            }
        }

        private void ResistSpellCheckLOS(GamePlayer player, ushort response, ushort targetOID)
        {
            if ((response & 0x100) == 0x100)
            {
                try
                {
                    if (Caster.CurrentRegion.GetObject(targetOID) is GameLiving target)
                    {
                        SpellResisted(target);
                    }
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                    {
                        log.Error($"targetOID:{targetOID} caster:{Caster} exception:{e}");
                    }
                }
            }
        }

        private void SpellResisted(GameLiving target)
        {
            base.OnSpellResisted(target);
        }

        /// <summary>
        /// Delve Info
        /// </summary>
        public override IList<string> DelveInfo
        {
            get
            {
                /*
                <Begin Info: Lesser Raven Bolt>
                Function: dmg w/resist decrease

                Damages the target, and lowers the target's resistance to that spell type.

                Damage: 32
                Resist decrease (Cold): 10%
                Target: Targetted
                Range: 1500
                Duration: 1:0 min
                Power cost: 5
                Casting time:      3.0 sec
                Damage: Cold

                <End Info>
                */

                var list = new List<string>();

                list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DirectDamageDebuffSpellHandler.DelveInfo.Function"));
                list.Add(" "); // empty line
                list.Add(Spell.Description);
                list.Add(" "); // empty line
                if (Spell.Damage != 0)
                {
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DelveInfo.Damage", Spell.Damage.ToString("0.###;0.###'%'")));
                }

                if (Spell.Value != 0)
                {
                    list.Add(string.Format(LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DirectDamageDebuffSpellHandler.DelveInfo.Decrease", DebuffTypeName, Spell.Value)));
                }

                list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DelveInfo.Target", Spell.Target));
                if (Spell.Range != 0)
                {
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DelveInfo.Range", Spell.Range));
                }

                if (Spell.Duration >= ushort.MaxValue * 1000)
                {
                    list.Add($"{LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DelveInfo.Duration")} Permanent.");
                }
                else if (Spell.Duration > 60000)
                {
                    list.Add($"{LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DelveInfo.Duration")}{Spell.Duration / 60000}:{Spell.Duration % 60000 / 1000:00} min");
                }
                else if (Spell.Duration != 0)
                {
                    list.Add($"{LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DelveInfo.Duration")}{Spell.Duration / 1000:0\' sec\';\'Permanent.\';\'Permanent.\'}");
                }

                if (Spell.Frequency != 0)
                {
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DelveInfo.Frequency", (Spell.Frequency * 0.001).ToString("0.0")));
                }

                if (Spell.Power != 0)
                {
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DelveInfo.PowerCost", Spell.Power.ToString("0;0'%'")));
                }

                list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DelveInfo.CastingTime", (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")));
                if (Spell.RecastDelay > 60000)
                {
                    list.Add($"{LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DelveInfo.RecastTime")}{Spell.RecastDelay / 60000}:{Spell.RecastDelay % 60000 / 1000:00} min");
                }
                else if (Spell.RecastDelay > 0)
                {
                    list.Add($"{LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DelveInfo.RecastTime")}{Spell.RecastDelay / 1000} sec");
                }

                if (Spell.Concentration != 0)
                {
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DelveInfo.ConcentrationCost", Spell.Concentration));
                }

                if (Spell.Radius != 0)
                {
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DelveInfo.Radius", Spell.Radius));
                }

                if (Spell.DamageType != eDamageType.Natural)
                {
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer)?.Client, "DelveInfo.Damage", GlobalConstants.DamageTypeToName(Spell.DamageType)));
                }

                return list;
            }
        }

        // constructor
        public DirectDamageDebuffSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}