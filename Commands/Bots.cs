using NetworkMessages.FromServer;
using System;
using TaleWorlds.MountAndBlade;


namespace ChatCommands.Commands
{
    // To Test:
    // First/Second arg is not a number
    // First/Second Command less than 0

    class Bots : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            bool isAdmin = false;
            bool isExists = AdminManager.Admins.TryGetValue(networkPeer.VirtualPlayer.Id.ToString(), out isAdmin);
            return isExists && isAdmin;
        }

        public string Command()
        {
            return "!bots";
        }

        public string Description()
        {
            return "Changes the number of bots. !bots <num bots team1> <num bots team2>";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            // Obligatory argument check
            if (args.Length != 2)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Invalid number of arguments"));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }
            int numBotsTeam1 = -1;
            if (!Int32.TryParse(args[0], out numBotsTeam1) || numBotsTeam1 < 0)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("First argument is not a positive number"));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }

            int numBotsTeam2 = -1;
            if (!Int32.TryParse(args[1], out numBotsTeam2) || numBotsTeam2 < 0)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Second argument is not a positive number"));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }

            AdminPanel.Instance.SetBots(numBotsTeam1, numBotsTeam2);

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerMessage("Team1 Bots: "+numBotsTeam1.ToString()));
            GameNetwork.EndModuleEventAsServer();

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerMessage("Team2 Bots: " + numBotsTeam2.ToString()));
            GameNetwork.EndModuleEventAsServer();

            return true;
        }
    }
}