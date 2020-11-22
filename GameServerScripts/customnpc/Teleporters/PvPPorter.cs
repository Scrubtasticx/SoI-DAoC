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
    public class PvPPorter : GameNPC
    {

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override bool AddToWorld()
        {
            Name = "PvP Zone";
			GuildName = "Teleporter";
            Model = 402;
            Size = 37;
            Level = 50;
            Flags |= eFlags.PEACE;
			
			return base.AddToWorld();
        }
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player)) return false;
            TurnTo(player.X, player.Y);
            player.Out.SendMessage("Hello " + player.Name + ", You can currently be translocated to the [PvP Zone].  Number of Players Currently In the PvP Zone = " + WorldMgr.GetClientsOfRegionCount(51) + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

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



                #region PvP Zone

                case "PvP Zone":
				if (!t.InCombat)
                    {                                     
                            t.MoveTo(341, 31575, 40154, 15999, 35);             
					}
					
					else { t.Client.Out.SendMessage("You can't port while in combat.", eChatType.CT_Say, eChatLoc.CL_PopupWindow); }
                    break;
                #endregion PvP Zone

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