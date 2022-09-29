using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace ChatCommands.Commands
{
    class Credits : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return true;
        }

        public string Command()
        {
            return "!credits";
        }

        public string Description()
        {
            return "Who made this mod ?";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerMessage("Mentalrob's Chat Commands By Errayn With contributions of Alverrt"));
            GameNetwork.EndModuleEventAsServer();
            return true;
        }
    }
}
