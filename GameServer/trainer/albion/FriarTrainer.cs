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
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Trainer
{
    /// <summary>
    /// Friar Trainer
    /// </summary>
    [NPCGuildScript("Friar Trainer", eRealm.Albion)] // this attribute instructs DOL to use this script for all "Friar Trainer" NPC's in Albion (multiple guilds are possible for one script)
    public class FriarTrainer : GameTrainer
    {
        public override eCharacterClass TrainedClass => eCharacterClass.Friar;

        /// <summary>
        /// The free starter armor from trainer
        /// </summary>
        private const string ArmorId1 = "friar_item";
        private const string ArmorId2 = "chaplains_robe";
        private const string ArmorId3 = "robes_of_the_neophyte";

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
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "FriarTrainer.Interact.Text2", Name), eChatType.CT_System, eChatLoc.CL_ChatWindow);

                if (player.Level >= 10 && player.Level < 15)
                {
                    if (player.Inventory.GetFirstItemByID(ArmorId3, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == null)
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "FriarTrainer.Interact.Text4", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        addGift(ArmorId3, player);
                    }

                    if (player.Inventory.GetFirstItemByID(ArmorId1, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == null)
                    { }
                    else
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "FriarTrainer.Interact.Text3", Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                }
            }
            else
            {
                // perhaps player can be promoted
                if (CanPromotePlayer(player))
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "FriarTrainer.Interact.Text1", Name, player.CharacterClass.Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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

            string lowerCase = text.ToLower();
            if (lowerCase == LanguageMgr.GetTranslation(player.Client.Account.Language, "FriarTrainer.WhisperReceiveCase.Text1"))
            {
                // promote player to other class
                if (CanPromotePlayer(player))
                {
                    PromotePlayer(player, (int)eCharacterClass.Friar, LanguageMgr.GetTranslation(player.Client.Account.Language, "FriarTrainer.WhisperReceive.Text1"), null);
                    addGift(ArmorId1, player);
                }
            }

            return true;
        }

        /// <summary>
        /// For Recieving Friar Item.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
            if (source == null || item == null)
            {
                return false;
            }

            if (!(source is GamePlayer player))
            {
                return false;
            }

            if (player.Level >= 10 && player.Level < 15 && item.Id_nb == ArmorId1)
            {
                player.Inventory.RemoveCountFromStack(item, 1);
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "FriarTrainer.ReceiveItem.Text1", Name, player.Name), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                addGift(ArmorId2, player);
            }

            return base.ReceiveItem(source, item);
        }
    }
}
