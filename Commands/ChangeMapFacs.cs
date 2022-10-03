using NetworkMessages.FromServer;
using System;
using TaleWorlds.MountAndBlade;


namespace ChatCommands.Commands
{
    class ChangeMapFacs : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            bool isAdmin = false;
            bool isExists = AdminManager.Admins.TryGetValue(networkPeer.VirtualPlayer.Id.ToString(), out isAdmin);
            return isExists && isAdmin;
        }

        public string Command()
        {
            return "!changemapfacs";
        }

        public string Description()
        {
            return "Changes the map and the team factions. !chagemapfacs <map id> <team1 faction> <team2 faction>";
        }

        bool ArgValid(Tuple<bool,string> args, NetworkCommunicator networkPeer, string messagePrefix="")
        {
            if(!args.Item1)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage(messagePrefix + args.Item2));
                GameNetwork.EndModuleEventAsServer();
                return false;
            }
            return true;
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            // Obligatory argument check
            if (args.Length != 3)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Invalid number of arguments"));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }

            // Validate the arguments
            string mapSearchString = args[0];
            Tuple<bool, string> mapSearchResult = AdminPanel.Instance.FindSingleMap(mapSearchString);
            if(!ArgValid(mapSearchResult,networkPeer))
            {
                return true;
            }

            string faction1SearchString = args[1];
            Tuple<bool, string> faction1SearchResult = AdminPanel.Instance.FindSingleFaction(faction1SearchString);
            if (!ArgValid(faction1SearchResult, networkPeer,"Faction1: "))
            {
                return true;
            }

            string faction2SearchString = args[2];
            Tuple<bool, string> faction2SearchResult = AdminPanel.Instance.FindSingleFaction(faction2SearchString);
            if (!ArgValid(faction2SearchResult, networkPeer, "Faction2: "))
            {
                return true;
            }

            // All arguments are good, change the map and the factions
            string mapName = mapSearchResult.Item2;
            string faction1 = faction1SearchResult.Item2;
            string faction2 = faction2SearchResult.Item2;

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerMessage("Changing map to " + mapName + " and factions to "+ faction1+" vs " +faction2));
            GameNetwork.EndModuleEventAsServer();

            AdminPanel.Instance.ChangeMapAndFactions(mapName, faction1, faction2);

            return true;
        }
    }
}