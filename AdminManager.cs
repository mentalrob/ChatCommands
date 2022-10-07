using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;

namespace ChatCommands
{
    public class AdminManager
    {
        public static Dictionary<string, bool> Admins = new Dictionary<string, bool>();

        public static bool PlayerIsAdmin(string peerId)
        {
            if(ConfigManager.GetConfig().Admins != null)
            {
                foreach (var adminInfo in ConfigManager.GetConfig().Admins)
                {
                    string currentId = adminInfo.Split('|')[1];
                    if (peerId == currentId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
