using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Core;

namespace ChatCommands.AdminPanel
{
    public struct MissionData
    {
        public string gameType;
        public string mapId;
        public string cultureTeam1;
        public string cultureTeam2;
        public bool cultureVote;
        public bool mapVote;
    }

    class StartMissionThread
    {
        // To successfully change the map, this thread must be called when the mission has ended
        public static void ThreadProc(Object missionData)
        {
            // Give us some buffer between the OnMissionEnd event and starting the next mission
            Thread.Sleep(500);
            AdminPanel.Instance.StartMissionOnly((MissionData)missionData);
        }
    }

    class MissionListener : IMissionListener
    {
        MissionData missionData;

        public void setMissionData(MissionData missionData)
        {
            this.missionData = missionData;
        }

        public void OnConversationCharacterChanged()
        {

        }

        public void OnEndMission()
        {
            // Run a thread that will create a start a mission after a delay
            Thread t = new Thread(new ParameterizedThreadStart(ChangeMapThread.ThreadProc));
            t.Start(missionData);

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

    public class AdminPanel
    {

        // Singleton
        private static AdminPanel instance;
        public static AdminPanel Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new AdminPanel();
                }
                return instance;
            }
        }

        public bool MissionIsRunning
        {
            get
            {
                return Mission.Current != null;
            }
        }

        string GetOptionString(MultiplayerOptions.OptionType optionType) 
        {
            string toReturn;
            MultiplayerOptions.Instance.GetOptionFromOptionType(optionType).GetValue(out toReturn);
            return toReturn;
        }

        MissionData getStateOfMultiplayerOptions()
        {
            MissionData toReturn;

            toReturn.cultureVote = MultiplayerIntermissionVotingManager.Instance.IsCultureVoteEnabled;
            toReturn.mapVote = MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled;
            toReturn.cultureTeam1 = GetOptionString(MultiplayerOptions.OptionType.CultureTeam1);
            toReturn.cultureTeam2 = GetOptionString(MultiplayerOptions.OptionType.CultureTeam2);
            toReturn.mapId = GetOptionString(MultiplayerOptions.OptionType.Map);
            toReturn.gameType = GetOptionString(MultiplayerOptions.OptionType.GameType);

            return toReturn;
        }

        List<string> FindMaps(string searchString)
        {
            List<string> availableMaps = MultiplayerOptions.Instance.GetMultiplayerOptionsList(MultiplayerOptions.OptionType.Map);
            return availableMaps.Where(str => str.Contains(searchString)).ToList();
        }

        Tuple<bool,string> FindSingleMap(string searchString)
        {
            List<string> foundMaps = FindMaps(searchString);

            if(foundMaps.Count == 1)
            {
                return new Tuple<bool, string>(true, foundMaps[0]);
            }
            else if (foundMaps.Count > 2)
            {
                // Check for special case where the name of a map sits inside the name of another map ie mp_tdm_map_001 or mp_tdm_map_001_spring
                foreach (string mapName in foundMaps)
                {
                    if (mapName == searchString)
                    {
                        return new Tuple<bool, string>(true, mapName);
                    }
                }

                return new Tuple<bool,string>(false,"More than one map found matching '"+searchString+"'");
            }
            else
            {
                return new Tuple<bool, string>(false, "No maps found matching '" + searchString + "'");
            }
        }

        public bool ChangeMap(string searchString)
        {
            Tuple<bool, string> searchResults = FindSingleMap(searchString);

            // Map was found, changing map
            if(searchResults.Item1)
            {
                MissionData currentState = getStateOfMultiplayerOptions();
                currentState.mapId = searchResults.Item2;
                StartMission(currentState);
                return true;
            }

            return false;
        }

        public void SetMultiplayerOptions(MissionData missionData, MultiplayerOptions.MultiplayerOptionsAccessMode opetionSet = MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions)
        {
            MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.GameType, opetionSet).UpdateValue(missionData.gameType);
            MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.Map, opetionSet).UpdateValue(missionData.mapId);
            MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.CultureTeam1, opetionSet).UpdateValue(missionData.cultureTeam1);
            MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.CultureTeam2, opetionSet).UpdateValue(missionData.cultureTeam2);
            MultiplayerIntermissionVotingManager.Instance.IsCultureVoteEnabled = missionData.cultureVote;
            MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled = missionData.mapVote;
        }



        public void StartMission(MissionData missionData)
        {

            SetMultiplayerOptions(missionData);

            if (!MissionIsRunning)
            {
                DedicatedCustomServerSubModule.Instance.StartMission();
            }
            else
            {
                MissionListener listener = new MissionListener();
                Mission.Current.AddListener(listener);

                MultiplayerIntermissionVotingManager.Instance.IsCultureVoteEnabled = false;
                MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled = false;

                listener.setMissionData(missionData);
                DedicatedCustomServerSubModule.Instance.EndMission();
            }
        }

        public bool StartMissionOnly(MissionData missionData)
        {
            SetMultiplayerOptions(missionData);

            if (!MissionIsRunning)
            {
                DedicatedCustomServerSubModule.Instance.StartMission();
                return true;
            }

            return false;
        }

        public bool EndMission()
        {
            if (MissionIsRunning)
            {
                DedicatedCustomServerSubModule.Instance.StartMission();
            }

            return false;
        }

    }
}
