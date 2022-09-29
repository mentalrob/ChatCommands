using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace ChatCommands.Commands
{
    class GodMode : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            bool isAdmin = false;
            bool isExists = AdminManager.Admins.TryGetValue(networkPeer.VirtualPlayer.Id.ToString(), out isAdmin);
            return isExists && isAdmin;
        }

        public string Command()
        {
            return "!godmode";
        }

        public string Description()
        {
            return "Ascend yourself. Be something selestial";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            if (networkPeer.ControlledAgent != null) {
                networkPeer.ControlledAgent.BaseHealthLimit = 2000;
                networkPeer.ControlledAgent.HealthLimit = 2000;
                networkPeer.ControlledAgent.Health = 2000;
                networkPeer.ControlledAgent.SetMinimumSpeed(10);
                networkPeer.ControlledAgent.SetMaximumSpeedLimit(10, false);
                
            }
            return true;
        }
    }
}
