using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BackToTheFutureV.Entities;
using GTA;
using GTA.Math;
using GTA.Native;

namespace BackToTheFutureV
{
    public class VehicleInfo
    {
        public Model Model { get; set; }

        public PedInfo Driver { get; set; }

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
            EngineRunning = veh.EngineRunning;
            PrimaryColor = veh.PrimaryColor;
            SecondaryColor = veh.SecondaryColor;
            if (veh.Driver != null)
            {
                IsStationary = false;
                Driver = new PedInfo(veh.Driver);
            }
        }
    }

    public class PedInfo
    {
        public Model Model { get; set; }

        public int PedType { get; set; }

        public PedInfo(Ped ped)
        {
            Model = ped.Model;
            PedType = Function.Call<int>(Hash.GET_PED_TYPE, ped);
        }
    }

    public class TimeCircuitsInfo
    {
        public TimeCircuits Circuits { get; set; }

        public int Timer { get; set; }

        public TimeCircuitsInfo(TimeCircuits circuits)
        {
            Circuits = circuits;
        }
    }

    public class Moment
    {
        public Moment(DateTime currentDate, Weather weather, float puddleLevel, int wantedLevel, IEnumerable<VehicleInfo> infos)
        {
            CurrentDate = currentDate;
            Weather = weather;
            PuddleLevel = puddleLevel;
            WantedLevel = wantedLevel;
            Vehicles = infos.ToList();
        }

        public Weather Weather { get; set; }

        public int WantedLevel { get; set; }

        public float PuddleLevel { get; set; }

        public DateTime CurrentDate { get; set; }

        public List<VehicleInfo> Vehicles { get; set; }
    }

    public class Era
    {
        /// <summary>
        /// The start of this era in years, for e.g. 2018
        /// </summary>
        [XmlAttribute]
        public int EraStart { get; set; }

        /// <summary>
        /// The end of this era in years, for e.g. 3018
        /// </summary>
        [XmlAttribute]
        public int EraEnd { get; set; }

        /// <summary>
        /// List of the vehicles that can spawn in this Era
        /// </summary>
        public EraVehicleInfo[] SpawnableVehicles { get; set; }

        private int maxProbability = -1;

        public EraVehicleInfo GetRandomVehicle()
        {
            var maxProbability = Math.Max(GetMaxProbability(), 100);
            var randomNum = Utils.Random.NextDouble(0, maxProbability);

            var cumulative = 0;
            foreach (var vehicleInfo in SpawnableVehicles)
            {
                cumulative += vehicleInfo.Probability;

                if (randomNum < cumulative)
                    return vehicleInfo;
            }

            return null;
        }

        private int GetMaxProbability()
        {
            if (maxProbability != -1)
                return maxProbability;

            int prob = 0;

            foreach(var veh in SpawnableVehicles)
            {
                if (veh.Probability > prob)
                    prob = veh.Probability;
            }

            maxProbability = prob;
            return prob;
        }

        private static List<Era> loadedEras = new List<Era>();

        public static void LoadEraXmls(string path)
        {
            var files = Directory.GetFiles(path);

            foreach(var file in files)
            {
                var extension = Path.GetExtension(file);
                if (extension != ".xml") return; // TODO: Log to output file

                var era = Utils.DeserializeObject<Era>(file);

                if (era != null)
                    loadedEras.Add(era);
            }

            // TODO: Log in output file: xx amount of eras loaded
        }

        public static Era GetEraForCurrentTime()
        {
            var time = Utils.GetWorldTime();

            foreach(var era in loadedEras)
            {
                if(time.Year >= era.EraStart && time.Year <= era.EraEnd)
                {
                    return era;
                }
            }

            return null;
        }
    }

    public class EraVehicleInfo
    {
        /// <summary>
        /// The model of the vehicle
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Value between 0 - 100 that defines the probability of its spawn. 
        /// If 100, will spawn 100% of the time. If 0, will spawn 0% of the time
        /// </summary>
        public int Probability { get; set; }

        /// <summary>
        /// The zones in which the vehicle can spawn in
        /// Use "all" to spawn in all zones
        /// </summary>
        public string[] Zone { get; set; }
    }
}
