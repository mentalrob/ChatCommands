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

        public static bool PlayerIsAdmin(VirtualPlayer peer)
        {
            foreach (var adminInfo in ConfigManager.GetConfig().Admins)
            {
                string currentId = adminInfo.Split('|')[1];
                if(peer.Id.ToString() == currentId)
                {
                    return true;
                }    
            }

            return false;
        }
    }
}
