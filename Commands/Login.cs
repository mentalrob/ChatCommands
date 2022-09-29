using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace ChatCommands.Commands
{
    class Login : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return true;
        }

        public string Command()
        {
            return "!login";
        }

        public string Description()
        {
            return "Admin login. Usage !login <password>";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            if (args.Length == 0 || args.Length > 1) {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Please only provide a password. Usage: !login <password>"));
                GameNetwork.EndModuleEventAsServer();
            }
            String password = args[0];
            Config config = ConfigManager.GetConfig();
            if (!password.Equals(config.AdminPassword)) {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Given password is wrong."));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }
            AdminManager.Admins.Add(networkPeer.VirtualPlayer.Id.ToString(), true);
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerMessage("Login succed. Welcome admin"));
            GameNetwork.EndModuleEventAsServer();
            return true;
        }
    }
}
