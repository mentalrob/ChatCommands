using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;

namespace ChatCommands.Commands
{
    class Kick : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            bool isAdmin = false;
            bool isExists = AdminManager.Admins.TryGetValue(networkPeer.VirtualPlayer.Id.ToString(), out isAdmin);
            return isExists && isAdmin;
        }

        public string Command()
        {
            return "!kick";
        }

        public string Description()
        {
            return "Kicks a player. Caution ! First user that contains the provided input will be kicked. Usage !kick <Player Name>";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            if (args.Length == 0)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Please provide a username. Player that contains provided input will be kicked."));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }

            NetworkCommunicator targetPeer = null;
            foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
            {
                if (peer.UserName.Contains(string.Join(" ", args)))
                {
                    targetPeer = peer;
                    break;
                }
            }
            if (targetPeer == null)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Target player not found"));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }

            foreach (NetworkCommunicator peer2 in GameNetwork.NetworkPeers)
            {
                if (peer2.ControlledAgent != null)
                {
                    GameNetwork.BeginModuleEventAsServer(peer2);
                    GameNetwork.WriteMessage(new ServerMessage("Player " + targetPeer.UserName + " is kicked from the server"));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
            DedicatedCustomServerSubModule.Instance.DedicatedCustomGameServer.KickPlayer(targetPeer.VirtualPlayer.Id, false);
            return true;
        }
    }
}

