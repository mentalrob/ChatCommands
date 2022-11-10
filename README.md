# Chat Commands

Chat commands is a server side mod that allow players to use chat commands

# Requirements For Development

- Harmony for .NET Framework 4.7.2

`0Harmony.dll is located in release file, If you are not going to make development you can ignore this requirement.`

## Installation

- Download the latest release (It's located on right panel)
- Add the following xml node to your `[Dedicated Server Files]/Modules/Multiplayer/SubModule.xml` file, between the  `<SubModules> </SubModules>` tags. 
```xml
<SubModule>
    <Name value="ChatCommands" />
    <DLLName value="ChatCommands.dll" />
    <SubModuleClassType value="ChatCommands.ChatCommandsSubModule" />
    <Tags>
        <Tag key="DedicatedServerType" value="custom" />
    </Tags>
</SubModule>
```
- Your SubModule.xml file should look like this
```xml
<?xml version="1.0" encoding="utf-8"?>
<Module>
	<Name value="Multiplayer" />
	<Id value="Multiplayer" />
	<Version value="e1.8.0" />
	<DefaultModule value="true" />
	<ModuleCategory value="Multiplayer" />
	<Official value="true" />
	<DependedModules>
		<DependedModule Id="Native" DependentVersion="e1.8.0" Optional="false" />
	</DependedModules>
	<SubModules>
        <!-- Added here -->
        <SubModule>
            <Name value="ChatCommands" />
            <DLLName value="ChatCommands.dll" />
            <SubModuleClassType value="ChatCommands.ChatCommandsSubModule" />
            <Tags>
                <Tag key="DedicatedServerType" value="custom" />
            </Tags>
        </SubModule>
        <!-- Your other submodules -->
	</SubModules>
</Module>
```

## Usage

While in the server you can open up the chat and type `!help` command to see what commands you can use.

### User Commands
- `!help` shows the help message
- `!login <password>` Logins as an administrator
- `!me <message>` Me command that everyone knows...

### Admin Commands
- `!ban` Bans a player. Caution ! First user that contains the provided input will be banned. Usage `!ban <Player Name>`
- `!fade` Evaporate a player. Usage `!fade <Player name>`
- `!godmode` Ascend yourself. Be something selestial
- `!gold` Set gold to a player. Usage `!gold <Player Name> <amount>`
- `!kick` Kicks a player. Caution ! First user that contains the provided input will be kicked. Usage `!kick <Player Name>`
- `!kill` Kills a provided username. Usage `!kill <Player Name>`
- `!tp` Teleport yourself to another. Usage `!tp <Target User>`
- `!tptome` Teleport player to you. Usage `!tptome <Target User>`
- `!unban` Unbans a player. Usage `!unban <Player Name>`
- `!maps` Lists available maps for the current, or a different, game type. `!maps <game type>`
- `!changemap` Changes the map. Use !maps to see available map IDs. `!chagemap <map id>`
- `!mapfacs` Changes the map and the team factions. `!chagemapfacs <map id> <team1 faction> <team2 faction>`
- `!mission` Changes the game type, map, and factions. `!mission <game type> <map id> <team1 faction> <team2 faction>`
- `!bots` Changes the number of bots for each factions. `!bots <team1 bots> <team2 bots>`
- `!id` Returns your unique ID `!id`
- `!healme` Healing yourself `!healme`
- `!heal` Healing a player `!heal <Player Name>`
- `!healall` Heal all players `!healall`

Login password is randomly generated, you can find the password under `bin/Win64_ServerShipping/chatCommands.json` and banlist is also in the same folder. 

## I want to create my own command.

Create a class that implements `Command` interface. And voila you have your own command. At the server startup it's automagically detects the `Command` implemented classes and appends it to registry. Lets examine an example.

```csharp
namespace ChatCommands.Commands
{
    class GodMode : Command // This is our class that implements `Command` interface.
    {
        // Here you can define who can use this command.
        public bool CanUse(NetworkCommunicator networkPeer) 
        {
            bool isAdmin = false;
            // Check if the user is admin
            bool isExists = AdminManager.Admins.TryGetValue(networkPeer.VirtualPlayer.Id.ToString(), out isAdmin);
            return isExists && isAdmin;
        }
        
        // Define the command here. Note that mod only filters command that starts with `!`
        public string Command()
        {
            return "!godmode"; 
        }

        // Your command's description.
        public string Description()
        {
            return "Ascend yourself. Be something selestial"; 
        }

        // Execution phase of the command.
        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            // networkPeer: issuer of the command
            // args: arguments of the command.
            // If a user types `!godmode blabla` `args[0]` will be `blabla`
            if (networkPeer.ControlledAgent != null) {
                networkPeer.ControlledAgent.BaseHealthLimit = 2000;
                networkPeer.ControlledAgent.HealthLimit = 2000;
                networkPeer.ControlledAgent.Health = 2000;
                networkPeer.ControlledAgent.SetMinimumSpeed(10);
                networkPeer.ControlledAgent.SetMaximumSpeedLimit(10, false);
                
            }
            return true;
        }
    }
}
```

For more example you can check other commands.

If you want to contribute you can, if you want me to implement a command, please open up an issue.

## License
[MIT](https://choosealicense.com/licenses/mit/)

# Thanks to
- Horns
- Falcomfr
