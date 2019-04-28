using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmpdl.Tools
{
    public static class StringExtensions
    {
        public static Uri AsUri(this string str)
        {
            return new Uri(str);
        }
    }
}
