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

namespace DOL.GS
{
    /// <summary>
    /// Midgard teleporter.
    /// </summary>
    /// <author>Aredhel</author>
    public class MidgardTeleporter : GameTeleporter
    {
        /// <summary>
        /// Player right-clicked the teleporter.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
            {
                return false;
            }

            string intro = string.Format(
                "Greetings. I can channel the energies of this place to send you {0} {1} {2} {3} {4} {5}",
                                         "to far away lands. If you wish to fight in the Frontiers I can send you to [Uppland] or to the",
                                         "border keeps [Svasud Faste] and [Vindsaul Faste]. Maybe you wish to undertake the Trials of",
                                         "Atlantis in [Oceanus] haven or wish to visit the [City of Aegirhamn] and the [Shrouded Isles]?",
                                         "You could explore the [Gotar] or perhaps you wish to meet the citizens inside",                                    
                                         "the great city of [Jordheim]?",
                                         "Or perhaps you are interested in porting to our training camp [Hafheim]?");
            SayTo(player, intro);
            return true;
        }

        /// <summary>
        /// Player has picked a subselection.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="subSelection"></param>
        protected override void OnSubSelectionPicked(GamePlayer player, Teleport subSelection)
        {
            switch (subSelection.TeleportID.ToLower())
            {
                case "shrouded isles":
                    {
                        string reply = string.Format(
                            "The isles of Aegir are an excellent choice. {0} {1}",
                                                     "Would you prefer the city of [Aegirhamn] or perhaps one of the outlying towns",
                                                     "like [Bjarken], [Hagall], or [Knarr]?");
                        SayTo(player, reply);
                        return;
                    }               
            }

            base.OnSubSelectionPicked(player, subSelection);
        }

        /// <summary>
        /// Player has picked a destination.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="destination"></param>
        protected override void OnDestinationPicked(GamePlayer player, Teleport destination)
        {
            switch (destination.TeleportID.ToLower())
            {
                case "bjarken":
                    break;  // No text?
                case "city of aegirhamn":
                    SayTo(player, "The Shrouded Isles await you.");
                    break;
                case "gotar":
                    SayTo(player, "You shall soon arrive in the Gotar.");
                    break;
                case "hagall":
                    break;  // No text?
                case "jordheim":
                    SayTo(player, "The great city awaits!");
                    break;
                case "knarr":
                    break;  // No text?
                case "oceanus":
                    if (player.Client.Account.PrivLevel < ServerProperties.Properties.ATLANTIS_TELEPORT_PLVL)
                    {
                        SayTo(player, "I'm sorry, but you are not authorized to enter Atlantis at this time.");
                        return;
                    }

                    SayTo(player, "You will soon arrive in the Haven of Oceanus.");
                    break;                
                case "svasud faste":
                    SayTo(player, "Svasud Faste is what you seek, and Svasud Faste is what you shall find.");
                    break;
                case "uppland":
                    SayTo(player, "Now to the Frontiers for the glory of the realm!");
                    break;
                case "vindsaul faste":
                    SayTo(player, "Vindsaul Faste is what you seek, and Vindsaul Faste is what you shall find.");
                    break;
                case "hafheim":
                    if (ServerProperties.Properties.DISABLE_TUTORIAL)
                    {
                        SayTo(player,"Sorry, this place is not available for now !");
                        return;
                    }

                    if (player.Level > 15)
                    {
                        SayTo(player,"Sorry, you are far too experienced to enjoy this place !");
                        return;
                    }

                    break;
                default:
                    SayTo(player, "This destination is not yet supported.");
                    return;
            }

            base.OnDestinationPicked(player, destination);
        }

        /// <summary>
        /// Teleport the player to the designated coordinates.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="destination"></param>
        protected override void OnTeleport(GamePlayer player, Teleport destination)
        {
            OnTeleportSpell(player, destination);
        }
    }
}
