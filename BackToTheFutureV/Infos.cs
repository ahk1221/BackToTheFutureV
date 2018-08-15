using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;

namespace BackToTheFutureV
{
    public class VehicleInfo
    {
        public Model Model { get; set; }

        public Model DriverModel { get; set; }

        public Vector3 Velocity { get; set; }

        public Vector3 Position { get; set; }

        public Vector3 Rotation { get; set; }

        public float Speed { get; set; }

        public bool IsStationary { get; set; }

        public bool EngineRunning { get; set; }

        public VehicleColor PrimaryColor { get; set; }

        public VehicleColor SecondaryColor { get; set; }

        public VehicleInfo(Vehicle veh)
        {
            Model = veh.Model;
            Velocity = veh.Velocity;
            Position = veh.Position;
            Rotation = veh.Rotation;
            Speed = veh.Speed;
            IsStationary = veh.Driver == null;
            EngineRunning = veh.EngineRunning;
            PrimaryColor = veh.PrimaryColor;
            SecondaryColor = veh.SecondaryColor;
            DriverModel = veh.Driver?.Model ?? PedHash.Abigail;
        }
    }

    public class PedInfo
    {
        public Model Model;

        public PedInfo(Ped ped)
        {
            Model = ped.Model;
        }
    }
}
