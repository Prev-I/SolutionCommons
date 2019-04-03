using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Solution.Core.Extensions
{
    public static class StaticExt
    {
        ///<summary>
        ///Verify that an object expose a .ToString() method
        ///</summary>
        public static bool IsValid(this object val)
        {
            if (val == null) return false;
            if (val.ToString() == string.Empty) return false;
            return true;
        }
    }
}
