using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

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

        public static Vehicle ReplaceVehicle(Vehicle vehicle, Model newHash, bool markAsNoLongerNeeded = true)
        {
            // Get the info for the original vehicle
            VehicleInfo info = new VehicleInfo(vehicle);
            info.Model = newHash;

            // Get the ped out of the vehicle
            Ped driver = vehicle.Driver;
            driver?.Task.WarpOutOfVehicle(vehicle);

            // Delete original vehicle
            vehicle.DeleteCompletely();

            // Spawn the new vehicle
            Vehicle spawnedVehicle = SpawnFromVehicleInfo(info);
            
            // This driving style means: (https://gtaforums.com/topic/822314-guide-driving-styles/)
            // 1 = Stop Before Vehicles
            // 2 = Stop Before Peds
            // 8 = Avoid Empty Vehicles
            // 16 = Avoid Peds
            // 32 = Avoid Objects
            // 128 = Stop at Traffic Lights
            // 256 = Use blinkers
            int drivingStyle = 1 + 2 + 8 + 16 + 32 + 128 + 256;

            // Set the ped inside the vehicle
            driver?.SetIntoVehicle(vehicle, VehicleSeat.Driver);
            driver?.Task.CruiseWithVehicle(spawnedVehicle, 20, drivingStyle);

            // Mark the vehicle as no longer needed to save memory
            if(markAsNoLongerNeeded)
            {
                spawnedVehicle.MarkAsNoLongerNeeded();
                spawnedVehicle.Model.MarkAsNoLongerNeeded();
            }

            // Return the spawned vehicle
            return spawnedVehicle;
        }

        public static Vehicle SpawnFromVehicleInfo(VehicleInfo vehicleInfo)
        {
            Vehicle vehicle = World.CreateVehicle(vehicleInfo.Model.Hash, vehicleInfo.Position);
            vehicle.Rotation = vehicleInfo.Rotation;
            vehicle.Velocity = vehicleInfo.Velocity;
            vehicle.Speed = vehicleInfo.Speed;
            vehicle.PrimaryColor = vehicleInfo.PrimaryColor;
            vehicle.SecondaryColor = vehicleInfo.SecondaryColor;
            vehicle.EngineRunning = vehicleInfo.EngineRunning;
            vehicle.IsPersistent = false;
            vehicle.MarkAsNoLongerNeeded(); 
            vehicle.Model.MarkAsNoLongerNeeded();

            Ped driver = null;

            if (!vehicleInfo.IsStationary)
            {
                driver = Function.Call<Ped>(Hash.CREATE_PED_INSIDE_VEHICLE, vehicle, vehicleInfo.Driver.PedType,
                    vehicleInfo.Driver.Model.Hash, (int)VehicleSeat.Driver, false, true);
                driver.MarkAsNoLongerNeeded();
                driver.Model.MarkAsNoLongerNeeded();
            }

            if (driver != null)
            {
                int drivingStyle = 1 + 2 + 8 + 16 + 32 + 128 + 256;
                driver.Task.CruiseWithVehicle(vehicle, 20, drivingStyle);
            }

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

        public static void DisplayHelpText(string text)
        {
            Function.Call(Hash._SET_TEXT_COMPONENT_FORMAT, "STRING");
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, text);
            Function.Call(Hash._0x238FFE5C7B0498A6, 0, 0, 1, -1);
        }

        public static void HideVehicle(Vehicle vehicle, bool hide)
        {
            vehicle.IsVisible = !hide;
            vehicle.HasCollision = !hide;
            vehicle.FreezePosition = hide;
            vehicle.EngineRunning = !hide;

            if (vehicle.Driver != null)
                vehicle.Driver.IsVisible = !hide;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value > max)
                return max;
            else if (value < min)
                return min;

            return value;
        }

        public static float CalculateVolume(Entity ent)
        {
            var distance = Vector3.Distance(Game.Player.Character.Position, ent.Position);

            var volume = 25f / distance;

            return Clamp(volume, 0, 1.5f);
        }

        public static void SetDecorator<T>(Entity entity, string propertyName, T value)
        {
            Type type = typeof(T);

            if (type == typeof(int))
                Function.Call(Hash.DECOR_SET_INT, entity.Handle, propertyName, (int)Convert.ChangeType(value, typeof(int)));
            else if (type == typeof(float))
                Function.Call(Hash._DECOR_SET_FLOAT, entity.Handle, propertyName, (float)Convert.ChangeType(value, typeof(float)));
            else if (type == typeof(bool))
                Function.Call(Hash.DECOR_SET_BOOL, entity.Handle, propertyName, (bool)Convert.ChangeType(value, typeof(bool)));
            else
                throw new Exception("SetDecorator provided with invalid type: " + type);
        }

        public static T GetDecorator<T>(Entity entity, string propertyName)
        {
            Type type = typeof(T);

            if (type == typeof(int))
                return (T)Convert.ChangeType(Function.Call<int>(Hash.DECOR_GET_INT, entity.Handle, propertyName), type);
            else if (type == typeof(float))
                return (T)Convert.ChangeType(Function.Call<float>(Hash._DECOR_GET_FLOAT, entity.Handle, propertyName), type);
            else if (type == typeof(bool))
                return (T)Convert.ChangeType(Function.Call<bool>(Hash.DECOR_GET_BOOL, entity.Handle, propertyName), type);
            else
                throw new Exception("GetDecorator provided with invalid type: " + type);

        }

        // Thanks to sollaholla for this
        public static float CalculateStereo(Entity ent)
        {
            Vector3 lDir = Quaternion.Euler(GameplayCamera.Rotation) * Vector3.RelativeRight;
            Vector3 rDir = -lDir;
            Vector3 relativeDir = ent.Position - GameplayCamera.Position;
            relativeDir.Normalize();

            float lf = Vector3.Angle(lDir, relativeDir);
            float volumeL = lf / 180f;
            float rf = Vector3.Angle(rDir, relativeDir);
            float volumeR = rf / 180f;

            return volumeR - volumeL;
        }

        public static bool SerializeObject<T>(T obj, string path)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                TextWriter stream = new StreamWriter(path);

                serializer.Serialize(stream, obj);
                stream.Close();

                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        public static T DeserializeObject<T>(string path)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                FileStream fs = new FileStream(path, FileMode.Open);

                return (T)serializer.Deserialize(fs);
            }
            catch(Exception e)
            {
                return default(T);
            }
        }

        public static float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat + (secondFloat - firstFloat) * by;
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
