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
    /// Bard Trainer
    /// </summary>
    [NPCGuildScript("Bard Trainer", eRealm.Hibernia)] // this attribute instructs DOL to use this script for all "Bard Trainer" NPC's in Albion (multiple guilds are possible for one script)
    public class BardTrainer : GameTrainer
    {
        public override eCharacterClass TrainedClass => eCharacterClass.Bard;

        public const string WeaponId1 = "bard_item";

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
                player.Out.SendMessage(Name + " says, \"Ahh, well met " + player.Name + ". Back for more lore and knowledge, eh.\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
            }
            else
            {
                // perhaps player can be promoted
                if (CanPromotePlayer(player))
                {
                    player.Out.SendMessage(Name + " says, \"Do you wish to walk the Path of Essence, pursuing a life of storytelling and [song]? Of brave deeds which grow braver with every telling?\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
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

            switch (text) {
                case "song":
                    // promote player to other class
                    if (CanPromotePlayer(player)) {
                        PromotePlayer(player, (int)eCharacterClass.Bard, "Welcome then, " + source.GetName(0, false) + ", to the Bard's life. Here, take this. Keep it well, " + source.GetName(0, false) + ", for the tools of our trade can be quite expensive.", null);
                        player.ReceiveItem(this,WeaponId1);
                    }

                    break;
            }

            return true;
        }
    }
}