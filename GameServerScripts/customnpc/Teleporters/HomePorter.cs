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
    public class HomePorter : GameNPC
    {

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override bool AddToWorld()
        {
            Name = "Go Home";
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
            player.Out.SendMessage("Hello " + player.Name + ", I can send you to your realms beginner town.[Home]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

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



                #region Home

                case "Home":
				if (!t.InCombat)
                {
					Say("Good Luck!");
                    foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    player.Out.SendSpellCastAnimation(this, 4953, 6);
                         
                											
						if (t.Realm == eRealm.Albion)
                        {                        
                        t.MoveTo(1, 559613, 511843, 2289, 3200);
                        }	
						
						if (t.Realm == eRealm.Midgard)
                        {                    
                        t.MoveTo(100, 804292, 726509, 4696, 842);
                        }
						
						if (t.Realm == eRealm.Hibernia)
                        {
                        t.MoveTo(200, 348073, 489646, 5200, 643);
                        }	
						
                        // Nothing else to check for
                        }
						else { t.Client.Out.SendMessage("You must not be in combat!", eChatType.CT_Say, eChatLoc.CL_PopupWindow); }
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