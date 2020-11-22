//Written by Sirru
/*
 * 
 * Edited by BluRaven 5-22-07
 * Added a check for Realm Rank, which boots the player if they are over
 * RR7 when the mob dies.  Added a check for how many players are online,
 * if you want to boot the player out of the farm zone if there are a
 * certain number of players online.  Also added a base ammount based on
 * the mobs level, plus there is a 5% chance for a jackpot which will
 * reward either 2x or 3x the ammount to the player.  Also added screen
 * center messages.  Also added a division of the reward based on the
 * number of players in the group.  Also added a check for if the player
 * is actually in the farm zone before it gives the reward (you must change
 * the farm zone region number to match yours, currently it's set to Darkness Falls.
 * -Blu
 * 5-22-07
 * 
 * 
 */

using System;
using System.IO;
using System.Collections;
using System.Reflection;
using DOL.Language;
using DOL.GS;
using DOL.GS.ServerProperties;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
/// <summary>
/// Represents an in-game GameHealer NPC
/// </summary>
public class RPMob : GameNPC
{

    public override void Die(GameObject killer)

    {

        GamePlayer player = killer as GamePlayer;
        int baserp = 0;
		if (Level >= 1 && Level <=10) { baserp = 10; }
		if (Level >= 11 && Level <=19) { baserp = 20; }
		if (Level >= 20 && Level <=29) { baserp = 30; }
		if (Level >= 30 && Level <=39) { baserp = 40; }
		if (Level >= 40 && Level <=44) { baserp = 50; }
        if (Level == 45) { baserp = 55; }
        if (Level == 46) { baserp = 60; }
        if (Level == 47) { baserp = 65; }
        if (Level == 48) { baserp = 70; }
        if (Level == 49) { baserp = 75; }
        if (Level == 50) { baserp = 80; }
        if (Level == 51) { baserp = 85; }
        if (Level == 52) { baserp = 90; }
        if (Level == 53) { baserp = 95; }
        if (Level == 54) { baserp = 100; }
        if (Level == 55) { baserp = 110; }
        if (Level >= 56) { baserp = 120; }
        if (Level >= 59) { baserp = 130; }

        int rewardrp;
        bool isjackpot;

        int multiplier = Util.Random(2, 5);
        int bonus = Util.Random(1, 3);
        int chance = Util.Random(1, 50);
        
        if (chance >= 40)
        {
            isjackpot = true;
        }
        else
        {
            isjackpot = false;
        }
        if (isjackpot)
        {
            rewardrp = ((baserp + bonus) * multiplier);
        }
        else
        {
            rewardrp = (baserp + bonus);
        }
       
        if(player is GamePlayer && IsWorthReward)

        {

            if (player.Group != null)
            {

                if (player.Group.MemberCount  == 1) { rewardrp = (rewardrp); }
                if (player.Group.MemberCount  == 2) { rewardrp = (rewardrp / 2); }
                if (player.Group.MemberCount  == 3) { rewardrp = (rewardrp / 3); }
                if (player.Group.MemberCount  == 4) { rewardrp = (rewardrp / 4); }
                if (player.Group.MemberCount  == 5) { rewardrp = (rewardrp / 5); }
                if (player.Group.MemberCount  == 6) { rewardrp = (rewardrp / 6); }
                if (player.Group.MemberCount  == 7) { rewardrp = (rewardrp / 7); }
                if (player.Group.MemberCount  >= 8) { rewardrp = (rewardrp / 8); }
                              
                foreach (GamePlayer player2 in player.Group.GetMembersInTheGroup())
                {

                    if (player2.RealmPoints >= 1755250)
                    {
                        player2.Out.SendMessage("Hero, You are RR7 or higher, you will not be rewarded here anymore!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        player2.MoveTo(79, 32401, 12245, 17413, 1902);

                    }
                    
					if (player2.Client.Account.PrivLevel == 1) { player2.RealmPoints += rewardrp; }
                    if (isjackpot) { player2.Out.SendMessage("JACKPOT!!!", eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow); player2.Out.SendPlaySound(eSoundType.Craft, 0x04); player2.Out.SendMessage("You just got " + multiplier + "x multiplier bonus points!", eChatType.CT_ScreenCenterSmaller, eChatLoc.CL_SystemWindow); }
                    player2.Out.SendMessage("Hero, You Get " + rewardrp + " realm points!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    player2.Out.SendMessage("You Get " + rewardrp + " realm points!", eChatType.CT_ScreenCenterSmaller, eChatLoc.CL_SystemWindow);
					player2.Out.SendUpdatePlayer();

                }


            }

            else
            {
                if (player.RealmPoints >= 1755250)
                {
                    player.Out.SendMessage("Hero, You are RR7 or higher, you will not be rewarded here anymore!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    player.MoveTo(79, 32401, 12245, 17413, 1902);

                }               
                    if (player.Client.Account.PrivLevel == 1) { player.RealmPoints += rewardrp; }
                    if (isjackpot) { player.Out.SendMessage("JACKPOT!!!", eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow); player.Out.SendPlaySound(eSoundType.Craft, 0x04); player.Out.SendMessage("You just got " + multiplier + "x multiplier bonus points!", eChatType.CT_ScreenCenterSmaller, eChatLoc.CL_SystemWindow); }
                    player.Out.SendMessage("Hero, You Get " + rewardrp + " realm points!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    player.Out.SendMessage("You Get " + rewardrp + " realm points!", eChatType.CT_ScreenCenterSmaller, eChatLoc.CL_SystemWindow);
					player.Out.SendUpdatePlayer();
            }

            //DropLoot(killer);

        }

        base.Die(killer);

        if ((Faction != null) && (killer is GamePlayer))

        {

            GamePlayer player3 = killer as GamePlayer;

            Faction.KillMember(player3);

        }

        StartRespawn();

    }

}

}