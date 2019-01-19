using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StNetease
{
    public static class StringExtend
    {
        public static string ToBase64(this string @this)
        {
            byte[] bys = Encoding.UTF8.GetBytes(@this);
            return Convert.ToBase64String(bys);
        }
        public static string FromBase64(this string @this)
        {
            byte[] bys = Convert.FromBase64String(@this);
            return Encoding.UTF8.GetString(bys);
        }
    }
}
