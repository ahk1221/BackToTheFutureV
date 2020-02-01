using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackToTheFutureV.Entities;
using BackToTheFutureV.Handlers;
using BackToTheFutureV.Memory;
using GTA;

namespace BackToTheFutureV
{
    public class TimeHandler
    {
        private static List<Moment> momentsInTime = new List<Moment>();
        private static List<Vehicle> vehiclesEnteredByPlayer = new List<Vehicle>();
        private static Dictionary<TimeCircuits, int> timeTravelledDeloreans = new Dictionary<TimeCircuits, int>();

        private static AudioPlayer warningSound = new AudioPlayer("rc_warning.wav", false);

        public static void Tick()
        {
            if (Game.Player.Character.CurrentVehicle != null && !Game.Player.Character.CurrentVehicle.IsTimeMachine() && !vehiclesEnteredByPlayer.Contains(Game.Player.Character.CurrentVehicle))
            {
                vehiclesEnteredByPlayer.Add(Game.Player.Character.CurrentVehicle);
            }

            foreach(var entry in timeTravelledDeloreans.ToList())
            {

            }
        }

        public static void AddToTimeTravelledList(TimeCircuits circuits)
        {
            var del = circuits.Delorean.CreateCopy(true);
            timeTravelledDeloreans.Add(del.Circuits, Game.GameTime + 3000);
        }

        public static void TimeTravelTo(TimeCircuits timeCircuits, DateTime time)
        {
            // Try to find a stored moment for our time jump
            var moment = GetStoredMoment(time, 6);
            if (moment != null)
            {
                // We found a moment.
                // Apply it.
                ApplyMoment(moment);

                UI.ShowSubtitle("Found: " + World.Weather + " Puddle Level: " + RainPuddleEditor.Level);
            }
            else
            {
                // Get the current Moment object for current situation.
                moment = GetMomentForNow();

                // Clear the entered vehicles list
                vehiclesEnteredByPlayer.Clear();

                // We didn't find a moment.
                // Randomise.
                Randomize(timeCircuits);

                UI.ShowSubtitle("Randomizin: " + World.Weather + " Puddle Level: " + RainPuddleEditor.Level);

                // Add to stored Moments list.
                momentsInTime.Add(moment);
            }

            // Set previous time
            timeCircuits.PreviousTime = Utils.GetWorldTime();

            // Set the new GTA time.
            Utils.SetWorldTime(time);

            // Set the era
            TrafficInjector.UpdateEra();
        }

        public static void Randomize(TimeCircuits circuits)
        {
            // Set the weather to a random weather
            World.Weather = Utils.GetRandomWeather();

            // Initial puddle level
            float puddleLevel = 0;

            // If the weather is raining
            if (World.Weather == Weather.Raining)
            {
                // Set the puddle to a random number between 0.4 and 0.8
                puddleLevel = (float)Utils.Random.NextDouble(0.4, 0.8);
            }
            // If the weather is clearing
            else if (World.Weather == Weather.Clearing)
            {
                // Set the puddle to 0.2
                puddleLevel = 0.2f;
            }
            // If the weather is a thunderstorm
            else if (World.Weather == Weather.ThunderStorm)
            {
                // Set the puddle to 0.9f
                puddleLevel = 0.9f;
            }

            // Apply the puddle level
            RainPuddleEditor.Level = puddleLevel;

            // Reset wanted level
            Game.Player.WantedLevel = 0;

            // Delete nearby Vehicles
            var nearbyVehicles = World.GetNearbyVehicles(circuits.Vehicle.Position, 50f).ToList();

            nearbyVehicles
                .Where(x => !x.IsTimeMachine() && !vehiclesEnteredByPlayer.Contains(x)).ToList()
                .ForEach(x => x.DeleteCompletely());
        }

        public static Moment GetMomentForNow()
        {
            // Get current information
            var currentTime = Utils.GetWorldTime();
            var currentWeather = World.Weather;
            var currentPuddleLevel = RainPuddleEditor.Level;
            var currentWantedLevel = Game.Player.WantedLevel;
            var infos = vehiclesEnteredByPlayer
                .Where(x => !x.IsTimeMachine() && x.Driver == null) // needs to be stationary
                .Select(x => new VehicleInfo(x));

            // Return a new Moment instance with all the above information
            return new Moment(currentTime, currentWeather, currentPuddleLevel, currentWantedLevel, infos);
        }

        public static void ApplyMoment(Moment moment)
        {
            World.GetNearbyVehicles(Game.Player.Character.Position, 50f)
                .Where(x => !x.IsTimeMachine())
                .ToList()
                .ForEach(x => x.DeleteCompletely());

            foreach (var vehicleInfo in moment.Vehicles)
            {
                Utils.SpawnFromVehicleInfo(vehicleInfo);
            }

            if (World.Weather != moment.Weather)
                World.Weather = moment.Weather;

            RainPuddleEditor.Level = moment.PuddleLevel;

            Game.Player.WantedLevel = moment.WantedLevel;
        }

        public static Moment GetStoredMoment(DateTime currentTime, int maxHoursRange)
        {
            Moment foundMoment = null;

            foreach (var moment in momentsInTime)
            {
                var momentDate = moment.CurrentDate;
                UI.Notify(momentDate.ToString());

                // Let's advance time temporarily to see if the two times still match up.
                var currentTimeAdvanced = currentTime.AddHours(maxHoursRange);
                var momentDateAdvanced = momentDate.AddHours(maxHoursRange);
                if (momentDateAdvanced.Year != currentTimeAdvanced.Year || momentDateAdvanced.Month != currentTimeAdvanced.Month ||
                    momentDateAdvanced.Day != currentTimeAdvanced.Day) continue;

                if (currentTime.Hour >= momentDate.Hour && currentTime.Hour <= momentDate.Hour + maxHoursRange)
                {
                    foundMoment = moment;
                    break;
                }
            }

            return foundMoment;
        }
    }
}
