using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmpdl_wrapper
{
    class ConsolePrinter
    {
        public void Write(string da)
        {
            Console.Write(da);
        }

        public void CursorLeftZero()
        {
            Console.CursorLeft = 0;
        }
    }
}
