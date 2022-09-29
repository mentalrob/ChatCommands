using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace ChatCommands.Commands
{
    class Kill : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            bool isAdmin = false;
            bool isExists = AdminManager.Admins.TryGetValue(networkPeer.VirtualPlayer.Id.ToString(), out isAdmin);
            return isExists && isAdmin;
        }

        public string Command()
        {
            return "!kill";
        }

        public string Description()
        {
            return "Kills a provided username. Usage !kill <Player Name>";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            if (args.Length == 0)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Please provide a username. Player that contains provided input will be killed"));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }

            NetworkCommunicator targetPeer = null;
            foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
            {
                if (peer.UserName.Contains(string.Join(" ", args)))
                {
                    targetPeer = peer;
                    break;
                }
            }
            if (targetPeer == null)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Target player not found"));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }

            if (!targetPeer.ControlledAgent.Equals(null)) {
                Agent agent = targetPeer.ControlledAgent;
                Blow blow = new Blow(agent.Index);
                blow.DamageType = TaleWorlds.Core.DamageTypes.Pierce;
                blow.BoneIndex = agent.Monster.HeadLookDirectionBoneIndex;
                blow.Position = agent.Position;
                blow.Position.z = blow.Position.z + agent.GetEyeGlobalHeight();
                blow.BaseMagnitude = 2000f;
                blow.WeaponRecord.FillAsMeleeBlow(null, null, -1, -1);
                blow.InflictedDamage = 2000;
                blow.SwingDirection = agent.LookDirection;
                MatrixFrame frame = agent.Frame;
                blow.SwingDirection = frame.rotation.TransformToParent(new Vec3(-1f, 0f, 0f, -1f));
                blow.SwingDirection.Normalize();
                blow.Direction = blow.SwingDirection;
                blow.DamageCalculated = true;
                sbyte mainHandItemBoneIndex = agent.Monster.MainHandItemBoneIndex;
                AttackCollisionData attackCollisionDataForDebugPurpose = AttackCollisionData.GetAttackCollisionDataForDebugPurpose(false, false, false, true, false, false, false, false, false, false, false, false, CombatCollisionResult.StrikeAgent, -1, 0, 2, blow.BoneIndex, BoneBodyPartType.Head, mainHandItemBoneIndex, Agent.UsageDirection.AttackLeft, -1, CombatHitResultFlags.NormalHit, 0.5f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, Vec3.Up, blow.Direction, blow.Position, Vec3.Zero, Vec3.Zero, agent.Velocity, Vec3.Up);
                agent.RegisterBlow(blow, attackCollisionDataForDebugPurpose);
            }

            return true;
        }
    }
}
