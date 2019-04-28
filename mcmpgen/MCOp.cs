using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mcmpgen
{
    public class MCOp
    {
        public string uuid;
        public string name;
        public int level;
        public bool bypassesPlayerLimit;
        
        public MCOp()
        {
            bypassesPlayerLimit = false;
            level = 4;
        }
        public MCOp(string name, string uuid)
        {
            this.name = name;
            this.uuid = uuid;
            bypassesPlayerLimit = false;
            level = 4;
        }
    }
}
