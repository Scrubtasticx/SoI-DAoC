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
using DOL.GS.PacketHandler;

namespace DOL.GS.Trainer
{
    /// <summary>
    /// Mercenary Trainer
    /// </summary>
    [NPCGuildScript("Mercenary Trainer", eRealm.Albion)] // this attribute instructs DOL to use this script for all "Mercenary Trainer" NPC's in Albion (multiple guilds are possible for one script)
    public class MercenaryTrainer : GameTrainer
    {
        public override eCharacterClass TrainedClass => eCharacterClass.Mercenary;

        private const string WeaponId1 = "slash_sword_item";
        private const string WeaponId2 = "crush_sword_item";
        private const string WeaponId3 = "thrust_sword_item";

        /// <summary>
        /// Interact with trainer
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
             {
                 return false;
             }

            // check if class matches.
            if (player.CharacterClass.ID == (int)TrainedClass)
            {
                OfferTraining(player);
            }
            else
            {
                // perhaps player can be promoted
                if (CanPromotePlayer(player))
                {
                    player.Out.SendMessage(Name + " says, \"Do you wish to [join the Guild of Shadows] and seek your fortune as a Mercenary?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
                    if (!player.IsLevelRespecUsed)
                    {
                        OfferRespecialize(player);
                    }
                }
                else
                {
                    CheckChampionTraining(player);
                }
            }

            return true;
        }

        /// <summary>
        /// Talk to trainer
        /// </summary>
        /// <param name="source"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public override bool WhisperReceive(GameLiving source, string text)
        {
            if (!base.WhisperReceive(source, text))
            {
                return false;
            }

            if (!(source is GamePlayer player))
            {
                return false;
            }

            if (CanPromotePlayer(player))
            {
                switch (text)
                {
                    case "join the Guild of Shadows":
                        player.Out.SendMessage(Name + " says, \"Very well. Choose a weapon, and you shall become one of us. Which would you have, [slashing], [crushing], or [thrusting]?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
                        break;
                    case "slashing":

                        PromotePlayer(player, (int)eCharacterClass.Mercenary, "Here is your Sword of the Initiate. Welcome to the Guild of Shadows.", null);
                        player.ReceiveItem(this,WeaponId1);
                        break;
                    case "crushing":

                        PromotePlayer(player, (int)eCharacterClass.Mercenary, "Here is your Mace of the Initiate. Welcome to the Guild of Shadows.", null);
                        player.ReceiveItem(this,WeaponId2);
                        break;
                    case "thrusting":

                        PromotePlayer(player, (int)eCharacterClass.Mercenary, "Here is your Rapier of the Initiate. Welcome to the Guild of Shadows.", null);
                        player.ReceiveItem(this,WeaponId3);
                        break;
                }
            }

            return true;
        }
    }
}