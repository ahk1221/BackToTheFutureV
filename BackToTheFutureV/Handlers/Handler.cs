using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackToTheFutureV.Entities;
using GTA;

namespace BackToTheFutureV.Handlers
{
    public abstract class Handler
    {
        public TimeCircuits TimeCircuits { get; }

        public Vehicle Vehicle => TimeCircuits.Vehicle;

        public DateTime DestinationTime { get => TimeCircuits.DestinationTime; set => TimeCircuits.DestinationTime = value; }
        public DateTime PreviousTime { get => TimeCircuits.PreviousTime; set => TimeCircuits.PreviousTime = value; }
        public bool IsTimeCircuitsEnabled { get => TimeCircuits.IsOn; set => TimeCircuits.IsOn = value; }
        public float MPHSpeed { get => TimeCircuits.MPHSpeed; set => TimeCircuits.MPHSpeed = value; }

        public Handler(TimeCircuits circuits)
        {
            TimeCircuits = circuits;
        }

        public abstract void Process();
        public abstract void Stop();
    }
}
