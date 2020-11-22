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
    /// Eldritch Trainer
    /// </summary>
    [NPCGuildScript("Eldritch Trainer", eRealm.Hibernia)] // this attribute instructs DOL to use this script for all "Eldritch Trainer" NPC's in Albion (multiple guilds are possible for one script)
    public class EldritchTrainer : GameTrainer
    {
        public override eCharacterClass TrainedClass => eCharacterClass.Eldritch;

        private const string WeaponId1 = "eldritch_item";

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
                player.Out.SendMessage(Name + " says, \"Drink up this knowledge, " + player.Name + ", and remember it, for there shall be a day when I no longer rise in the morning, and you may be required to take my place.\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
            }
            else
            {
                // perhaps player can be promoted
                if (CanPromotePlayer(player))
                {
                    player.Out.SendMessage(Name + " says, \"Greetings, " + player.Name + ". It is my understanding that you have chosen the Path of Focus, and wish to train as an [Eldritch].\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
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
                case "Eldritch":
                    // promote player to other class
                    if (CanPromotePlayer(player)) {
                        PromotePlayer(player, (int)eCharacterClass.Eldritch, "I can give you the gift of knowledge, but wisdom you must seek on your own. I welcome you, " + source.GetName(0, false) + ". Here, take this welcoming gift. Use it wisely.", null);
                        player.ReceiveItem(this,WeaponId1);
                    }

                    break;
            }

            return true;
        }
    }
}
