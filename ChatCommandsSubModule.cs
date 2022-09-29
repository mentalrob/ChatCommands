using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using HarmonyLib;
using System.Reflection;
using ChatCommands.Patches;
using System.IO;
using Newtonsoft.Json;

namespace ChatCommands
{

    

    public class ChatCommandsSubModule : MBSubModuleBase
    {
        public static ChatCommandsSubModule Instance { get; private set; }

        private void setup() {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string configPath = Path.Combine(basePath, "chatCommands.json");
            if (!File.Exists(configPath))
            {
                Config config = new Config();
                config.AdminPassword = Helpers.RandomString(6);
                ConfigManager.SetConfig(config);
                string json = JsonConvert.SerializeObject(config);
                File.WriteAllText(configPath, json);
            }
            else {
                string configString = File.ReadAllText(configPath);
                Config config = JsonConvert.DeserializeObject<Config>(configString);
                ConfigManager.SetConfig(config);
            }
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            this.setup();
            Debug.Print("** CHAT COMMANDS BY MENTALROB LOADED **", 0, Debug.DebugColor.Green);
           
            CommandManager cm = new CommandManager();
            Harmony.DEBUG = true;

            var harmony = new Harmony("mentalrob.chatcommands.bannerlord");
            // harmony.PatchAll(assembly);
            var original = typeof(ChatBox).GetMethod("ServerPrepareAndSendMessage", BindingFlags.NonPublic | BindingFlags.Instance);
            var prefix = typeof(PatchChatBox).GetMethod("Prefix");
            harmony.Patch(original, prefix: new HarmonyMethod(prefix));
        }

        protected override void OnSubModuleUnloaded() {
            Debug.Print("** CHAT COMMANDS BY MENTALROB UNLOADED **", 0, Debug.DebugColor.Green);
            // Game.OnGameCreated -= OnGameCreated;
        }


        public override void OnMultiplayerGameStart(Game game, object starterObject) {

            Debug.Print("** CHAT HANDLER ADDED **", 0, Debug.DebugColor.Green);
            game.AddGameHandler<ChatHandler>();
            // game.AddGameHandler<ManipulatedChatBox>();
            
        }
        public override void OnGameEnd(Game game) {
            game.RemoveGameHandler<ChatHandler>();
        }

    }
}
