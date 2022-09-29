using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.MountAndBlade;

namespace ChatCommands.Commands
{
    interface Command
    {
        string Command();
        bool CanUse(NetworkCommunicator networkPeer);
        bool Execute(NetworkCommunicator networkPeer, string[] args);

        string Description();
    }
}
