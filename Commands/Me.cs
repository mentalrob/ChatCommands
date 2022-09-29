using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.MountAndBlade;

namespace ChatCommands.Commands
{
    public class Me : Command
    {
        public string Command()
        {
            return "!me";
        }
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return true;
        }

        

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new ServerMessage("* "+ networkPeer.UserName + " " + string.Join(" ", args), false));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None, null);
            return true;
        }

        public string Description()
        {
            return "The me command that everyone knows, Usage !me <Emote>";
        }
    }
}
