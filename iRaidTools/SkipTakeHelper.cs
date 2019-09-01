using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidTools
{
    public static class TakeHelper
    {
        /// <summary>
        /// By default take can only between 1 and 100, used for pagination when returning data through API's.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static int TakeDefault(this int num)
        {
            if (num < 1)
                return 1;

            if (num > 100)
                return 100;

            return num;
        }

        /// <summary>
        /// Pagination helper.
        /// </summary>
        /// <param name="num"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public static int PageHelper(this int num, int take)
        {
            return (num == 1 || num == 0) ? 0 : num * take.TakeDefault();
        }
    }
}
