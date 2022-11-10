using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;

namespace ChatCommands.Commands
{
    class Ban : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            bool isAdmin = false;
            bool isExists = AdminManager.Admins.TryGetValue(networkPeer.VirtualPlayer.Id.ToString(), out isAdmin);
            return isExists && isAdmin;
        }

        public string Command()
        {
            return "!ban";
        }

        public string Description()
        {
            return "Bans a player. Caution ! First user that contains the provided input will be banned. Usage !ban <Player Name>";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            if (args.Length == 0) {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Please provide a username. Player that contains provided input will be banned"));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }

            NetworkCommunicator targetPeer = null;
            foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers) {
                if(peer.UserName.Contains(string.Join(" ", args))) {
                    targetPeer = peer;
                    break;
                }
            }
            if (targetPeer == null) {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Target player not found"));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }
            
            using (StreamWriter sw = File.AppendText(BanManager.BanListPath()))
            {
                sw.WriteLine(targetPeer.UserName + "|" + targetPeer.VirtualPlayer.Id.ToString());
            }

            foreach (NetworkCommunicator peer2 in GameNetwork.NetworkPeers)
            {
                if (peer2.ControlledAgent != null)
                {
                    GameNetwork.BeginModuleEventAsServer(peer2);
                    GameNetwork.WriteMessage(new ServerMessage("Player " + targetPeer.UserName + " is banned from the server"));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
            DedicatedCustomServerSubModule.Instance.DedicatedCustomGameServer.KickPlayer(targetPeer.VirtualPlayer.Id, false);
            return true;
            // throw new NotImplementedException();
        }
    }
}
