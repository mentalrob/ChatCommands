using NetworkMessages.FromServer;
using TaleWorlds.MountAndBlade;

namespace ChatCommands.Commands
{
    class HealAll : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            bool isAdmin = false;
            bool isExists = AdminManager.Admins.TryGetValue(networkPeer.VirtualPlayer.Id.ToString(), out isAdmin);
            return isExists && isAdmin;
        }

        public string Command()
        {
            return "!healall";
        }

        public string Description()
        {
            return "Healing all players.";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
            {
                if (peer.ControlledAgent != null)
                {
                    peer.ControlledAgent.Health = peer.ControlledAgent.HealthLimit;
                    GameNetwork.BeginModuleEventAsServer(peer);
                    GameNetwork.WriteMessage(new ServerMessage("Players are heal"));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
            return true;
        }
    }
}
