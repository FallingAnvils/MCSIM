using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mcmpgen
{
    public class BashGenerator
    {
        public List<Command> Commands;
        public override string ToString()
        {
            return "#!/bin/bash\n\n" + string.Join("\n", Commands.Select(c => c.ToString()));
        }
        public BashGenerator()
        {
            this.Commands = new List<Command>();
        }
    }
}
