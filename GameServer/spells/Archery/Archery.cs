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
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using DOL.AI.Brain;
using System.Collections.Generic;

namespace DOL.GS.Spells
{
    [SpellHandler("Archery")]
    public class Archery : ArrowSpellHandler
    {
        public enum eShotType
        {
            Other = 0,
            Critical = 1,
            Power = 2,
            PointBlank = 3,
            Rapid = 4
        }

        /// <summary>
        /// Does this spell break stealth on start?
        /// </summary>
        public override bool UnstealthCasterOnStart => false;

        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (Caster.ObjectState != GameObject.eObjectState.Active)
            {
                return false;
            }

            if (!Caster.IsAlive)
            {
                MessageToCaster("You are dead and can't cast!", eChatType.CT_System);
                return false;
            }

            // Is PS ?
            GameSpellEffect Phaseshift = FindEffectOnTarget(Caster, "Phaseshift");
            if (Phaseshift != null && (Spell.InstrumentRequirement == 0 || Spell.SpellType == "Mesmerize"))
            {
                MessageToCaster("You're phaseshifted and can't cast a spell", eChatType.CT_System);
                return false;
            }

            // Is Shield Disarm ?
            ShieldTripDisarmEffect shieldDisarm = Caster.EffectList.GetOfType<ShieldTripDisarmEffect>();
            if (shieldDisarm != null)
            {
                MessageToCaster("You're disarmed and can't cast a spell", eChatType.CT_System);
                return false;
            }

            // Is Mentalist RA5L ?
            SelectiveBlindnessEffect SelectiveBlindness = Caster.EffectList.GetOfType<SelectiveBlindnessEffect>();
            if (SelectiveBlindness != null)
            {
                GameLiving EffectOwner = SelectiveBlindness.EffectSource;
                if (EffectOwner == selectedTarget)
                {
                    if (Caster is GamePlayer)
                    {
                        ((GamePlayer)Caster).Out.SendMessage($"{selectedTarget.GetName(0, true)} is invisible to you!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                    }

                    return false;
                }
            }

            // Is immune ?
            if (selectedTarget != null && selectedTarget.HasAbility("DamageImmunity"))
            {
                MessageToCaster($"{selectedTarget.Name} is immune to this effect!", eChatType.CT_SpellResisted);
                return false;
            }

            if (Caster.IsSitting)
            {
                MessageToCaster("You can't cast while sitting!", eChatType.CT_SpellResisted);
                return false;
            }

            if (Spell.RecastDelay > 0)
            {
                int left = Caster.GetSkillDisabledDuration(Spell);
                if (left > 0)
                {
                    MessageToCaster($"You must wait {left / 1000 + 1} seconds to use this spell!", eChatType.CT_System);
                    return false;
                }
            }

            string targetType = Spell.Target.ToLower();
            if (targetType == "area")
            {
                if (!Caster.IsWithinRadius(Caster.GroundTarget, CalculateSpellRange()))
                {
                    MessageToCaster("Your area target is out of range.  Select a closer target.", eChatType.CT_SpellResisted);
                    return false;
                }
            }

            if (targetType == "enemy")
            {
                if (Caster.IsObjectInFront(selectedTarget, 180) == false)
                {
                    MessageToCaster("Your target is not in view!", eChatType.CT_SpellResisted);
                    Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));
                    return false;
                }

                if (Caster.TargetInView == false)
                {
                    MessageToCaster("Your target is not visible!", eChatType.CT_SpellResisted);
                    Caster.Notify(GameLivingEvent.CastFailed, new CastFailedEventArgs(this, CastFailedEventArgs.Reasons.TargetNotInView));
                    return false;
                }
            }

            if (Caster is GamePlayer && Caster.AttackWeapon != null && GlobalConstants.IsBowWeapon((eObjectType)Caster.AttackWeapon.Object_Type))
            {
                if (Spell.LifeDrainReturn == (int)eShotType.Critical && (!Caster.IsStealthed))
                {
                    MessageToCaster("You must be stealthed and wielding a bow to use this ability!", eChatType.CT_SpellResisted);
                    return false;
                }

                return true;
            }
            else
            {
                if (Spell.LifeDrainReturn == (int)eShotType.Critical)
                {
                    MessageToCaster("You must be stealthed and wielding a bow to use this ability!", eChatType.CT_SpellResisted);
                    return false;
                }

                MessageToCaster("You must be wielding a bow to use this ability!", eChatType.CT_SpellResisted);
                return false;
            }
        }

        public override void SendSpellMessages()
        {
            MessageToCaster($"You prepare a {Spell.Name}", eChatType.CT_YouHit);
        }

        public override int CalculateToHitChance(GameLiving target)
        {
            int bonustohit = Caster.GetModified(eProperty.ToHitBonus);

            // miss rate is 0 on same level opponent
            int hitchance = 100 + bonustohit;

            if ((Caster is GamePlayer && target is GamePlayer) == false)
            {
                hitchance -= (int)(Caster.GetConLevel(target) * ServerProperties.Properties.PVE_SPELL_CONHITPERCENT);
                hitchance += Math.Max(0, target.Attackers.Count - 1) * ServerProperties.Properties.MISSRATE_REDUCTION_PER_ATTACKERS;
            }

            return hitchance;
        }

        /// <summary>
        /// Adjust damage based on chance to hit.
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="hitChance"></param>
        /// <returns></returns>
        public override int AdjustDamageForHitChance(int damage, int hitChance)
        {
            int adjustedDamage = damage;

            if (hitChance < 85)
            {
                adjustedDamage += (int)(adjustedDamage * (hitChance - 85) * 0.038);
            }

            return adjustedDamage;
        }

        /// <summary>
        /// Level mod for effect between target and caster if there is any
        /// </summary>
        /// <returns></returns>
        public override double GetLevelModFactor()
        {
            return 0.025;
        }

        public override AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
        {
            AttackData ad = base.CalculateDamageToTarget(target, effectiveness);
            GameSpellEffect bladeturn = FindEffectOnTarget(target, "Bladeturn");
            if (bladeturn != null)
            {
                GamePlayer player;
                switch (Spell.LifeDrainReturn)
                {
                    case (int)eShotType.Critical:
                        {
                            if (target is GamePlayer)
                            {
                                player = target as GamePlayer;
                                player.Out.SendMessage("A shot penetrated your magic barrier!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            }

                            ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;
                        }

                        break;
                    case (int)eShotType.Power:
                        {
                            player = target as GamePlayer;
                            player.Out.SendMessage("A shot penetrated your magic barrier!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;
                            bladeturn.Cancel(false);
                        }

                        break;
                    default:
                        {
                            if (Caster is GamePlayer)
                            {
                                player = Caster as GamePlayer;
                                player.Out.SendMessage("Your strike was absorbed by a magical barrier!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                            }

                            if (target is GamePlayer)
                            {
                                player = target as GamePlayer;
                                player.Out.SendMessage("The blow was absorbed by a magical barrier!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                                ad.AttackResult = GameLiving.eAttackResult.Missed;
                                bladeturn.Cancel(false);
                            }
                        }

                        break;
                }
            }

            if (ad.AttackResult != GameLiving.eAttackResult.Missed)
            {
                if (target is GameNPC npc)
                {
                    if (npc.Brain != null && (npc.Brain is IControlledBrain) == false)
                    {
                        // boost for npc damage until we find exactly where calculation is going wrong -tolakram
                        ad.Damage = (int)(ad.Damage * 1.57);
                    }
                }

                // Volley damage reduction based on live testing - tolakram
                if (Spell.Target.ToLower() == "area")
                {
                    ad.Damage = (int)(ad.Damage * 0.815);
                }
            }

            return ad;
        }

        /// <summary>
        /// Determines what damage type to use.  For archery the player can choose.
        /// </summary>
        /// <returns></returns>
        public override eDamageType DetermineSpellDamageType()
        {
            GameSpellEffect ef = FindEffectOnTarget(Caster, "ArrowDamageTypes");
            if (ef != null)
            {
                return ef.SpellHandler.Spell.DamageType;
            }

            return eDamageType.Slash;
        }

        /// <summary>
        /// Calculates the base 100% spell damage which is then modified by damage variance factors
        /// </summary>
        /// <returns></returns>
        public override double CalculateDamageBase(GameLiving target)
        {
            double spellDamage = Spell.Damage;

            if (Caster is GamePlayer player)
            {
                int manaStatValue = player.GetModified((eProperty)player.CharacterClass.ManaStat);
                spellDamage *= (manaStatValue + 300) / 275.0;
            }

            if (spellDamage < 0)
            {
                spellDamage = 0;
            }

            return spellDamage;
        }

        public override void FinishSpellCast(GameLiving target)
        {
            if (target == null && Spell.Target.ToLower() != "area")
            {
                return;
            }

            if (Caster == null)
            {
                return;
            }

            if (Caster is GamePlayer && Caster.IsStealthed)
            {
                (Caster as GamePlayer).Stealth(false);
            }

            if (Spell.Target.ToLower() == "area")
            {
                // always put archer into combat when using area (volley)
                Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
                Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;

                foreach (GameLiving npc in WorldMgr.GetNPCsCloseToSpot(Caster.CurrentRegionID, Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, (ushort)Spell.Radius))
                {
                    if (npc.Realm == 0 || Caster.Realm == 0)
                    {
                        npc.LastAttackedByEnemyTickPvE = npc.CurrentRegion.Time;
                    }
                }
            }
            else
            {
                if (target.Realm == 0 || Caster.Realm == 0)
                {
                    target.LastAttackedByEnemyTickPvE = target.CurrentRegion.Time;
                    Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
                }
                else
                {
                    target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;
                    Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;
                }
            }

            base.FinishSpellCast(target);
        }

        /// <summary>
        /// Calculates the effective casting time
        /// </summary>
        /// <returns>effective casting time in milliseconds</returns>
        public override int CalculateCastingTime()
        {
            if (Spell.LifeDrainReturn == (int)eShotType.Power)
            {
                return 6000;
            }

            int ticks = Spell.CastTime;

            double percent = 1.0;
            int dex = Caster.GetModified(eProperty.Dexterity);

            if (dex < 60)
            {
                // do nothing.
            }
            else if (dex < 250)
            {
                percent = 1.0 - (dex - 60) * 0.15 * 0.01;
            }
            else
            {
                percent = 1.0 - ((dex - 60) * 0.15 + (dex - 250) * 0.05) * 0.01;
            }

            if (Caster is GamePlayer player)
            {
                percent *= 1.0 - Caster.GetModified(eProperty.CastingSpeed) * 0.01;
            }

            ticks = (int)(ticks * Math.Max(Caster.CastingSpeedReductionCap, percent));

            if (ticks < Caster.MinimumCastingSpeed)
            {
                ticks = Caster.MinimumCastingSpeed;
            }

            return ticks;
        }

        public override int PowerCost(GameLiving target) { return 0; }

        public override int CalculateEnduranceCost()
        {
            int syphon = Caster.GetModified(eProperty.ArcaneSyphon);
            if (syphon > 0)
            {
                if (Util.Chance(syphon))
                {
                    return 0;
                }
            }
            return (int)(Caster.MaxEndurance * (Spell.Power * .01));
        }

        public override bool CasterIsAttacked(GameLiving attacker)
        {
            if (Spell.Uninterruptible)
            {
                return false;
            }

            if (IsCasting && Stage < 2)
            {
                double mod = Caster.GetConLevel(attacker);
                double chance = 65;
                chance += mod * 10;
                chance = Math.Max(1, chance);
                chance = Math.Min(99, chance);
                if (attacker is GamePlayer)
                {
                    chance = 100;
                }

                if (Util.Chance((int)chance))
                {
                    Caster.TempProperties.setProperty(INTERRUPT_TIMEOUT_PROPERTY, Caster.CurrentRegion.Time + Caster.SpellInterruptDuration);
                    MessageToLiving(Caster, $"{attacker.GetName(0, true)} attacks you and your shot is interrupted!", eChatType.CT_SpellResisted);
                    InterruptCasting();
                    return true;
                }
            }

            return true;
        }

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>();

                // list.Add("Function: " + (Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType));
                // list.Add(" "); //empty line
                list.Add(Spell.Description);
                list.Add(" "); // empty line
                if (Spell.InstrumentRequirement != 0)
                {
                    list.Add($"Instrument require: {GlobalConstants.InstrumentTypeToName(Spell.InstrumentRequirement)}");
                }

                if (Spell.Damage != 0)
                {
                    list.Add($"Damage: {Spell.Damage:0.###;0.###'%'}");
                }
                else if (Spell.Value != 0)
                {
                    list.Add($"Value: {Spell.Value:0.###;0.###'%'}");
                }

                list.Add($"Target: {Spell.Target}");
                if (Spell.Range != 0)
                {
                    list.Add($"Range: {Spell.Range}");
                }

                if (Spell.Duration >= ushort.MaxValue * 1000)
                {
                    list.Add("Duration: Permanent.");
                }
                else if (Spell.Duration > 60000)
                {
                    list.Add($"Duration: {Spell.Duration / 60000}:{(Spell.Duration % 60000 / 1000).ToString("00")} min");
                }
                else if (Spell.Duration != 0)
                {
                    list.Add($"Duration: {Spell.Duration / 1000:0' sec';'Permanent.';'Permanent.'}");
                }

                if (Spell.Frequency != 0)
                {
                    list.Add($"Frequency: {Spell.Frequency * 0.001:0.0}");
                }

                if (Spell.Power != 0)
                {
                    list.Add($"Endurance cost: {Spell.Power:0;0'%'}");
                }

                list.Add($"Casting time: {Spell.CastTime * 0.001:0.0## sec;-0.0## sec;'instant'}");
                if (Spell.RecastDelay > 60000)
                {
                    list.Add($"Recast time: {Spell.RecastDelay / 60000}:{Spell.RecastDelay % 60000 / 1000:00} min");
                }
                else if (Spell.RecastDelay > 0)
                {
                    list.Add($"Recast time: {Spell.RecastDelay / 1000} sec");
                }

                if (Spell.Radius != 0)
                {
                    list.Add($"Radius: {Spell.Radius}");
                }

                if (Spell.DamageType != eDamageType.Natural)
                {
                    list.Add($"Damage: {GlobalConstants.DamageTypeToName(Spell.DamageType)}");
                }

                return list;
            }
        }

        /// <summary>
        /// Do not trigger Subspells
        /// </summary>
        /// <param name="target"></param>
        public override void CastSubSpells(GameLiving target)
        {
        }

        public Archery(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
