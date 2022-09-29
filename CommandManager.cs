using ChatCommands.Commands;
using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace ChatCommands
{
    class CommandManager
    {
        public static CommandManager Instance {get;set;}

        public Dictionary<string, Command> commands;


        public CommandManager() {
            if (CommandManager.Instance == null) {
                
                this.Initialize();
                CommandManager.Instance = this;
            }
        }

        public bool Execute(NetworkCommunicator networkPeer, string command, string[] args) {
            Command executableCommand; 
            bool exists = commands.TryGetValue(command, out executableCommand);
            if (!exists) {
                // networkPeer.
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("This command is not exists", false));
                GameNetwork.EndModuleEventAsServer();
                return false;
            }
            if (!executableCommand.CanUse(networkPeer)) {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("You are not authorized to run this command", false));
                GameNetwork.EndModuleEventAsServer();
                return false;
            }
            return executableCommand.Execute(networkPeer, args);
        }

        private void Initialize() {
            this.commands = new Dictionary<string, Command>();
            foreach (Type mytype in System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                 .Where(mytype => mytype.GetInterfaces().Contains(typeof(Command))))
            {
                Command command = (Command) Activator.CreateInstance(mytype);
                if (!commands.ContainsKey(command.Command())) {
                    Debug.Print("** Chat Command " + command.Command() + " have been initiated !", 0, Debug.DebugColor.Green);
                    commands.Add(command.Command(), command);
                }
            }

        }
    }
}
