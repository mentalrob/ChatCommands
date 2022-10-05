using NetworkMessages.FromServer;
using System;
using TaleWorlds.MountAndBlade;


namespace ChatCommands.Commands
{
    class ChangeMission : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            bool isAdmin = false;
            bool isExists = AdminManager.Admins.TryGetValue(networkPeer.VirtualPlayer.Id.ToString(), out isAdmin);
            return isExists && isAdmin;
        }

        public string Command()
        {
            return "!mission";
        }

        public string Description()
        {
            return "Changes the game type, map, and factions. !mission <game type> <map id> <team1 faction> <team2 faction>";
        }

        bool ArgValid(Tuple<bool, string> args, NetworkCommunicator networkPeer, string messagePrefix = "")
        {
            if (!args.Item1)
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
            if (args.Length != 4)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Invalid number of arguments"));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }

            // Validate the arguments
            string gameTypeSearchString = args[0];
            Tuple<bool, string> gameTypeSearchResult = AdminPanel.Instance.FindSingleGameType(gameTypeSearchString);
            if (!ArgValid(gameTypeSearchResult, networkPeer))
            {
                return true;
            }

            string mapSearchString = args[1];
            Tuple<bool, string> mapSearchResult = AdminPanel.Instance.FindMapForGameType(gameTypeSearchResult.Item2,mapSearchString);
            if (!ArgValid(mapSearchResult, networkPeer))
            {
                return true;
            }

            string faction1SearchString = args[2];
            Tuple<bool, string> faction1SearchResult = AdminPanel.Instance.FindSingleFaction(faction1SearchString);
            if (!ArgValid(faction1SearchResult, networkPeer, "Faction1: "))
            {
                return true;
            }

            string faction2SearchString = args[3];
            Tuple<bool, string> faction2SearchResult = AdminPanel.Instance.FindSingleFaction(faction2SearchString);
            if (!ArgValid(faction2SearchResult, networkPeer, "Faction2: "))
            {
                return true;
            }

            // All arguments are good, change the map and the factions
            string gameType = gameTypeSearchResult.Item2;
            string mapName = mapSearchResult.Item2;
            string faction1 = faction1SearchResult.Item2;
            string faction2 = faction2SearchResult.Item2;

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerMessage("GameType: " + gameType));
            GameNetwork.EndModuleEventAsServer();

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerMessage("Map: " + mapName));
            GameNetwork.EndModuleEventAsServer();

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerMessage("Faction1: " + faction1));
            GameNetwork.EndModuleEventAsServer();

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerMessage("Faction2: " + faction2));
            GameNetwork.EndModuleEventAsServer();

            AdminPanel.Instance.ChangeGameTypeMapFactions(gameType, mapName, faction1, faction2);

            return true;
        }
    }
}