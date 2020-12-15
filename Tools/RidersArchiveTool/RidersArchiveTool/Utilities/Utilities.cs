using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RidersArchiveTool.Utilities
{
    public class Utilities
    {
        /// <summary>
        /// Rounds a number up to the next multiple unless the number is already a multiple.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="multiple">The multiple.</param>
        public static int RoundUp(int number, int multiple)
        {
            if (multiple == 0)
                return number;

            int remainder = number % multiple;
            if (remainder == 0)
                return number;

            return number + multiple - remainder;
        }
    }
}
