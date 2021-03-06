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
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
    [SpellHandler("VampiirEffectivenessDeBuff")]
    public class VampiirEffectivenessDeBuff : SpellHandler
    {
        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {

            if (effect.Owner is GamePlayer)
            {
                base.OnEffectStart(effect);
                GamePlayer player = effect.Owner as GamePlayer;
                player.TempProperties.setProperty("PreEffectivenessDebuff", player.Effectiveness);

                double effectiveness = player.Effectiveness;
                double valueToAdd = Spell.Value * effectiveness / 100;
                valueToAdd = effectiveness - valueToAdd;

                if (valueToAdd > 0)
                {
                    player.Effectiveness = valueToAdd;
                }
                else
                {
                    player.Effectiveness = 0;
                }

                MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner);

                // Added to fix?
                player.Out.SendUpdateWeaponAndArmorStats();
                player.Out.SendStatusUpdate();
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (effect.Owner is GamePlayer)
            {
                GamePlayer player = effect.Owner as GamePlayer;
                player.Effectiveness = player.TempProperties.getProperty<double>("PreEffectivenessDebuff");
                player.TempProperties.removeProperty("PreEffectivenessDebuff");
                MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_Spell);
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner);

                // Added to fix?
                player.Out.SendUpdateWeaponAndArmorStats();
                player.Out.SendStatusUpdate();
            }

            return 0;
        }

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>(16)
                {
                    $"Name: {Spell.Name}",
                    $"Description: {Spell.Description}",
                    $"Target: {Spell.Target}",
                    $"Casting time: {(Spell.CastTime * 0.001):0.0## sec;-0.0## sec;'instant'}"
                };

                if (Spell.Duration >= ushort.MaxValue * 1000)
                {
                    list.Add("Duration: Permanent.");
                }
                else if (Spell.Duration > 60000)
                {
                    list.Add($"Duration: {Spell.Duration / 60000}:{Spell.Duration % 60000 / 1000:00} min");
                }
                else if (Spell.Duration != 0)
                {
                    list.Add($"Duration: {Spell.Duration / 1000:0' sec';'Permanent.';'Permanent.'}");
                }

                // Recast
                if (Spell.RecastDelay > 60000)
                {
                    list.Add($"Recast time: {Spell.RecastDelay / 60000}:{Spell.RecastDelay % 60000 / 1000:00} min");
                }
                else if (Spell.RecastDelay > 0)
                {
                    list.Add($"Recast time: {Spell.RecastDelay / 1000} sec");
                }

                // Range
                if (Spell.Range != 0)
                {
                    list.Add($"Range: {Spell.Range}");
                }

                // Radius
                if (Spell.Radius != 0)
                {
                    list.Add($"Radius: {Spell.Radius}");
                }

                // Cost
                if (Spell.Power != 0)
                {
                    list.Add($"Power cost: {Spell.Power:0;0'%'}");
                }

                // Effect
                list.Add($"Debuff Effectiveness Of target By: {Spell.Value}%");

                if (Spell.Frequency != 0)
                {
                    list.Add($"Frequency: {Spell.Frequency * 0.001:0.0}");
                }

                return list;
            }
        }

        public VampiirEffectivenessDeBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}