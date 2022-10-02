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
using System.Threading;


namespace ChatCommands.Commands
{
    class ChangeMap : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            bool isAdmin = false;
            bool isExists = AdminManager.Admins.TryGetValue(networkPeer.VirtualPlayer.Id.ToString(), out isAdmin);
            return isExists && isAdmin;
        }

        public string Command()
        {
            return "!changemap";
        }

        public string Description()
        {
            return "Changes the map. Use !maps to see available map IDs. !chagemaps <partial map id>";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            // Obligatory argument check
            if (args.Length != 1)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Invalid number of arguments"));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }
             
            string searchString = args[0];
            Tuple<bool, string> searchResult = AdminPanel.Instance.FindSingleMap(searchString);

            if(searchResult.Item1)
            { 
                AdminPanel.Instance.ChangeMap(searchResult.Item2);
            }
            else
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage(searchResult.Item2));
                GameNetwork.EndModuleEventAsServer();
            }

            return true;
        }
    }
}