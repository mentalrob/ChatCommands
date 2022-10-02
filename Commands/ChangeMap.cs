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

struct MapChangeData
{
    public string mapId;
    public string cultureTeam1;
    public string cultureTeam2;
    public bool cultureVote;
    public bool mapVote;
}

class ChangeMapThread
{
    // To successfully change the map, this thread must be called when the mission has ended
    public static void ThreadProc(Object mapChangeData)
    {
        // Give us some buffer between the OnMissionEnd event and starting the next mission
        Thread.Sleep(500);

        // Change the data and start the server
        MapData.Change((MapChangeData)mapChangeData);
        DedicatedCustomServerSubModule.Instance.StartMission();
    }
}

class MissionListener : IMissionListener
{
    MapChangeData mapChangeData;

    public void setMapChangeData(MapChangeData mcd)
    {
        mapChangeData = mcd;
    }

    public void OnConversationCharacterChanged()
    {

    }

    public void OnEndMission()
    {
        // Run a thread that will create a start a mission after a delay
        Thread t = new Thread(new ParameterizedThreadStart(ChangeMapThread.ThreadProc));
        t.Start(mapChangeData);

        Debug.Print("Removing Mission Listener", 0, Debug.DebugColor.Yellow);
        Mission.Current.RemoveListener(this);
    }

    public void OnEquipItemsFromSpawnEquipment(Agent agent, Agent.CreationType creationType)
    {

    }

    public void OnEquipItemsFromSpawnEquipmentBegin(Agent agent, Agent.CreationType creationType)
    {

    }

    public void OnInitialDeploymentPlanMade(BattleSideEnum battleSide, bool isFirstPlan)
    {

    }

    public void OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
    {

    }

    public void OnResetMission()
    {

    }
}

//TODO: This probably needs to be more globally accessible 
static class MapData
{
    public static void Change(MapChangeData mcd)
    {
        MapChangeData dataToChange = (MapChangeData)mcd;

        // Update MultipalyerOptions and then start the mission
        MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.Map, MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions).UpdateValue(dataToChange.mapId);
        MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.CultureTeam1, MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions).UpdateValue(dataToChange.cultureTeam1);
        MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.CultureTeam2, MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions).UpdateValue(dataToChange.cultureTeam2);
        MultiplayerIntermissionVotingManager.Instance.IsCultureVoteEnabled = dataToChange.cultureVote;
        MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled = dataToChange.mapVote;
    }
}

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
            List<string> availableMaps = MultiplayerOptions.Instance.GetMultiplayerOptionsList(MultiplayerOptions.OptionType.Map);
            List<string> foundMaps = availableMaps.Where(str => str.Contains(searchString)).ToList();

            // Check for special case where the name of a map sits inside the name of another map ie mp_tdm_map_001 + mp_tdm_map_001_spring
            foreach(string mapName in foundMaps)
            {
                if(mapName == searchString)
                {
                    foundMaps.Clear();
                    foundMaps.Add(mapName);
                    break;
                }
            }

            if (foundMaps.Count > 2)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("A total of "+foundMaps.Count+" maps matched '"+ searchString + "':"));
                GameNetwork.EndModuleEventAsServer();

                foreach (var map in foundMaps)
                {
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new ServerMessage(map));
                    GameNetwork.EndModuleEventAsServer();
                }

                return true;
            }
            else if(foundMaps.Count == 0)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("No maps found matching '"+ searchString+"'"));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }

            // We need to save the state before ending the mission in case enable_automated_battle_switching is turned on
            MapChangeData mapChangeData;
            mapChangeData.mapId = foundMaps[0];

            string cultureTeam1 = "";
            MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.CultureTeam1).GetValue(out cultureTeam1);
            mapChangeData.cultureTeam1 = cultureTeam1;

            string cultureTeam2 = "";
            MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.CultureTeam2).GetValue(out cultureTeam2);
            mapChangeData.cultureTeam2 = cultureTeam2;

            mapChangeData.cultureVote = MultiplayerIntermissionVotingManager.Instance.IsCultureVoteEnabled;
            mapChangeData.mapVote = MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled;

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerMessage("Changing map to " + foundMaps[0] + "."));
            GameNetwork.EndModuleEventAsServer();

            // Check if a mission is active
            if (Mission.Current != null)
            {
                // Because a mission is running, we need to wait for it to be over before we can start a new mission
                // Add a listener so we can register to the OnMissionEnd event
                MissionListener listener = new MissionListener();

                Debug.Print("Adding Mission Listener", 0, Debug.DebugColor.Yellow);
                Mission.Current.AddListener(listener);

                // Set the data for the change
                listener.setMapChangeData(mapChangeData);

                // Prevent enable_automated_battle_switching from interfering
                MultiplayerIntermissionVotingManager.Instance.IsCultureVoteEnabled = false;
                MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled = false;

                // End the mission
                DedicatedCustomServerSubModule.Instance.EndMission();
            }
            else
            {
                // No mission is running, therefore we can start a mission immediately
                MapData.Change(mapChangeData);
                DedicatedCustomServerSubModule.Instance.StartMission();
                
            }

            return true;
        }
    }
}