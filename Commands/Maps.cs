using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;



namespace ChatCommands.Commands
{

    class Maps : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            bool isAdmin = false;
            bool isExists = AdminManager.Admins.TryGetValue(networkPeer.VirtualPlayer.Id.ToString(), out isAdmin);
            return isExists && isAdmin;
        }

        public string Command()
        {
            return "!maps";
        }

        public string Description()
        {
            return "Lists available maps. !maps";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            List<string> availableMaps = AdminPanel.Instance.GetAllAvailableMaps();

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerMessage("Maps: "));
            GameNetwork.EndModuleEventAsServer();

            foreach (var map in availableMaps)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage(map));
                GameNetwork.EndModuleEventAsServer();
            }

            string currentMapId = "";
            MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.Map).GetValue(out currentMapId);

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerMessage("Current Map: "+currentMapId));
            GameNetwork.EndModuleEventAsServer();

            return true;
        }
    }
}
