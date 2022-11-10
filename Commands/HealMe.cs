using TaleWorlds.MountAndBlade;

namespace ChatCommands.Commands
{
    class HealMe : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            bool isAdmin = false;
            bool isExists = AdminManager.Admins.TryGetValue(networkPeer.VirtualPlayer.Id.ToString(), out isAdmin);
            return isExists && isAdmin;
        }

        public string Command()
        {
            return "!healme";
        }

        public string Description()
        {
            return "Healing yourself.";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            if (networkPeer.ControlledAgent != null)
            {
                networkPeer.ControlledAgent.Health = networkPeer.ControlledAgent.HealthLimit;

            }
            return true;
        }
    }
}
