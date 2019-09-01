using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace iRaidTools
{
    public static class StringHelper
    {
        public static string ToJson(dynamic data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public static bool IsNullOrEmpty(this string str)
        {
            if(str == null || str == string.Empty)
            {
                return true;
            }

            return false;
        }
    }
}
