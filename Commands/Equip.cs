using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.ObjectSystem;

namespace ChatCommands.Commands
{
    class Equip : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            bool isAdmin = false;
            bool isExists = AdminManager.Admins.TryGetValue(networkPeer.VirtualPlayer.Id.ToString(), out isAdmin);
            return isExists && isAdmin;
        }

        public string Command()
        {
            return "!equip";
        }

        public string Description()
        {
            return "Command to equip a beefy set of armor";
        }

        protected BodyProperties GetBodyProperties(
      MissionPeer missionPeer,
      BasicCultureObject cultureLimit)
        {
            NetworkCommunicator networkPeer = missionPeer.GetNetworkPeer();
            if (networkPeer != null)
                return networkPeer.PlayerConnectionInfo.GetParameter<PlayerData>("PlayerData").BodyProperties;
            Debug.FailedAssert("networkCommunicator != null", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Multiplayer\\SpawnBehaviors\\SpawningBehaviors\\SpawningBehaviorBase.cs", nameof(GetBodyProperties), 366);
            Team team = missionPeer.Team;
            BasicCharacterObject troopCharacter = MultiplayerClassDivisions.GetMPHeroClasses(cultureLimit).ToList<MultiplayerClassDivisions.MPHeroClass>().GetRandomElement<MultiplayerClassDivisions.MPHeroClass>().TroopCharacter;
            MatrixFrame spawnFrame = Mission.Current.GetMissionBehavior<SpawnComponent>().GetSpawnFrame(team, troopCharacter.HasMount(), true);
            AgentBuildData agentBuildData1 = new AgentBuildData(troopCharacter).Team(team).InitialPosition(in spawnFrame.origin);
            Vec2 vec2 = spawnFrame.rotation.f.AsVec2;
            vec2 = vec2.Normalized();
            ref Vec2 local = ref vec2;
            AgentBuildData agentBuildData2 = agentBuildData1.InitialDirection(in local).TroopOrigin((IAgentOriginBase)new BasicBattleAgentOrigin(troopCharacter)).EquipmentSeed(Mission.Current.GetMissionBehavior<MissionLobbyComponent>().GetRandomFaceSeedForCharacter(troopCharacter, 0)).ClothingColor1(team.Side == BattleSideEnum.Attacker ? cultureLimit.Color : cultureLimit.ClothAlternativeColor).ClothingColor2(team.Side == BattleSideEnum.Attacker ? cultureLimit.Color2 : cultureLimit.ClothAlternativeColor2).IsFemale(troopCharacter.IsFemale);
            agentBuildData2.Equipment(Equipment.GetRandomEquipmentElements(troopCharacter, !(Game.Current.GameType is MultiplayerGame), false, agentBuildData2.AgentEquipmentSeed));
            agentBuildData2.BodyProperties(BodyProperties.GetRandomBodyProperties(agentBuildData2.AgentRace, agentBuildData2.AgentIsFemale, troopCharacter.GetBodyPropertiesMin(false), troopCharacter.GetBodyPropertiesMax(), (int)agentBuildData2.AgentOverridenSpawnEquipment.HairCoverType, agentBuildData2.AgentEquipmentSeed, troopCharacter.HairTags, troopCharacter.BeardTags, troopCharacter.TattooTags));
            return agentBuildData2.AgentBodyProperties;
        }

        protected Tuple<AgentBuildData,int> SpawnAgents(NetworkCommunicator networkPeer)
        {
            BasicCultureObject cultureLimit1 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions));
            BasicCultureObject cultureLimit2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions));

            MissionPeer component = networkPeer.GetComponent<MissionPeer>();

                IAgentVisual agentVisualForPeer = component.GetAgentVisualForPeer(0);
                BasicCultureObject basicCultureObject = component.Team.Side == BattleSideEnum.Attacker ? cultureLimit1 : cultureLimit2;
                int num = component.SelectedTroopIndex;
                IEnumerable<MultiplayerClassDivisions.MPHeroClass> mpHeroClasses = MultiplayerClassDivisions.GetMPHeroClasses(basicCultureObject);
                MultiplayerClassDivisions.MPHeroClass mpHeroClass = num < 0 ? (MultiplayerClassDivisions.MPHeroClass)null : mpHeroClasses.ElementAt<MultiplayerClassDivisions.MPHeroClass>(num);
                if (mpHeroClass == null && num < 0)
                {
                    mpHeroClass = mpHeroClasses.First<MultiplayerClassDivisions.MPHeroClass>();
                    num = 0;
                }
                BasicCharacterObject heroCharacter = mpHeroClass.HeroCharacter;
                Equipment equipment = heroCharacter.Equipment.Clone(false);
                IEnumerable<ValueTuple<EquipmentIndex, EquipmentElement>> alternativeEquipments = MPPerkObject.GetOnSpawnPerkHandler(component)?.GetAlternativeEquipments(true);
                if (alternativeEquipments != null)
                {
                    foreach (ValueTuple<EquipmentIndex, EquipmentElement> valueTuple in alternativeEquipments)
                        equipment[valueTuple.Item1] = valueTuple.Item2;
                }
                MatrixFrame matrixFrame;
                if (agentVisualForPeer == null)
                {
                            
                    matrixFrame = Mission.Current.GetMissionBehavior<SpawnComponent>().GetSpawnFrame(component.Team, heroCharacter.Equipment.Horse.Item != null, false);
                }
                else
                {
                    matrixFrame = agentVisualForPeer.GetFrame();
                    matrixFrame.rotation.MakeUnit();
                }

                AgentBuildData agentBuildData1 = new AgentBuildData(heroCharacter).MissionPeer(component).Equipment(equipment).Team(component.Team).TroopOrigin((IAgentOriginBase)new BasicBattleAgentOrigin(heroCharacter)).InitialPosition(in matrixFrame.origin);
                Vec2 vec2 = matrixFrame.rotation.f.AsVec2.Normalized();
                ref Vec2 local = ref vec2;
                return new Tuple<AgentBuildData,int>(agentBuildData1.InitialDirection(in local).IsFemale(component.Peer.IsFemale).BodyProperties(this.GetBodyProperties(component, basicCultureObject)).VisualsIndex(0).ClothingColor1(component.Team == Mission.Current.AttackerTeam ? basicCultureObject.Color : basicCultureObject.ClothAlternativeColor).ClothingColor2(component.Team == Mission.Current.AttackerTeam ? basicCultureObject.Color2 : basicCultureObject.ClothAlternativeColor2),num);
           
        }

        class OtherThread
        {
            // To successfully change the map, this thread must be called when the mission has ended
            public static void ThreadProc(Object obj)
            {
                // Give us some buffer between the OnMissionEnd event and starting the next mission
                Thread.Sleep(100);

                int agentIndex = (int)obj;
                Mission.Current.FindAgentWithIndex(agentIndex).FadeOut(true,false);
            }
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            if(networkPeer.ControlledAgent != null)
            {
                Agent old = networkPeer.ControlledAgent;
                Vec3 OriginalPos = networkPeer.ControlledAgent.Position;
                Tuple<AgentBuildData, int> retVal = SpawnAgents(networkPeer);

                BasicCharacterObject basicCharacterObject;

                AgentBuildData bda = retVal.Item1;

                Equipment e = new Equipment(networkPeer.ControlledAgent.Character.Equipment.Clone());
                Equipment ogEquip = networkPeer.ControlledAgent.Character.Equipment.Clone();
                ItemObject item = MBObjectManager.Instance.GetObject<ItemObject>("mp_plumed_lamellar_helmet");

                bda = bda.InitialPosition(OriginalPos);
                Vec3 lookDir3 = old.LookDirection;
                Vec2 lookDir = lookDir3.AsVec2;

                bda = bda.InitialDirection(lookDir);

                EquipmentElement helm = e[EquipmentIndex.Head];
                helm.CosmeticItem = item;

                e[EquipmentIndex.Head] = helm;
                bda = bda.Equipment(e);
                Mission.Current.SpawnAgent(bda);

                old.FadeOut(true, false);

                //// Spawn the original character so we can get rid of whatever BS override was occuring
                //bda = bda.Equipment(ogEquip);
            }

            return true;
        }
    }
}
