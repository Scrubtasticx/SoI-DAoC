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
using System.Collections.Generic;
using DOL.AI.Brain;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Simulates disarming a target by stopping their attack
    /// </summary>
    [SpellHandler("Disarm")]
    public class DisarmSpellHandler : SpellHandler
    {
        /// <summary>
        /// called after normal spell cast is completed and effect has to be started
        /// </summary>
        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        /// <summary>
        /// When an applied effect starts
        /// duration spells only
        /// </summary>
        /// <param name="effect"></param>
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            if (effect.Owner.Realm == 0 || Caster.Realm == 0)
            {
                effect.Owner.LastAttackedByEnemyTickPvE = effect.Owner.CurrentRegion.Time;
                Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
            }
            else
            {
                effect.Owner.LastAttackedByEnemyTickPvP = effect.Owner.CurrentRegion.Time;
                Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;
            }

            effect.Owner.DisarmedTime = effect.Owner.CurrentRegion.Time + CalculateEffectDuration(effect.Owner, Caster.Effectiveness);
            effect.Owner.StopAttack();
            MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), eChatType.CT_Spell, effect.Owner);
            effect.Owner.StartInterruptTimer(effect.Owner.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
            if (effect.Owner is GameNPC npc && npc.Brain is IOldAggressiveBrain aggroBrain)
            {
                aggroBrain.AddToAggroList(Caster, 1);
            }
        }

        /// <summary>
        /// When an applied effect expires.
        /// Duration spells only.
        /// </summary>
        /// <param name="effect">The expired effect</param>
        /// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
        /// <returns>immunity duration in milliseconds</returns>
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            // effect.Owner.IsDisarmed = false;
            if (!noMessages)
            {
                MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
            }

            return base.OnEffectExpires(effect,noMessages);
        }

        /// <summary>
        /// Delve Info
        /// </summary>
        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    $"Function: {(Spell.SpellType == string.Empty ? "(not implemented)" : Spell.SpellType)}",
                    " ",
                    Spell.Description,
                    " "
                };

                if (Spell.Duration != 0)
                {
                    list.Add($"Duration: {Spell.Duration / 1000}sec");
                }

                list.Add($"Target: {Spell.Target}");
                if (Spell.Range != 0)
                {
                    list.Add($"Range: {Spell.Range}");
                }

                if (Spell.Power != 0)
                {
                    list.Add($"Power cost: {Spell.Power:0;0'%'}");
                }

                list.Add($"Casting time: {(Spell.CastTime * 0.001):0.0## sec;-0.0## sec;'instant'}");
                if (Spell.RecastDelay > 60000)
                {
                    list.Add($"Recast time: {Spell.RecastDelay / 60000}:{Spell.RecastDelay % 60000 / 1000:00} min");
                }
                else if (Spell.RecastDelay > 0)
                {
                    list.Add($"Recast time: {Spell.RecastDelay / 1000} sec");
                }

                return list;
            }
        }

        // constructor
        public DisarmSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }
    }
}