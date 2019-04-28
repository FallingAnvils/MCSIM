using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mcmpgen
{
    public class BatchBashGenerator
    {
        public BatchGenerator BatchGenerator;
        public BashGenerator BashGenerator;

        public BatchBashGenerator AddCommand(Command command)
        {
            BatchGenerator.Commands.Add(command);
            BashGenerator.Commands.Add(command);
            return this;
        }

        public string GenerateBash()
        {
            return BashGenerator.ToString();
        }
        
        public string GenerateBatch()
        {
            return BatchGenerator.ToString();
        }

        public BatchBashGenerator()
        {
            this.BatchGenerator = new BatchGenerator();
            this.BashGenerator = new BashGenerator();
        }
    }
}
