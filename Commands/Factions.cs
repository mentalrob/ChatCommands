using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;

namespace ChatCommands.Commands
{

    class Factions : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            bool isAdmin = false;
            bool isExists = AdminManager.Admins.TryGetValue(networkPeer.VirtualPlayer.Id.ToString(), out isAdmin);
            return isExists && isAdmin;
        }

        public string Command()
        {
            return "!factions";
        }

        public string Description()
        {
            return "Lists available factions. !factions";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            List<string> availableFactions = AdminPanel.Instance.GetAllFactions();

            if(args.Length > 0)
            {
                Tuple<bool,string> found = AdminPanel.Instance.FindSingleFaction(args[0]);
                if(found.Item1)
                {
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new ServerMessage(found.Item2));
                    GameNetwork.EndModuleEventAsServer();
                }
                else
                {
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new ServerMessage("No faction found"));
                    GameNetwork.EndModuleEventAsServer();
                }
                
            }

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerMessage("Factions: "));
            GameNetwork.EndModuleEventAsServer();

            foreach (var faction in availableFactions)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage(faction));
                GameNetwork.EndModuleEventAsServer();
            }

            string team1Faction = "";
            MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.CultureTeam1).GetValue(out team1Faction);

            string team2Faction = "";
            MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.CultureTeam2).GetValue(out team2Faction);

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerMessage("Current Factions: " + team1Faction +" "+team2Faction));
            GameNetwork.EndModuleEventAsServer();

            return true;
        }
    }
}
