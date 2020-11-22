using System;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using DOL.Events;
using DOL.Database;
using DOL.Database.Attributes;
using DOL.AI.Brain;
using DOL.GS.SkillHandler;
using DOL.GS;
using DOL.GS.Scripts;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using DOL.GS.Effects;

using log4net;

namespace DOL.GS.Scripts
{
    public class BGPorter : GameNPC
    {

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override bool AddToWorld()
        {
            Name = "Battlegrounds";
			GuildName = "Teleporter";
            Model = 184;
            Size = 50;
            Level = 50;
            Flags |= eFlags.PEACE;
			
			return base.AddToWorld();
        }
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player)) return false;
            TurnTo(player.X, player.Y);
            player.Out.SendMessage("Hello " + player.Name + ", You can currently be translocated to the [BG Zone].  Number of Players Currently In the BG Zone = " + WorldMgr.GetClientsOfRegionCount(241) + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

            return true;
        }



        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str)) return false;
            if (!(source is GamePlayer)) return false;
            GamePlayer t = (GamePlayer)source;
            TurnTo(t.X, t.Y);

            switch (str)
            {



                #region BG Zone

                case "BG Zone":
				if (!t.InCombat)
                {
					Say("I will send you to the appropriate Battleground for your level, Good Luck.");
                    foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    player.Out.SendSpellCastAnimation(this, 4953, 6);
                         
						if (t.Level < 5)
						{
                        // Tell them oops
                        Say("You must be level 5 before you can join Battlegrounds.");
                        }
												
						if (t.Realm == eRealm.Albion && t.Level > 4 && t.Level < 50)
                        {
                        // Move to Molvik area 241
                        t.MoveTo(241, 531997, 541272, 5992, 4031);
                        }
						
						if (t.Realm == eRealm.Midgard && t.Level > 4 && t.Level < 50)
                        {
                        // Move to Molvik area 241
                        t.MoveTo(241, 549468, 577418, 5992, 2552);
                        }
						
						if (t.Realm == eRealm.Hibernia && t.Level > 4 && t.Level < 50)
                        {
                        // Move to Molvik area 241
                        t.MoveTo(241, 576254, 544246, 5992, 1462);
                        }
						
                        if (t.Level > 49)
                        {
                        // Tell them oops
                        Say("Those who have reached their 50th season use the Frontiers as their Battlegrounds.");
                        }
                        // Nothing else to check for

                        }
						else { t.Client.Out.SendMessage("You must use your realms Teleporter for BGs.", eChatType.CT_Say, eChatLoc.CL_PopupWindow); }
                    break;
                #endregion BG Zone

                default: break;
            }
            return true;





        }
        private void SendReply(GamePlayer target, string msg)
        {
            target.Client.Out.SendMessage(
                msg,
                eChatType.CT_Say, eChatLoc.CL_PopupWindow);
        }
       
    }

}