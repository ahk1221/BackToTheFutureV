using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV
{
    public class Utils
    {
        public static DateTime ParseFromRawString(string raw)
        {
            if (raw.Length == 12)
            {
                var month = raw.Substring(0, 2);
                var year = raw.Substring(2, 4);
                var day = raw.Substring(6, 2);
                var hour = raw.Substring(8, 2);
                var minute = raw.Substring(10, 2);

                return new DateTime(int.Parse(year), int.Parse(month), int.Parse(day), int.Parse(hour), int.Parse(minute), 0);
            }

            return DateTime.MinValue;
        }
    }
}
