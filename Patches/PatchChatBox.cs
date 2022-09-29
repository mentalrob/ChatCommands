using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace ChatCommands.Patches
{
    class PatchChatBox
    {
        public static bool Prefix(ChatBox __instance, NetworkCommunicator fromPeer, bool toTeamOnly, string message) {
            return !message.StartsWith("!");
        }
    }
}
