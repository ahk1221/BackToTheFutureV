using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackToTheFutureV.Entities;

namespace BackToTheFutureV
{
    public class RemoteDeloreanHandler
    {
        public static List<RemoteDelorean> remoteDeloreans = new List<RemoteDelorean>();
        
        public static void AddDelorean(Delorean delorean)
        {
            remoteDeloreans.Add(new RemoteDelorean(delorean));
        }
    }
}
