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
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Quests.Atlantis.Artifacts
{
    /// <summary>
    /// Quest for the Arms of the Winds artifact.
    /// </summary>
    /// <author>Aredhel</author>
    class ArmsOfTheWinds : ArtifactQuest
    {
        public ArmsOfTheWinds()
        { }

        public ArmsOfTheWinds(GamePlayer questingPlayer)
            : base(questingPlayer) { }

        /// <summary>
        /// This constructor is needed to load quests from the DB.
        /// </summary>
        /// <param name="questingPlayer"></param>
        /// <param name="dbQuest"></param>
        public ArmsOfTheWinds(GamePlayer questingPlayer, DBQuest dbQuest)
            : base(questingPlayer, dbQuest) { }

        /// <summary>
        /// Quest initialisation.
        /// </summary>
        public static void Init()
        {
            Init("Arms of the Winds", typeof(ArmsOfTheWinds));
        }

        /// <summary>
        /// Check if player is eligible for this quest.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool CheckQuestQualification(GamePlayer player)
        {
            if (!base.CheckQuestQualification(player))
            {
                return false;
            }

            // TODO: Check if this is the correct level for the quest.
            return player.Level >= 45;
        }

        /// <summary>
        /// Handle an item given to the scholar.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="item"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public override bool ReceiveItem(GameLiving source, GameLiving target, InventoryItem item)
        {
            if (base.ReceiveItem(source, target, item))
            {
                return true;
            }

            if (!(source is GamePlayer player) || !(target is Scholar scholar))
            {
                return false;
            }

            if (Step == 2 && ArtifactMgr.GetArtifactID(item.Name) == ArtifactId)
            {
                string armorType = GlobalConstants.ArmorLevelToName(player.BestArmorLevel, player.Realm);
                ItemTemplate template = null;
                Dictionary<string, ItemTemplate> versions = ArtifactMgr.GetArtifactVersions(ArtifactId, (eCharacterClass)player.CharacterClass.ID, player.Realm);

                foreach (string versionKey in versions.Keys)
                {
                    string[] serializedKey = versionKey.Split(';');
                    if (serializedKey[0] == armorType)
                    {
                        template = versions[versionKey];
                        break;
                    }
                }

                if (template != null && RemoveItem(player, item))
                {
                    GiveItem(scholar, player, ArtifactId, template);
                    string reply = $"I feel fortunate to be able to study some of these artifacts that have survived from the days of the Atlanteans. I am only sad that I shall never get to meet the people that created wonderful objects like the Arms of the Winds. The magic in them has been reawakened for you, {player.CharacterClass.Name}. You must take care of these sleeves because I cannot do it again. If you lose them or they are  destroyed, the Arms of the Winds will be forever lost to you. I hope they serve you well, {player.CharacterClass.Name}.";
                    scholar.TurnTo(player);
                    scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
                    FinishQuest();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Handle whispers to the scholar.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public override bool WhisperReceive(GameLiving source, GameLiving target, string text)
        {
            if (base.WhisperReceive(source, target, text))
            {
                return true;
            }

            // ReSharper disable once UsePatternMatching
            GamePlayer player = source as GamePlayer;
            if (player == null || !(target is Scholar scholar))
            {
                return false;
            }

            if (Step == 1 && text.ToLower() == ArtifactId.ToLower())
            {
                string reply ="The Arms of the Winds! If I only had Anthos\' Fish Skin. It is important that I have the scales, since Anthos trapped the magic of the Arms of the Winds in the Skin. If you have lost the skin, and I hope you haven\'t, you will have to go find the scales again and bring them to me.";
                scholar.TurnTo(player);
                scholar.SayTo(player, eChatLoc.CL_PopupWindow, reply);
                Step = 2;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Description for the current step.
        /// </summary>
        public override string Description
        {
            get
            {
                switch (Step)
                {
                    case 1:
                        return "Defeat Raging Tornado.";
                    case 2:
                        return "Turn in Anthos' Fish Skin in the Hall of Heroes or in the Oceanus Haven.";
                    default:
                        return base.Description;
                }
            }
        }

        /// <summary>
        /// The name of the quest (not necessarily the same as
        /// the name of the reward).
        /// </summary>
        public override string Name => "Arms of the Winds";

        /// <summary>
        /// The reward for this quest.
        /// </summary>
        public override string ArtifactId => "Arms of the Winds";
    }
}
