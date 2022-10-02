using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatCommands
{
    public class Config
    {
        public string AdminPassword { get; set; }
        public List<string> Admins { get; set; }
    }
}
