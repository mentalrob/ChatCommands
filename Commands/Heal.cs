using NetworkMessages.FromServer;
using TaleWorlds.MountAndBlade;

namespace ChatCommands.Commands
{
    class Heal : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            bool isAdmin = false;
            bool isExists = AdminManager.Admins.TryGetValue(networkPeer.VirtualPlayer.Id.ToString(), out isAdmin);
            return isExists && isAdmin;
        }

        public string Command()
        {
            return "!heal";
        }

        public string Description()
        {
            return "Healing player. Usage !heal <Player Name>";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            if (args.Length == 0)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Please provide a username. Player that contains provided input will be heal."));
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

            if (networkPeer.ControlledAgent != null && targetPeer.ControlledAgent != null)
            {
                targetPeer.ControlledAgent.Health = targetPeer.ControlledAgent.HealthLimit;
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Player " + targetPeer.UserName + " is heal"));
                GameNetwork.EndModuleEventAsServer();
                GameNetwork.BeginModuleEventAsServer(targetPeer);
                GameNetwork.WriteMessage(new ServerMessage("Player " + networkPeer.UserName + " healed you"));
                GameNetwork.EndModuleEventAsServer();
            }
            return true;
        }
    }
}