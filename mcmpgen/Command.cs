using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mcmpgen
{
    public class Command
    {
        public string FileName;
        public List<string> Args;
        public override string ToString()
        {
            return FileName + " " + string.Join(" ", Args);
        }
        public Command()
        {
            this.Args = new List<string>();
            this.FileName = string.Empty;
        }
    }
}
