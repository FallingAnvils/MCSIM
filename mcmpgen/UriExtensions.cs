using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmpdl.Tools
{
    public static class UriExtensions
    {
        public static string LastElement(this Uri uri)
        {
            return uri.ToString().Split('/').Last();
        }
    }
}
