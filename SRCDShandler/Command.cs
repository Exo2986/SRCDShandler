using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRCDShandler
{
    public class Command
    {
        public string Identifier;
        public string RefVar;
        public Action<string[]> OnCommandRun = delegate (string[] args)
        {
            return;
        };
        public Command(string identifier)
        {
            Identifier = identifier;
        }
    }
}
