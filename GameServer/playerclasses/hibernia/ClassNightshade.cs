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

namespace DOL.GS.PlayerClass
{
    /// <summary>
    /// Hibernia Nightshade Class
    /// </summary>
    [CharacterClass((int)eCharacterClass.Nightshade, "Nightshade", "Stalker")]
    public class ClassNightshade : ClassStalker
    {
        private static readonly string[] AutotrainableSkills = { Specs.Stealth };

        public ClassNightshade()
        {
            Profession = "PlayerClass.Profession.PathofEssence";
            SpecPointsMultiplier = 22;
            PrimaryStat = eStat.DEX;
            SecondaryStat = eStat.QUI;
            TertiaryStat = eStat.STR;
            ManaStat = eStat.DEX;
        }

        public override bool CanUseLefthandedWeapon => true;

        public override eClassType ClassType => eClassType.Hybrid;

        public override IList<string> GetAutotrainableSkills()
        {
            return AutotrainableSkills;
        }

        public override bool HasAdvancedFromBaseClass()
        {
            return true;
        }
    }
}
