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
using System.Collections.Generic;
using System.Linq;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS;

namespace DOL.GS.Commands
{
    [Cmd(
		"&port",
        ePrivLevel.GM,
        "Modify or use the jumppoint system",
        //usage
        "/port add <name>",
        "/port list",
        "/port to <name>",
        "/port remove <name>")]
    public class OnJumpPoint : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
        	if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}
        	switch(args[1])
            {
                case "add":
            		AddJumpPoint(client, args); break;
                case "list":
                    ListJumpPoints(client); break;
                case "to":
                    PortToJumpPoint(client,args); break;
                case "remove":
                    RemoveJumpPoint(client, args); break;
                case "update":
                    UpdateJumpPoints(); break;
                default:
                    DisplaySyntax(client);
                    break;
            }
        }

        private void AddJumpPoint(GameClient client, string[] args)
        {
            if (args.Length != 3)
            {
                client.Out.SendMessage("Usage : /port add <name>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            
            DBJumpPoint p = GameServer.Database.SelectObjects<DBJumpPoint>("Name = @Name", new QueryParameter("@Name", args[2])).FirstOrDefault();
            
            if(p != null)
            {
                SendSystemMessage(client, "JumpPoint with name '" + args[2] + "' already exists");
                return;
            }
            
            p = new DBJumpPoint();
            p.Xpos = client.Player.X;
            p.Ypos = client.Player.Y;
            p.Zpos = client.Player.Z;
            p.Region = client.Player.CurrentRegionID;
            p.Heading = client.Player.Heading;
            p.Name = args[2];

            GameServer.Database.AddObject(p);
            
            SendSystemMessage(client,"JumpPoint added with name '" + args[2] + "'");            
        }

        private void ListJumpPoints(GameClient client)
        {
            var col = GameServer.Database.SelectAllObjects<DBJumpPoint>();
            col = col.OrderBy(x => x.ZoneName).ThenBy(x => x.Name).ToList();

            SendSystemMessage(client, "----------List of JumpPoints----------");

            foreach (DBJumpPoint p in col)
            {
                string zoneAndName = p.ZoneName + ": " + p.Name;
                SendSystemMessage(client, zoneAndName);
            }
        }

        private void RemoveJumpPoint(GameClient client, string[] args)
        {
            if (args.Length != 3)
            {
                SendSystemMessage(client, "Usage : /port remove <name>");
                return;
            }

            DBJumpPoint p = GameServer.Database.SelectObjects<DBJumpPoint>("Name = @Name", new QueryParameter("@Name", args[2])).FirstOrDefault();;

            if(p == null)
            {
                SendSystemMessage(client, "No JumpPoint with name '" + args[2] + "' found");
                return;
            }
            
            GameServer.Database.DeleteObject(p);
            SendSystemMessage(client, "Removed JumpPoint with name '" + args[2] + "'");            
        }

        private void PortToJumpPoint(GameClient client, string[] args)
        {
            if (args.Length == 3 && args[1] == "to")
            {
                DBJumpPoint p = GameServer.Database.SelectObjects<DBJumpPoint>("Name = @Name", new QueryParameter("@Name", args[2])).FirstOrDefault();

                if(p == null)
                {
                    SendSystemMessage(client, "No JumpPoint with name '" + args[2] + "' found");
                    return;
                }
                if (CheckExpansion(client, client, p.Region))
                {
                	client.Player.MoveTo(p.Region, p.Xpos, p.Ypos, p.Zpos, p.Heading);
                }                
            }            
            else
            {
                SendSystemMessage(client, "Usage : /port to <name>");
                return;
            }
        }

        private void SendSystemMessage(GameClient client, string msg)
        {
            client.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }

        private bool CheckExpansion(GameClient clientJumper, GameClient clientJumpee, ushort RegionID)
        {
            Region reg = WorldMgr.GetRegion(RegionID);
            
            if (reg != null && reg.Expansion > (int)clientJumpee.ClientType)
            {
                clientJumper.Out.SendMessage(clientJumpee.Player.Name + " cannot jump to Destination region (" + reg.Description + ") because it is not supported by your client type.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }
            return true;
        }

        // intended as a one time use to update existing entries on your 'jumppoint' table
        private void UpdateJumpPoints()
        {
            var col = GameServer.Database.SelectAllObjects<DBJumpPoint>();

            foreach (DBJumpPoint p in col)
            {
                p.ZoneName = WorldMgr.GetRegion(p.Region).GetZone(p.Xpos, p.Ypos).Description;
                GameServer.Database.SaveObject(p);
            }
        }
    }
}
