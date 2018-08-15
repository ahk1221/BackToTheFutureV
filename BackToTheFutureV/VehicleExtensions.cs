using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;

namespace BackToTheFutureV
{
    public static class VehicleExtensions
    {
        public static void DeleteCompletely(this Vehicle vehicle)
        {
            try
            {
                vehicle?.Driver?.Delete();
                vehicle?.Occupants?.ToList().ForEach(x => x?.Delete());
                vehicle?.Delete();
            }
            catch(Exception e)
            {

            }
        }
    }
}
