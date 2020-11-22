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
    /// Viking Trainer
    /// </summary>
    [NPCGuildScript("Viking Trainer", eRealm.Midgard)] // this attribute instructs DOL to use this script for all "Acolyte Trainer" NPC's in Albion (multiple guilds are possible for one script)
    public class VikingTrainer : GameTrainer
    {
        private const string PracticeWeaponId = "training_axe";

        public override eCharacterClass TrainedClass => eCharacterClass.Viking;

        public VikingTrainer() : base(eChampionTrainerType.Viking)
        {
        }

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

            // check if class matches
            if (player.CharacterClass.ID == (int)TrainedClass)
            {
                // player can be promoted
                if (player.Level >= 5)
                {
                    player.Out.SendMessage(Name + " says, \"You must now seek your training elsewhere. Which path would you like to follow? [Warrior], [Berserker], [Skald] or [Thane]?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }
                else
                {
                    OfferTraining(player);
                }

                // ask for basic equipment if player doesnt own it
                if (player.Inventory.GetFirstItemByID(PracticeWeaponId, eInventorySlot.MinEquipable, eInventorySlot.LastBackpack) == null)
                {
                    player.Out.SendMessage(Name + " says, \"Do you require a [practice weapon]?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
                }
            }
            else
            {
                CheckChampionTraining(player);
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
                case "Warrior":
                    if (player.Race == (int)eRace.Dwarf || player.Race == (int)eRace.Kobold || player.Race == (int)eRace.Norseman || player.Race == (int)eRace.Troll || player.Race == (int)eRace.Valkyn || player.Race == (int)eRace.MidgardMinotaur) {
                        player.Out.SendMessage(Name + " says, \"I can't tell you something about this class.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
                    }
                    else {
                        player.Out.SendMessage(Name + " says, \"The path of a Warrior is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
                    }

                    return true;
                case "Berserker":
                    if (player.Race == (int)eRace.Dwarf || player.Race == (int)eRace.Troll || player.Race == (int)eRace.Norseman || player.Race == (int)eRace.Valkyn || player.Race == (int)eRace.MidgardMinotaur)
                    {
                        player.Out.SendMessage(Name + " says, \"I can't tell you something about this class.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
                    }
                    else {
                        player.Out.SendMessage(Name + " says, \"The path of a Berserker is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
                    }

                    return true;
                case "Skald":
                    if (player.Race == (int)eRace.Dwarf || player.Race == (int)eRace.Kobold || player.Race == (int)eRace.Norseman || player.Race == (int)eRace.Troll) {
                        player.Out.SendMessage(Name + " says, \"I can't tell you something about this class.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
                    }
                    else {
                        player.Out.SendMessage(Name + " says, \"The path of a Skald is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
                    }

                    return true;
                case "Thane":
                    if (player.Race == (int)eRace.Dwarf || player.Race == (int)eRace.Frostalf || player.Race == (int)eRace.Norseman || player.Race == (int)eRace.Troll)
                    {
                        player.Out.SendMessage(Name + " says, \"I can't tell you something about this class.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
                    }
                    else
                    {
                        player.Out.SendMessage(Name + " says, \"The path of a Thane is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
                    }

                    return true;
                case "practice weapon":
                    if (player.Inventory.GetFirstItemByID(PracticeWeaponId, eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null)
                    {
                        player.ReceiveItem(this,PracticeWeaponId);
                    }

                    return true;
            }

            return true;
        }
    }
}