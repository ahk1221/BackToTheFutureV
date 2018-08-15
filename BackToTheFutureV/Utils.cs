using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV
{
    public enum MapArea
    {
        County = 2072609373,
        City = -289320599
    }

    public class Utils
    {
        public static Random Random = new Random();

        private static readonly Weather[] validWeatherTypes = new Weather[]
        {
            Weather.Clear,
            Weather.Clearing,
            Weather.Clouds,
            Weather.ExtraSunny,
            Weather.Foggy,
            Weather.Overcast,
            Weather.Raining,
            Weather.ThunderStorm
        };

        private static readonly VehicleHash[] cityVehicles =
        {
            VehicleHash.Mule,
            VehicleHash.Mule2,
            VehicleHash.Mule3,
            VehicleHash.Blista,
            VehicleHash.Blista2,
            VehicleHash.Brioso,
            VehicleHash.Dilettante,
            VehicleHash.Issi2,
            VehicleHash.Prairie,
            VehicleHash.Rhapsody,
            VehicleHash.Exemplar,
            VehicleHash.Felon,
            VehicleHash.Sentinel,
            VehicleHash.Sentinel2,
            VehicleHash.Zion,
            VehicleHash.Zion2,
            VehicleHash.Ambulance,
            VehicleHash.Police2,
            VehicleHash.Police3,
            VehicleHash.Blade,
            VehicleHash.Dominator,
            VehicleHash.Ruiner,
            VehicleHash.Coach,
            VehicleHash.Airbus,
            VehicleHash.Bus,
            VehicleHash.Tailgater,
            VehicleHash.Warrener,
            VehicleHash.Washington,
            VehicleHash.Stratum,
            VehicleHash.Romero,
        };

        private static readonly VehicleHash[] countryVehicles =
        {
            VehicleHash.Rebel,
            VehicleHash.Rebel2,
            VehicleHash.Mesa,
            VehicleHash.Dune,
            VehicleHash.BfInjection,
            VehicleHash.Police,
            VehicleHash.Police4,
            VehicleHash.Habanero,
            VehicleHash.Rocoto,
            VehicleHash.Asterope,
            VehicleHash.Emperor2,
            VehicleHash.Glendale,
            VehicleHash.Regina,
            VehicleHash.Peyote,
            VehicleHash.Tornado,
            VehicleHash.Tornado3,
            VehicleHash.Tornado4,
            VehicleHash.TowTruck2,
            VehicleHash.Scrap,
            VehicleHash.Surfer2,
            VehicleHash.Surfer,
        };

        public static DateTime ParseFromRawString(string raw)
        {
            if (raw.Length == 12)
            {
                var month = raw.Substring(0, 2);
                var year = raw.Substring(2, 4);
                var day = raw.Substring(6, 2);
                var hour = raw.Substring(8, 2);
                var minute = raw.Substring(10, 2);

                UI.ShowSubtitle("Month: " + month + " Year: " + year + " Day: " + day + " Hour: " + hour + " Minute: " + minute);

                return new DateTime(int.Parse(year), int.Parse(month), int.Parse(day), int.Parse(hour), int.Parse(minute), 0);
            }

            return DateTime.MinValue;
        }

        public static Model GetRandomVehicleHash(Model model, Vector3 position)
        {
            var areaType = (MapArea)Function.Call<int>(Hash.GET_HASH_OF_MAP_AREA_AT_COORDS, position.X, position.Y, position.Z);
            switch (areaType)
            {
                case MapArea.City:
                    return GetRandomVehicle(cityVehicles, model);
                case MapArea.County:
                    return GetRandomVehicle(countryVehicles, model);
            }
            return VehicleHash.Premier;
        }

        private static Model GetRandomVehicle(VehicleHash[] array, Model model, float maxDiff = 0.6f)
        {
            var validVehicles = array.Where(x => Difference(x, model) < 0.6f).ToArray();

            if(validVehicles == null || validVehicles.Length <= 0)
            {
                return VehicleHash.Premier;
            }

            return validVehicles[Random.Next(validVehicles.Length)];
        }

        public static float Difference(Model model, Model otherModel)
        {
            return Math.Abs(GetRadiusOfModel(model) - GetRadiusOfModel(otherModel)); 
        }

        public static float GetRadiusOfModel(Model model)
        {
            // Define variables
            Vector3 min, max;

            // Get the dimensions
            model.GetDimensions(out min, out max);

            // Return the output
            return (max - min).Length() / 2;
        }

        public static Vehicle SpawnFromVehicleInfo(VehicleInfo vehicleInfo, bool markAsNoLongerNeeded = true)
        {
            var vehicle = World.CreateVehicle(vehicleInfo.Model, vehicleInfo.Position);
            vehicle.Rotation = vehicleInfo.Rotation;
            vehicle.Velocity = vehicleInfo.Velocity;
            vehicle.Speed = vehicleInfo.Speed;
            vehicle.PrimaryColor = vehicleInfo.PrimaryColor;
            vehicle.SecondaryColor = vehicleInfo.SecondaryColor;
            vehicle.EngineRunning = vehicleInfo.EngineRunning;

            if (!vehicleInfo.IsStationary)
                vehicle.CreatePedOnSeat(VehicleSeat.Driver, vehicleInfo.DriverModel);

            var driver = vehicle.Driver;
            if (driver != null)
            {
                driver.Task.CruiseWithVehicle(vehicle, 11, (int)DrivingStyle.Normal);
            }

            vehicle.IsPersistent = false;

            if(markAsNoLongerNeeded)
                vehicle.MarkAsNoLongerNeeded();

            return vehicle;
        }

        public static Weather GetRandomWeather()
        {
            var num = Random.Next(0, validWeatherTypes.Length);

            return validWeatherTypes[num];
        }

        public static DateTime GetWorldTime()
        {
            return World.CurrentDate;
        }

        public static void SetWorldTime(DateTime time)
        {
            Function.Call(Hash.SET_CLOCK_DATE, time.Day, time.Month, time.Year);
            Function.Call(Hash.SET_CLOCK_TIME, time.Hour, time.Minute, time.Second);
        }
    }

    public static class DrawingUtils
    { 
        public static PointF GetPoint(PointF orig)
        {
            var res = Game.ScreenResolution;
            var offset = GetOffsetForAspectRatio(GetAspectRatio());

            var newPoint = new PointF(orig.X / res.Width + offset, orig.Y / res.Height + offset);

            return newPoint;
        }

        public static SizeF GetSize(SizeF size)
        {
            var res = Game.ScreenResolution;

            var newPoint = new SizeF(size.Width / res.Width, size.Height / res.Height);

            return newPoint;
        }

        public static float GetOffsetForAspectRatio(float aspect)
        {
            var zone = Function.Call<float>(Hash.GET_SAFE_ZONE_SIZE);
            var safeZone = 1 - zone;
            safeZone *= 0.5f;
            if (aspect <= 1.77777777778f)
                return safeZone;
            var o = 1f - 1.7777777910232544f / aspect;
            o *= 0.5f;
            return o + safeZone;
        }

        public static float GetAspectRatio()
        {
            return Function.Call<float>(Hash._0xF1307EF624A80D87, 1); // _GET_ASPECT_RATIO
        }
    }
}
