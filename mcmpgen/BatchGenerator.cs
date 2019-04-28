using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mcmpgen
{
    public class BatchGenerator
    {
        public List<Command> Commands;
        public override string ToString()
        {
            return string.Join("@echo off\n\n", Commands.Select(c => c.ToString()));
        }
        public BatchGenerator()
        {
            this.Commands = new List<Command>();
        }
    }
}
