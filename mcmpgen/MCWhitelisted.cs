using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mcmpgen
{
    public class MCWhitelisted
    {
        public string uuid;
        public string name;
        public MCWhitelisted(string name, string uuid)
        {
            this.name = name;
            this.uuid = uuid;
        }
        public MCWhitelisted() { }
    }
}
