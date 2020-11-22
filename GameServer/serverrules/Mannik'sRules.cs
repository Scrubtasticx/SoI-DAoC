using System;
using System.Collections;
using DOL;
using DOL.Database;
using DOL.Language;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using System.Collections.Generic;

namespace DOL.GS.ServerRules
{
    /// <summary>
    /// Set of rules for "PvP" server type.
    /// </summary>
    [ServerRules(eGameServerType.GST_PvE)]
    public class PvEServerRules : AbstractServerRules
    {
        public override string RulesDescription()
        {
            return "PvE/RvR/PvP ServerRules";
        }

        protected const string KILLED_BY_PLAYER_PROP = "PvP killed by player";

        public override void ImmunityExpiredCallback(GamePlayer player)
        {
            if (player.ObjectState != GameObject.eObjectState.Active) return;
            if (player.Client.IsPlaying == false) return;
            else
                player.Out.SendMessage("Your temporary invulnerability timer has expired.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

            return;
        }

        public override void OnPlayerKilled(GamePlayer killedPlayer, GameObject killer)
        {
            base.OnPlayerKilled(killedPlayer, killer);
            if (killer == null || killer is GamePlayer)
                killedPlayer.TempProperties.setProperty(KILLED_BY_PLAYER_PROP, KILLED_BY_PLAYER_PROP);
            else
                killedPlayer.TempProperties.removeProperty(KILLED_BY_PLAYER_PROP);
        }
        public override void OnReleased(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = (GamePlayer)sender;
            if (player.TempProperties.getProperty<object>(KILLED_BY_PLAYER_PROP, null) != null)
            {
                player.TempProperties.removeProperty(KILLED_BY_PLAYER_PROP);
                StartImmunityTimer(player, ServerProperties.Properties.TIMER_KILLED_BY_PLAYER * 1000);//When Killed by a Player
            }
            else
            {
                StartImmunityTimer(player, ServerProperties.Properties.TIMER_KILLED_BY_MOB * 1000);//When Killed by a Mob
            }
        }
        public override void OnPlayerTeleport(GamePlayer player, GameLocation source, Teleport destination)
        {
            // Since region change already starts an immunity timer we only want to do this if a player
            // is teleporting within the same region
            if (source.RegionID == destination.RegionID)
            {
                StartImmunityTimer(player, ServerProperties.Properties.TIMER_PVP_TELEPORT * 1000);
            }
        }
        protected int[] m_rvrRegions =
        {
             163, // new frontiers
             236, //Claret
             237, //Killaloe
             238, //Thid
             239, //Braemar
             240, //Wilton
             241, //Molvik
             242, //Leirvik

		};
        protected int[] m_pvpRegions =
        {
          341, // Cave of Cruachan
		};

        public override bool IsAllowedToAttack(GameLiving attacker, GameLiving defender, bool quiet)
        {
            #region rvr
            if (m_rvrRegions != null)
            {
                foreach (int rvrreg in m_rvrRegions)
                    if (attacker.CurrentRegionID == rvrreg)
                    {
                        if (!base.IsAllowedToAttack(attacker, defender, quiet))
                            return false;

                        //Don't allow attacks on same realm members on Normal Servers
                        if (attacker.Realm == defender.Realm && !(attacker is GamePlayer && ((GamePlayer)attacker).DuelTarget == defender))
                        {
                            // allow confused mobs to attack same realm
                            if (attacker is GameNPC && (attacker as GameNPC).IsConfused)
                                return true;

                            if (attacker.Realm == 0)
                            {
                                return FactionMgr.CanLivingAttack(attacker, defender);
                            }

                            if (quiet == false) MessageToLiving(attacker, "You can't attack a member of your realm!");
                            return false;
                        }

                        return true;
                    }
            }
            #endregion
            #region PvP
            if (m_pvpRegions != null)
            {
                foreach (int pvpreg in m_pvpRegions)
                    if (attacker.CurrentRegionID == pvpreg)
                    {
                        if (!base.IsAllowedToAttack(attacker, defender, quiet))
                        {
                            return false;
                        }
                        // if controlled NPC - do checks for owner instead
                        if (attacker is GameNPC)
                        {
                            IControlledBrain controlled = ((GameNPC)attacker).Brain as IControlledBrain;
                            if (controlled != null)
                            {
                                attacker = controlled.GetPlayerOwner();
                                quiet = true; // silence all attacks by controlled npc
                            }
                        }
                        if (defender is GameNPC)
                        {
                            IControlledBrain controlled = ((GameNPC)defender).Brain as IControlledBrain;
                            if (controlled != null)
                                defender = controlled.GetPlayerOwner();
                        }
                        // ogre: sometimes other players shouldn't be attackable            
                        if (attacker is GamePlayer playerAttacker && defender is GamePlayer playerDefender)
                        {
                            // check group
                            if (playerAttacker.Group != null && playerAttacker.Group.IsInTheGroup(playerDefender))
                            {
                                if (!quiet)
                                {
                                    MessageToLiving(playerAttacker, "You can't attack your group members.");
                                }

                                return false;
                            }

                            if (playerAttacker.DuelTarget != defender)
                            {
                                // check guild
                                if (playerAttacker.Guild != null && playerAttacker.Guild == playerDefender.Guild)
                                {
                                    if (!quiet)
                                    {
                                        MessageToLiving(playerAttacker, "You can't attack your guild members.");
                                    }

                                    return false;
                                }

                                // Player can't hit other members of the same BattleGroup
                                BattleGroup mybattlegroup = (BattleGroup)playerAttacker.TempProperties.getProperty<object>(BattleGroup.BATTLEGROUP_PROPERTY, null);

                                if (mybattlegroup != null && mybattlegroup.IsInTheBattleGroup(playerDefender))
                                {
                                    if (!quiet)
                                    {
                                        MessageToLiving(playerAttacker, "You can't attack a member of your battlegroup.");
                                    }

                                    return false;
                                }


                            }
                        }


                        if (attacker.Realm == 0 && defender.Realm == 0)
                        {
                            return FactionMgr.CanLivingAttack(attacker, defender);
                        }

                        // allow confused mobs to attack same realm
                        if (attacker is GameNPC && (attacker as GameNPC).IsConfused && attacker.Realm == defender.Realm)
                        {
                            return true;
                        }

                        // "friendly" NPCs can't attack "friendly" players
                        if (defender is GameNPC && defender.Realm != 0 && attacker.Realm != 0 && defender is GameKeepGuard == false && defender is GameFont == false)
                        {
                            if (quiet == false)
                            {
                                MessageToLiving(attacker, "You can't attack a friendly NPC!");
                            }

                            return false;
                        }

                        // "friendly" NPCs can't be attacked by "friendly" players
                        if (attacker is GameNPC && attacker.Realm != 0 && defender.Realm != 0 && attacker is GameKeepGuard == false)
                        {
                            return false;
                        }


                        return true;
                    }
            }

            #endregion
            if (!base.IsAllowedToAttack(attacker, defender, quiet))

                return false;

            // if controlled NPC - do checks for owner instead
            if (attacker is GameNPC)
            {
                IControlledBrain controlled = ((GameNPC)attacker).Brain as IControlledBrain;
                if (controlled != null)
                {
                    attacker = controlled.GetPlayerOwner();
                    quiet = true; // silence all attacks by controlled npc
                }
            }
            if (defender is GameNPC)
            {
                IControlledBrain controlled = ((GameNPC)defender).Brain as IControlledBrain;
                if (controlled != null)
                    defender = controlled.GetPlayerOwner();
            }

            //"You can't attack yourself!"
            if (attacker == defender)
            {
                if (quiet == false) MessageToLiving(attacker, "You can't attack yourself!");
                return false;
            }

            // Pet release might cause one of these to be null
            if (attacker == null || defender == null)
                return false;

            if (attacker.Realm != eRealm.None && defender.Realm != eRealm.None)
            {
                if (attacker is GamePlayer && ((GamePlayer)attacker).DuelTarget == defender)
                    return true;
                if (quiet == false) MessageToLiving(attacker, "You can not attack other players in this area!");
                return false;
            }

            //allow attacks on same realm only under the following circumstances
            if (attacker.Realm == defender.Realm)
            {
                //allow confused mobs to attack same realm
                if (attacker is GameNPC && (attacker as GameNPC).IsConfused)
                    return true;

                // else, don't allow mobs to attack mobs
                if (attacker.Realm == eRealm.None)
                {
                    return FactionMgr.CanLivingAttack(attacker, defender);
                }

                if (quiet == false) MessageToLiving(attacker, "You can't attack a member of your realm!");
                return false;
            }


            return true;

        }

        /// <summary>
        /// Is caster allowed to cast a spell
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="target"></param>
        /// <param name="spell"></param>
        /// <param name="spellLine"></param>
        /// <returns>true if allowed</returns>

        public override bool IsAllowedToCastSpell(GameLiving caster, GameLiving target, Spell spell, SpellLine spellLine)
          {
              if (m_pvpRegions != null)
              {
                  foreach (int reg in m_pvpRegions)
                      if (caster.CurrentRegionID == reg)
                      {
                          if (!base.IsAllowedToCastSpell(caster, target, spell, spellLine)) return false;

                          GamePlayer casterPlayer = caster as GamePlayer;
                          if (casterPlayer != null)
                          {
                              if (casterPlayer.IsInvulnerableToAttack)
                              {
                                  // always allow selftargeted spells
                                  if (spell.Target == "Self") return true;

                                  // only caster can be the target, can't buff/heal other players
                                  // PBAE/GTAE doesn't need a target so we check spell type as well
                                  if (caster != target || spell.Target == "Area" || spell.Target == "Enemy" || (spell.Target == "Group" && spell.SpellType != "SpeedEnhancement"))
                                  {
                                      MessageToLiving(caster, "You can only cast spells on yourself until your PvP invulnerability timer wears off!", eChatType.CT_Important);
                                      return false;
                                  }
                              }

                          }
                          return true;
                      }
              }
              return true;
          }
        

        public override bool IsSameRealm(GameLiving source, GameLiving target, bool quiet)
        {
            #region pve
            if (source == null || target == null)
                return false;

            // if controlled NPC - do checks for owner instead
            if (source is GameNPC)
            {
                IControlledBrain controlled = ((GameNPC)source).Brain as IControlledBrain;
                if (controlled != null)
                {
                    source = controlled.GetPlayerOwner();
                    quiet = true; // silence all attacks by controlled npc
                }
            }
            if (target is GameNPC)
            {
                IControlledBrain controlled = ((GameNPC)target).Brain as IControlledBrain;
                if (controlled != null)
                    target = controlled.GetPlayerOwner();
            }

            if (source == target)
                return true;

            // clients with priv level > 1 are considered friendly by anyone
            if (target is GamePlayer && ((GamePlayer)target).Client.Account.PrivLevel > 1) return true;

            // mobs can heal mobs, players heal players/NPC
            if (source.Realm == 0 && target.Realm == 0) return true;
            if (source.Realm != 0 && target.Realm != 0) return true;

            //Peace flag NPCs are same realm
            if (target is GameNPC)
                if ((((GameNPC)target).Flags & GameNPC.eFlags.PEACE) != 0)
                    return true;

            if (source is GameNPC)
                if ((((GameNPC)source).Flags & GameNPC.eFlags.PEACE) != 0)
                    return true;

            if (quiet == false) MessageToLiving(source, target.GetName(0, true) + " is not a member of your realm!");
            return false;
            #endregion
            #region rvr
            if (m_rvrRegions != null)
            {
                foreach (int reg in m_rvrRegions)
                    if (source.CurrentRegionID == reg)
                    {
                        if (source == null || target == null)
                            return false;

                        // if controlled NPC - do checks for owner instead
                        if (source is GameNPC)
                        {
                            IControlledBrain controlled = ((GameNPC)source).Brain as IControlledBrain;
                            if (controlled != null)
                            {
                                source = controlled.GetLivingOwner();
                                quiet = true; // silence all attacks by controlled npc
                            }
                        }
                        if (target is GameNPC)
                        {
                            IControlledBrain controlled = ((GameNPC)target).Brain as IControlledBrain;
                            if (controlled != null)
                                target = controlled.GetLivingOwner();
                        }

                        if (source == target)
                            return true;

                        // clients with priv level > 1 are considered friendly by anyone
                        if (target is GamePlayer && ((GamePlayer)target).Client.Account.PrivLevel > 1) return true;
                        // checking as a gm, targets are considered friendly
                        if (source is GamePlayer && ((GamePlayer)source).Client.Account.PrivLevel > 1) return true;

                        //Peace flag NPCs are same realm
                        if (target is GameNPC)
                            if ((((GameNPC)target).Flags & GameNPC.eFlags.PEACE) != 0)
                                return true;

                        if (source is GameNPC)
                            if ((((GameNPC)source).Flags & GameNPC.eFlags.PEACE) != 0)
                                return true;

                        if (source.Realm != target.Realm)
                        {
                            if (quiet == false) MessageToLiving(source, target.GetName(0, true) + " is not a member of your realm!");
                            return false;
                        }
                        return true;
                    }
                #endregion
                #region PvP

                if (m_pvpRegions != null)
                {
                    foreach (int reg in m_pvpRegions)
                        if (source.CurrentRegionID == reg)
                        {
                            if (source == null || target == null)
                                return false;

                            // if controlled NPC - do checks for owner instead
                            if (source is GameNPC)
                            {
                                IControlledBrain controlled = ((GameNPC)source).Brain as IControlledBrain;
                                if (controlled != null)
                                {
                                    source = controlled.GetLivingOwner();
                                    quiet = true; // silence all attacks by controlled npc
                                }
                            }
                            if (target is GameNPC)
                            {
                                IControlledBrain controlled = ((GameNPC)target).Brain as IControlledBrain;
                                if (controlled != null)
                                    target = controlled.GetLivingOwner();
                            }

                            if (source == target)
                                return true;

                            // clients with priv level > 1 are considered friendly by anyone
                            if (target is GamePlayer && ((GamePlayer)target).Client.Account.PrivLevel > 1) return true;
                            // checking as a gm, targets are considered friendly
                            if (source is GamePlayer && ((GamePlayer)source).Client.Account.PrivLevel > 1) return true;

                            // mobs can heal mobs, players heal players/NPC
                            if (source.Realm == 0 && target.Realm == 0) return true;

                            //keep guards
                            if (source is GameKeepGuard && target is GamePlayer)
                            {
                                if (!GameServer.KeepManager.IsEnemy(source as GameKeepGuard, target as GamePlayer))
                                    return true;
                            }

                            if (target is GameKeepGuard && source is GamePlayer)
                            {
                                if (!GameServer.KeepManager.IsEnemy(target as GameKeepGuard, source as GamePlayer))
                                    return true;
                            }

                            //doors need special handling
                            if (target is GameKeepDoor && source is GamePlayer)
                                return GameServer.KeepManager.IsEnemy(target as GameKeepDoor, source as GamePlayer);

                            if (source is GameKeepDoor && target is GamePlayer)
                                return GameServer.KeepManager.IsEnemy(source as GameKeepDoor, target as GamePlayer);

                            //components need special handling
                            if (target is GameKeepComponent && source is GamePlayer)
                                return GameServer.KeepManager.IsEnemy(target as GameKeepComponent, source as GamePlayer);

                            //Peace flag NPCs are same realm
                            if (target is GameNPC)
                                if ((((GameNPC)target).Flags & GameNPC.eFlags.PEACE) != 0)
                                    return true;

                            if (source is GameNPC)
                                if ((((GameNPC)source).Flags & GameNPC.eFlags.PEACE) != 0)
                                    return true;

                            if (source is GamePlayer && target is GamePlayer)
                                return true;

                            if (source is GamePlayer && target is GameNPC && target.Realm != 0)
                                return true;

                            if (quiet == false) MessageToLiving(source, target.GetName(0, true) + " is not a member of your realm!");
                            return false;
                        }

                }
            }

            #endregion
        }

        public override bool IsAllowedCharsInAllRealms(GameClient client)
        {
            return true;
        }

        public override bool IsAllowedToGroup(GamePlayer source, GamePlayer target, bool quiet)
        {
            if (source == null || target == null) return false;

            if (m_rvrRegions != null)
            {
                if (m_rvrRegions != null)
                {
                    foreach (int reg in m_rvrRegions)
                        if (source.CurrentRegionID == reg)
                        {
                            if (source.Realm != target.Realm)
                            {
                                if (quiet == false) MessageToLiving(source, "You can't invite a player of another realm.");
                                return false;
                            }
                        }
                }
                return true;
            }
            return true;
        }

        public override bool IsAllowedToJoinGuild(GamePlayer source, Guild guild)
        {
            return true;
        }

        public override bool IsAllowedToTrade(GameLiving source, GameLiving target, bool quiet)
        {
            if (source == null || target == null) return false;

            // clients with priv level > 1 are allowed to trade with anyone
            if (source is GamePlayer && target is GamePlayer)
            {
                if ((source as GamePlayer).Client.Account.PrivLevel > 1 || (target as GamePlayer).Client.Account.PrivLevel > 1)
                    return true;
            }

            //Peace flag NPCs can trade with everyone
            if (target is GameNPC)
                if ((((GameNPC)target).Flags & GameNPC.eFlags.PEACE) != 0)
                    return true;

            if (source is GameNPC)
                if ((((GameNPC)source).Flags & GameNPC.eFlags.PEACE) != 0)
                    return true;
            if (m_rvrRegions != null)
            {
                foreach (int reg in m_rvrRegions)
                    if (source.CurrentRegionID == reg)
                    {
                        if (source.Realm != target.Realm)
                        {
                            if (quiet == false) MessageToLiving(source, "You can't trade with enemy realm!");
                            return false;
                        }
                    }
            }
            return true;
        }

        public override bool IsAllowedToUnderstand(GameLiving source, GamePlayer target)
        {
            if (source == null || target == null) return false;

            // clients with priv level > 1 are allowed to talk and hear anyone
            if (source is GamePlayer && ((GamePlayer)source).Client.Account.PrivLevel > 1) return true;
            if (target.Client.Account.PrivLevel > 1) return true;

            //Peace flag NPCs can be understood by everyone

            if (source is GameNPC)
                if ((((GameNPC)source).Flags & GameNPC.eFlags.PEACE) != 0)
                    return true;
            if (m_rvrRegions != null)
            {
                foreach (int reg in m_rvrRegions)
                    if (source.CurrentRegionID == reg)
                    {
                        if (source.Realm > 0 && source.Realm != target.Realm) return false;
                    }
            }
            return true;
        }

        public override bool IsAllowedToBind(GamePlayer player, BindPoint point)
        {
            if (point.Realm == 0) return true;
            foreach (int reg in m_rvrRegions)
                if (player.CurrentRegionID == reg)
                {
                    return player.Realm == (eRealm)point.Realm;
                }
            return true;
        }

        public override byte GetColorHandling(GameClient client)
        {
            if (m_rvrRegions != null)
            {
                foreach (int reg in m_rvrRegions)
                    if (client.Player.CurrentRegionID == reg)
                    {
                        return 0;
                    }
            }
            if (m_pvpRegions != null)
            {
                foreach (int reg in m_pvpRegions)
                    if (client.Player.CurrentRegionID == reg)
                    {
                        return 1;
                    }
            }
            return 3;
        }

        public override string GetPlayerName(GamePlayer source, GamePlayer target)
        {
            if (m_rvrRegions != null)
            {
                foreach (int reg in m_rvrRegions)
                    if (source.CurrentRegionID == reg)
                    {
                        if (IsSameRealm(source, target, true))
                            return target.Name;
                        return source.RaceToTranslatedName(target.Race, target.Gender);
                    }
            }
            return target.Name;

        }

        public override string GetPlayerLastName(GamePlayer source, GamePlayer target)
        {
            if (m_rvrRegions != null)
            {
                foreach (int reg in m_rvrRegions)
                    if (source.CurrentRegionID == reg)
                    {
                        if (IsSameRealm(source, target, true))
                            return target.LastName;

                        return target.RealmRankTitle(source.Client.Account.Language);
                    }
            }
            return target.LastName;
        }

        public override string GetPlayerGuildName(GamePlayer source, GamePlayer target)
        {
            return target.GuildName;
        }

        public override string GetPlayerTitle(GamePlayer source, GamePlayer target)
        {
            return target.CurrentTitle.GetValue(source, target);
        }

        public override void ResetKeep(GuardLord lord, GameObject killer)
        {
            base.ResetKeep(lord, killer);
            lord.Component.AbstractKeep.Reset((eRealm)killer.Realm);
        }

        public override void OnPlayerLevelUp(GamePlayer player, int previousLevel)
        {
        }

        /// <summary>
        /// Gets the player's Total Amount of Realm Points Based on Level, Realm Level of other constraints.
        /// </summary>
        /// <param name="source">The player</param>
        /// <param name="target"></param>
        /// <returns>The total pool of realm points !</returns>
        public override int GetPlayerRealmPointsTotal(GamePlayer source)
        {
            return source.Level > 19 ? source.RealmLevel + (source.Level - 19) : source.RealmLevel;
        }

        public override IList<string> FormatPlayerStatistics(GamePlayer player)
        {
            var stat = new List<string>();

            int total = 0;
            #region Players Killed
            //only show if there is a kill [by Suncheck]
            if ((player.KillsAlbionPlayers + player.KillsMidgardPlayers + player.KillsHiberniaPlayers) > 0)
            {
                stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Kill.Title"));
                if (player.KillsAlbionPlayers > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Kill.AlbionPlayer") + ": " + player.KillsAlbionPlayers.ToString("N0"));
                if (player.KillsMidgardPlayers > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Kill.MidgardPlayer") + ": " + player.KillsMidgardPlayers.ToString("N0"));
                if (player.KillsHiberniaPlayers > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Kill.HiberniaPlayer") + ": " + player.KillsHiberniaPlayers.ToString("N0"));
                total = player.KillsMidgardPlayers + player.KillsAlbionPlayers + player.KillsHiberniaPlayers;
                stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Kill.TotalPlayers") + ": " + total.ToString("N0"));
            }
            #endregion
            stat.Add(" ");
            #region Players Deathblows
            //only show if there is a kill [by Suncheck]
            if ((player.KillsAlbionDeathBlows + player.KillsMidgardDeathBlows + player.KillsHiberniaDeathBlows) > 0)
            {
                total = 0;
                if (player.KillsAlbionDeathBlows > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Deathblows.AlbionPlayer") + ": " + player.KillsAlbionDeathBlows.ToString("N0"));
                if (player.KillsMidgardDeathBlows > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Deathblows.MidgardPlayer") + ": " + player.KillsMidgardDeathBlows.ToString("N0"));
                if (player.KillsHiberniaDeathBlows > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Deathblows.HiberniaPlayer") + ": " + player.KillsHiberniaDeathBlows.ToString("N0"));
                total = player.KillsMidgardDeathBlows + player.KillsAlbionDeathBlows + player.KillsHiberniaDeathBlows;
                stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Deathblows.TotalPlayers") + ": " + total.ToString("N0"));
            }
            #endregion
            stat.Add(" ");
            #region Players Solo Kills
            //only show if there is a kill [by Suncheck]
            if ((player.KillsAlbionSolo + player.KillsMidgardSolo + player.KillsHiberniaSolo) > 0)
            {
                total = 0;
                if (player.KillsAlbionSolo > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Solo.AlbionPlayer") + ": " + player.KillsAlbionSolo.ToString("N0"));
                if (player.KillsMidgardSolo > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Solo.MidgardPlayer") + ": " + player.KillsMidgardSolo.ToString("N0"));
                if (player.KillsHiberniaSolo > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Solo.HiberniaPlayer") + ": " + player.KillsHiberniaSolo.ToString("N0"));
                total = player.KillsMidgardSolo + player.KillsAlbionSolo + player.KillsHiberniaSolo;
                stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Solo.TotalPlayers") + ": " + total.ToString("N0"));
            }
            #endregion
            stat.Add(" ");
            #region Keeps
            //only show if there is a capture [by Suncheck]
            if ((player.CapturedKeeps + player.CapturedTowers) > 0)
            {
                stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Capture.Title"));
                //stat.Add("Relics Taken: " + player.RelicsTaken.ToString("N0"));
                //stat.Add("Albion Keeps Captured: " + player.CapturedAlbionKeeps.ToString("N0"));
                //stat.Add("Midgard Keeps Captured: " + player.CapturedMidgardKeeps.ToString("N0"));
                //stat.Add("Hibernia Keeps Captured: " + player.CapturedHiberniaKeeps.ToString("N0"));
                if (player.CapturedKeeps > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Capture.Keeps") + ": " + player.CapturedKeeps.ToString("N0"));
                //stat.Add("Keep Lords Slain: " + player.KeepLordsSlain.ToString("N0"));
                //stat.Add("Albion Towers Captured: " + player.CapturedAlbionTowers.ToString("N0"));
                //stat.Add("Midgard Towers Captured: " + player.CapturedMidgardTowers.ToString("N0"));
                //stat.Add("Hibernia Towers Captured: " + player.CapturedHiberniaTowers.ToString("N0"));
                if (player.CapturedTowers > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.Capture.Towers") + ": " + player.CapturedTowers.ToString("N0"));
                //stat.Add("Tower Captains Slain: " + player.TowerCaptainsSlain.ToString("N0"));
                //stat.Add("Realm Guard Kills Albion: " + player.RealmGuardTotalKills.ToString("N0"));
                //stat.Add("Realm Guard Kills Midgard: " + player.RealmGuardTotalKills.ToString("N0"));
                //stat.Add("Realm Guard Kills Hibernia: " + player.RealmGuardTotalKills.ToString("N0"));
                //stat.Add("Total Realm Guard Kills: " + player.RealmGuardTotalKills.ToString("N0"));
            }
            #endregion
            stat.Add(" ");
            #region PvE
            //only show if there is a kill [by Suncheck]
            if ((player.KillsDragon + player.KillsEpicBoss + player.KillsLegion) > 0)
            {
                stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.PvE.Title"));
                if (player.KillsDragon > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.PvE.KillsDragon") + ": " + player.KillsDragon.ToString("N0"));
                if (player.KillsEpicBoss > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.PvE.KillsEpic") + ": " + player.KillsEpicBoss.ToString("N0"));
                if (player.KillsLegion > 0) stat.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerStatistic.PvE.KillsLegion") + ": " + player.KillsLegion.ToString("N0"));
            }
            #endregion

            return stat;
        }

    }
}
